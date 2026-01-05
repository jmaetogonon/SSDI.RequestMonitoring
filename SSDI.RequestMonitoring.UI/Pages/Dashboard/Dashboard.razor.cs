using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Pages.Dashboard.Dto;

namespace SSDI.RequestMonitoring.UI.Pages.Dashboard;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    private string activeTab = "all";
    private int currentSlide = 0;
    private ElementReference carouselContainer;
    private DotNetObjectReference<Dashboard>? dotNetRef;
    private IJSObjectReference? jsModule;

    private RequestMetrics purchaseRequestMetrics = new();
    private RequestMetrics jobOrderMetrics = new();

    private List<Purchase_RequestVM> purchaseRequests = [];
    private List<Job_OrderVM> jobOrders = [];

    private List<ActivityItem> recentActivity = [];
    private List<TeamRequestItem> teamRequestsNeedingAttention = [];
    private List<ApprovedRequestItem> approveReqyestItems = [];

    private int totalDraftCount = 0;
    private int totalPendingSubmissionCount = 0;

    private bool isLoading = true;
    private RequestType requestType = RequestType.All;

    private Confirmation__Modal? confirmModal;

    protected override async Task OnInitializedAsync()
    {
        await currentUser.InitializeAsync();
        await LoadDashboardData();
        isLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Create a reference to this component for JavaScript to call
            dotNetRef = DotNetObjectReference.Create(this);

            // Initialize the carousel with JavaScript
            await InitializeCarousel();
        }
    }

    private void SwitchTab(string tab)
    {
        activeTab = tab;
        currentSlide = 0; // Reset to first slide
        requestType = tab switch
        {
            "purchase" => RequestType.Purchase,
            "joborder" => RequestType.JobOrder,
            _ => RequestType.All
        };

        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        // Clean up JavaScript references
        if (jsModule != null)
        {
            await jsModule.DisposeAsync();
        }

        // Clean up global reference
        await jsRuntime.InvokeVoidAsync("eval",
            "if (window.__dashboardDotNetHelper) { window.__dashboardDotNetHelper = null; }");

        dotNetRef?.Dispose();
    }

    private async Task LoadDashboardData()
    {
        try
        {
            // Load both request types in parallel
            var purchaseTask = LoadPurchaseRequestData();
            var jobOrderTask = LoadJobOrderData();

            await Task.WhenAll(purchaseTask, jobOrderTask);

            totalDraftCount = purchaseRequestMetrics.DraftCount + jobOrderMetrics.DraftCount;
            totalPendingSubmissionCount = purchaseRequestMetrics.PendingSubmissionCount + jobOrderMetrics.PendingSubmissionCount;

            // Combine recent activity
            recentActivity = purchaseRequestMetrics.RecentActivity
                .Concat(jobOrderMetrics.RecentActivity)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();

            // Load team requests for supervisors
            if (utils.IsSupervisor())
            {
                await LoadTeamRequests();
                await LoadApprovedRequests();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            toastSvc.ShowError("Failed to load dashboard data");
        }
    }

    private async Task LoadPurchaseRequestData()
    {
        try
        {
            if (utils.IsUser())
            {
                purchaseRequests = await purchaseRequestSvc.GetAllPurchaseRequestsByUser(currentUser.UserId);
            }
            else
            {
                purchaseRequests = await purchaseRequestSvc.GetAllPurchaseReqBySupervisor(currentUser.UserId, true, true);

                if (utils.IsCEO())
                {
                    var ceoRequests = await purchaseRequestSvc.GetAllPurchaseReqByCeo();
                    purchaseRequests = purchaseRequests.Concat(ceoRequests).DistinctBy(e => e.Id).ToList();
                }

                if (utils.IsAdmin())
                {
                    var adminRequests = await purchaseRequestSvc.GetAllPurchaseRequestsByAdmin();
                    purchaseRequests = purchaseRequests.Concat(adminRequests).DistinctBy(r => r.Id).ToList();
                }
            }

            purchaseRequestMetrics = CalculateMetrics(purchaseRequests, "purchase");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading purchase requests: {ex.Message}");
            purchaseRequestMetrics = new RequestMetrics();
        }
    }

    private async Task LoadJobOrderData()
    {
        try
        {
            if (utils.IsUser())
            {
                jobOrders = await jobOrderSvc.GetAllJobOrdersByUser(currentUser.UserId);
            }
            else
            {
                jobOrders = await jobOrderSvc.GetAllJobOrderBySupervisor(currentUser.UserId, true, true);

                if (utils.IsCEO())
                {
                    var ceoRequests = await jobOrderSvc.GetAllJobOrderByCeo();
                    jobOrders = jobOrders.Concat(ceoRequests).DistinctBy(e => e.Id).ToList();
                }

                if (utils.IsAdmin())
                {
                    var adminRequests = await jobOrderSvc.GetAllJobOrdersByAdmin();
                    jobOrders = jobOrders.Concat(adminRequests).DistinctBy(r => r.Id).ToList();
                }
            }

            jobOrderMetrics = CalculateMetrics(jobOrders, "joborder");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading job orders: {ex.Message}");
            jobOrderMetrics = new RequestMetrics();
        }
    }

    private RequestMetrics CalculateMetrics<T>(List<T> requests, string requestType) where T : IRequestDetailVM
    {
        var metrics = new RequestMetrics();
        metrics.TotalCount = requests.Count;

        // Calculate status counts
        metrics.DraftCount = requests.Count(r => r.Status == RequestStatus.Draft);
        metrics.PendingSubmissionCount = requests.Count(r => r.Status == RequestStatus.Draft);
        metrics.PendingEndorsementCount = requests.Count(r => r.Status == RequestStatus.ForEndorsement);
        metrics.PendingAdminApprovalCount = requests.Count(r => r.Status == RequestStatus.ForAdminVerification);
        metrics.PendingAdminRequisitionCount = requests.Count(r => r.Status == RequestStatus.ForRequisition);
        metrics.PendingCEOApprovalCount = requests.Count(r => r.Status == RequestStatus.ForCeoApproval);
        metrics.PendingClosureCount = requests.Count(r => r.Status == RequestStatus.PendingRequesterClosure);
        metrics.AllPendingCount = requests.Count(r => r.Status != RequestStatus.Closed && r.Status != RequestStatus.Cancelled);
        metrics.CompletedCount = requests.Count(r =>
            r.Status == RequestStatus.Closed);
        metrics.RejectedCount = requests.Count(r => r.Status == RequestStatus.Rejected);

        // Recent activity
        metrics.RecentActivity = requests
            .OrderByDescending(r => r.DateModified)
            .Take(5)
            .Select(r => new ActivityItem
            {
                Id = r.Id,
                RequestType = requestType,
                Title = r.Nature_Of_Request,
                Status = r.Status,
                Date = r.DateModified ?? DateTime.Now
            })
            .ToList();

        return metrics;
    }

    private async Task LoadTeamRequests()
    {
        try
        {
            await Task.Delay(100);
            List<Purchase_RequestVM> purchaseTeamRequests = [];
            List<Job_OrderVM> jobOrderTeamRequests = [];

            if (purchaseRequests.Any(e => e.ReportType == "Department")) purchaseTeamRequests = [.. purchaseRequests.Where(e => e.Status == RequestStatus.Draft || e.Status == RequestStatus.Rejected)];
            if (jobOrders.Any(e => e.ReportType == "Department")) jobOrderTeamRequests = [.. jobOrders.Where(e => e.Status == RequestStatus.Draft || e.Status == RequestStatus.Rejected)];

            if (purchaseRequests.Any(e => e.ReportType == "Division")) purchaseTeamRequests = [.. purchaseRequests.Where(e => e.Status == RequestStatus.ForEndorsement)];
            if (jobOrders.Any(e => e.ReportType == "Division")) jobOrderTeamRequests = [.. jobOrders.Where(e => e.Status == RequestStatus.ForEndorsement)];

            if (utils.IsCEO())
            {
                var ceoPRequests = purchaseTeamRequests.Concat(purchaseRequests.Where(e => e.Status == RequestStatus.ForCeoApproval)).ToList();
                var ceoJORequests = jobOrderTeamRequests.Concat(jobOrders.Where(e => e.Status == RequestStatus.ForCeoApproval)).ToList();
                purchaseTeamRequests = ceoPRequests;
                jobOrderTeamRequests = ceoJORequests;
            }

            if (utils.IsAdmin())
            {
                var adminPRequests = purchaseTeamRequests.Concat(purchaseRequests.Where(e => e.Status == RequestStatus.ForAdminVerification || e.Status == RequestStatus.ForRequisition)).ToList();
                var adminJORequests = jobOrderTeamRequests.Concat(jobOrders.Where(e => e.Status == RequestStatus.ForAdminVerification || e.Status == RequestStatus.ForRequisition)).ToList();
                purchaseTeamRequests = adminPRequests;
                jobOrderTeamRequests = adminJORequests;
            }

            var allTeamRequests = purchaseTeamRequests
                .Select(r => new TeamRequestItem
                {
                    Id = r.Id,
                    RequestType = "purchase",
                    Title = r.Nature_Of_Request,
                    RequestedBy = r.Name,
                    Status = r.Status,
                    Date = r.DateModified ?? DateTime.Now,
                    Priority = r.Priority,
                    ReportType = r.ReportType,
                })
                .Concat(jobOrderTeamRequests
                    .Select(r => new TeamRequestItem
                    {
                        Id = r.Id,
                        RequestType = "joborder",
                        Title = r.Nature_Of_Request,
                        RequestedBy = r.Name,
                        Status = r.Status,
                        Date = r.DateModified ?? DateTime.Now,
                        Priority = r.Priority,
                        ReportType = r.ReportType,
                    }))
                .OrderByDescending(r => r.Date)
                .Take(10)
                .ToList();

            teamRequestsNeedingAttention = allTeamRequests;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading team requests: {ex.Message}");
            teamRequestsNeedingAttention = [];
        }
    }

    private async Task LoadApprovedRequests()
    {
        try
        {
            await Task.Delay(100);

            var allRequests = purchaseRequests
                .Select(r => new ApprovedRequestItem
                {
                    Id = r.Id,
                    RequestType = "purchase",
                    Title = r.Nature_Of_Request,
                    RequestedBy = r.Name,
                    Status = r.Status,
                    Date = r.DateModified ?? DateTime.Now,
                    Priority = r.Priority,
                    ReportType = r.ReportType,
                    PendingSince = GetPendingSince(r.Id, "purchase")
                })
                .Concat(jobOrders
                    .Select(r => new ApprovedRequestItem
                    {
                        Id = r.Id,
                        RequestType = "joborder",
                        Title = r.Nature_Of_Request,
                        RequestedBy = r.Name,
                        Status = r.Status,
                        Date = r.DateModified ?? DateTime.Now,
                        Priority = r.Priority,
                        ReportType = r.ReportType,
                        PendingSince = GetPendingSince(r.Id, "joboder")
                    }))
                .OrderByDescending(r => r.PendingSince)
                .ToList();

            approveReqyestItems = allRequests;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading approved requests: {ex.Message}");
            approveReqyestItems = [];
        }
    }

    private DateTime? GetPendingSince(int id, string type)
    {
        if (type == "purchase")
        {
            var request = purchaseRequests.FirstOrDefault(r => r.Id == id);
            var approvals = request?.Approvals.ToList();

            if (request?.Status == RequestStatus.Closed || request?.Status == RequestStatus.Cancelled)
            {
                return null;
            }

            if (request?.RequestedById == currentUser.UserId)
            {
                return request?.DateCreated;
            }

            if (request?.ReportType == "Department")
            {
                var currentIndex = approvals?.LastOrDefault(a => a.Stage == ApprovalStage.DepartmentHead && a.Action == ApprovalAction.Approve);
                if (currentIndex != null) return currentIndex?.ActionDate;
            }

            if (request?.ReportType == "Division")
            {
                var currentIndex = approvals?.LastOrDefault(a => a.Stage == ApprovalStage.DivisionHead && a.Action == ApprovalAction.Approve);
                if (currentIndex != null) return currentIndex?.ActionDate;
            }

            if (utils.IsCEO())
            {
                var currentIndex = approvals?.LastOrDefault(a => a.Stage == ApprovalStage.CeoOrAvp && a.Action == ApprovalAction.Approve);
                if (currentIndex != null) return currentIndex?.ActionDate;
            }

            if (utils.IsAdmin())
            {
                var currentIndex = approvals?.LastOrDefault(a => a.Stage == ApprovalStage.Admin && a.Action == ApprovalAction.Approve);
                if (currentIndex != null) return currentIndex?.ActionDate;
            }
        }
        else
        {
            var request = jobOrders.FirstOrDefault(r => r.Id == id);
            var approvals = request?.Approvals.ToList();

            if (request?.RequestedById == currentUser.UserId)
            {
                return request?.DateCreated;
            }

            if (request?.ReportType == "Department")
            {
                var currentIndex = approvals?.LastOrDefault(a => a.Stage == ApprovalStage.DepartmentHead && a.Action == ApprovalAction.Approve);
                if (currentIndex != null) return currentIndex?.ActionDate;
            }

            if (request?.ReportType == "Division")
            {
                var currentIndex = approvals?.LastOrDefault(a => a.Stage == ApprovalStage.DivisionHead && a.Action == ApprovalAction.Approve);
                if (currentIndex != null) return currentIndex?.ActionDate;
            }

            if (utils.IsCEO())
            {
                var currentIndex = approvals?.LastOrDefault(a => a.Stage == ApprovalStage.CeoOrAvp && a.Action == ApprovalAction.Approve);
                if (currentIndex != null) return currentIndex?.ActionDate;
            }

            if (utils.IsAdmin())
            {
                var currentIndex = approvals?.LastOrDefault(a => a.Stage == ApprovalStage.Admin && a.Action == ApprovalAction.Approve);
                if (currentIndex != null) return currentIndex?.ActionDate;
            }
        }

        return null;
    }

    // Navigation Methods
    private void ViewAllRequests() => navigationManager.NavigateTo($"{GetRequestLink()}");

    private void FilterByStatus(RequestStatus status)
    {
        if (status == RequestStatus.PendingRequesterClosure)
        {
            navigationManager.NavigateTo($"{GetRequestLink()}");
            return;
        }
        navigationManager.NavigateTo($"{GetRequestLink()}?status={status}");
    }

    private string GetRequestLink() => requestType switch
    {
        RequestType.Purchase => "/requests/purchase-requests",
        RequestType.JobOrder => "/requests/job-orders",
        _ => "/dashboard"
    };

    private async Task SyncUsers()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save sync users?",
            Title = "Sync Users",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Sync",
            CancelText = "No, Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (result)
        {
            await confirmModal!.SetLoadingAsync(true);
            var isSuccess = await userSvc.SyncUsers();

            await confirmModal!.SetLoadingAsync(false);
            await confirmModal!.HideAsync();
            if (!isSuccess)
            {
                toastSvc.ShowError("Sorry. You can't the sync users right now.");
                return;
            }
            toastSvc.ShowSuccess("Users synced successfully!");
        }
    }

    private async Task RefreshDashboard()
    {
        isLoading = true;
        StateHasChanged();

        await LoadDashboardData();

        isLoading = false;
        StateHasChanged();
        toastSvc.ShowSuccess("Dashboard refreshed!");
    }

    private string GetTimeOfDayGreeting()
    {
        var hour = DateTime.Now.Hour;
        return hour switch
        {
            < 12 => "morning",
            < 17 => "afternoon",
            _ => "evening"
        };
    }

    #region Carousel

    private async Task InitializeCarousel()
    {
        try
        {
            // Add data-carousel attribute to the carousel container
            await jsRuntime.InvokeVoidAsync("eval",
                @"
                (function() {
                    const container = document.querySelector('.metrics-carousel');
                    if (!container) return;

                    // Add data attribute for identification
                    container.setAttribute('data-carousel', 'true');

                    let touchStartX = 0;
                    let touchEndX = 0;
                    const swipeThreshold = 50;

                    // Touch events for swipe
                    container.addEventListener('touchstart', (e) => {
                        touchStartX = e.changedTouches[0].screenX;
                    }, { passive: true });

                    container.addEventListener('touchend', (e) => {
                        touchEndX = e.changedTouches[0].screenX;
                        handleSwipe();
                    }, { passive: true });

                    function handleSwipe() {
                        const diff = touchStartX - touchEndX;

                        if (Math.abs(diff) > swipeThreshold) {
                            if (diff > 0) {
                                // Swipe left - next slide
                                if (window.__dashboardDotNetHelper) {
                                    window.__dashboardDotNetHelper.invokeMethodAsync('NextSlideJS');
                                }
                            } else {
                                // Swipe right - previous slide
                                if (window.__dashboardDotNetHelper) {
                                    window.__dashboardDotNetHelper.invokeMethodAsync('PrevSlideJS');
                                }
                            }
                        }
                    }
                })();
                ");

            //// Store the .NET helper globally
            //await JSRuntime.InvokeVoidAsync("eval",
            //    "window.__dashboardDotNetHelper = arguments[0];", dotNetRef);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing carousel: {ex.Message}");
        }
    }

    // Carousel methods
    private void NextSlide()
    {
        var totalSlides = GetTotalSlides();
        if (currentSlide < totalSlides - 1)
        {
            currentSlide++;
            StateHasChanged();
        }
    }

    private void PrevSlide()
    {
        if (currentSlide > 0)
        {
            currentSlide--;
            StateHasChanged();
        }
    }

    private void GoToSlide(int slideIndex)
    {
        currentSlide = slideIndex;
        StateHasChanged();
    }

    // These methods will be called from JavaScript
    [JSInvokable]
    public async Task NextSlideJS()
    {
        NextSlide();
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task PrevSlideJS()
    {
        PrevSlide();
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleWheel(WheelEventArgs e)
    {
        try
        {
            // Check if mobile view using a simpler approach
            var isMobile = await jsRuntime.InvokeAsync<bool>("eval",
                "window.innerWidth <= 768");

            if (isMobile)
            {
                if (e.DeltaY > 0)
                {
                    NextSlide();
                }
                else
                {
                    PrevSlide();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling wheel: {ex.Message}");
        }
    }

    private int GetTotalSlides()
    {
        if (activeTab == "all")
        {
            var count = 1; // Total Requests

            if (utils.IsUser()) count += 2; // My Drafts + Pending For Close

            if (purchaseRequests.Any(e => e.ReportType == "Department")) count += 2; // For Submission + Rejected

            if (purchaseRequests.Any(e => e.ReportType == "Division")) count += 1; // For Endorsement

            if (utils.IsAdmin()) count += 2; // For Approval + For Requisition

            if (utils.IsCEO()) count += 1; // For Approval

            count += 1; // Completed

            return Math.Max(1, count);
        }
        else if (activeTab == "purchase" || activeTab == "joborder")
        {
            var count = 1; // Total

            if (utils.IsUser()) count += 2; // My Drafts + Pending For Close

            if (purchaseRequests.Any(e => e.ReportType == "Department")) count += 2; // For Submission + Rejected

            if (purchaseRequests.Any(e => e.ReportType == "Division")) count += 1; // For Endorsement

            if (utils.IsAdmin()) count += 2; // For Approval + For Requisition

            if (utils.IsCEO()) count += 1; // For Approval

            count += 1; // Closed

            return Math.Max(1, count);
        }

        return 1;
    }

    #endregion Carousel
}