using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Pages.Dashboard.Dto;

namespace SSDI.RequestMonitoring.UI.Pages.Dashboard;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    private string _activeTab = "all";
    private int _currentSlide = 0;
    private ElementReference _carouselContainer;
    private DotNetObjectReference<Dashboard>? _dotNetRef;
    private IJSObjectReference? _jsModule;

    private RequestMetrics _purchaseRequestMetrics = new();
    private RequestMetrics _jobOrderMetrics = new();

    private List<Purchase_RequestVM> _purchaseRequests = [];
    private List<Job_OrderVM> _jobOrders = [];

    private List<ActivityItem> _recentActivity = [];
    private List<TeamRequestItem> _teamRequestsNeedingAttention = [];
    private List<ApprovedRequestItem> _approveRequestItems = [];

    private int _totalDraftCount = 0;
    private int _totalPendingSubmissionCount = 0;

    private bool _isLoading = true;
    private RequestType _requestType = RequestType.All;

    private Confirmation__Modal? _confirmModal;

    protected override async Task OnInitializedAsync()
    {
        await currentUser.InitializeAsync();
        await LoadDashboardData();
        _isLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);

            // Import JS module once
            _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/dashboard.js");

            // Initialize carousel
            await _jsModule.InvokeVoidAsync("initializeCarousel", _dotNetRef);
        }
    }

    private void SwitchTab(string tab)
    {
        _activeTab = tab;
        _currentSlide = 0; // Reset to first slide
        _requestType = tab switch
        {
            "purchase" => RequestType.Purchase,
            "joborder" => RequestType.JobOrder,
            _ => RequestType.All
        };

        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("disposeCarousel");
            //await _jsModule.DisposeAsync();
        }

        _dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task LoadDashboardData()
    {
        try
        {
            // Load both request types in parallel
            var purchaseTask = LoadPurchaseRequestData();
            var jobOrderTask = LoadJobOrderData();

            await Task.WhenAll(purchaseTask, jobOrderTask);

            _totalDraftCount = _purchaseRequestMetrics.DraftCount + _jobOrderMetrics.DraftCount;
            _totalPendingSubmissionCount = _purchaseRequestMetrics.PendingSubmissionCount + _jobOrderMetrics.PendingSubmissionCount;

            // Combine recent activity
            _recentActivity = [.. _purchaseRequestMetrics.RecentActivity
                .Concat(_jobOrderMetrics.RecentActivity)
                .OrderByDescending(a => a.Date)
                .Take(10)];

            // Load team requests for supervisors
            if (currentUser.IsSupervisor)
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
            if (currentUser.IsUser)
            {
                _purchaseRequests = await purchaseRequestSvc.GetAllPurchaseRequestsByUser(currentUser.UserId);
            }
            else
            {
                _purchaseRequests = await purchaseRequestSvc.GetAllPurchaseReqBySupervisor(currentUser.UserId, true, true);

                if (currentUser.IsCEO)
                {
                    var ceoRequests = await purchaseRequestSvc.GetAllPurchaseReqByCeo();
                    _purchaseRequests = [.. _purchaseRequests.Concat(ceoRequests).DistinctBy(e => e.Id)];
                }

                if (currentUser.IsAdmin)
                {
                    var adminRequests = await purchaseRequestSvc.GetAllPurchaseRequestsByAdmin();
                    _purchaseRequests = [.. _purchaseRequests.Concat(adminRequests).DistinctBy(r => r.Id)];
                }
            }

            _purchaseRequestMetrics = CalculateMetrics(_purchaseRequests, "purchase");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading purchase requests: {ex.Message}");
            _purchaseRequestMetrics = new RequestMetrics();
        }
    }

    private async Task LoadJobOrderData()
    {
        try
        {
            if (currentUser.IsUser)
            {
                _jobOrders = await jobOrderSvc.GetAllJobOrdersByUser(currentUser.UserId);
            }
            else
            {
                _jobOrders = await jobOrderSvc.GetAllJobOrderBySupervisor(currentUser.UserId, true, true);

                if (currentUser.IsCEO)
                {
                    var ceoRequests = await jobOrderSvc.GetAllJobOrderByCeo();
                    _jobOrders = [.. _jobOrders.Concat(ceoRequests).DistinctBy(e => e.Id)];
                }

                if (currentUser.IsAdmin)
                {
                    var adminRequests = await jobOrderSvc.GetAllJobOrdersByAdmin();
                    _jobOrders = [.. _jobOrders.Concat(adminRequests).DistinctBy(r => r.Id)];
                }
            }

            _jobOrderMetrics = CalculateMetrics(_jobOrders, "joborder");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading job orders: {ex.Message}");
            _jobOrderMetrics = new RequestMetrics();
        }
    }

    private static RequestMetrics CalculateMetrics<T>(List<T> requests, string requestType) where T : IRequestDetailVM
    {
        var metrics = new RequestMetrics
        {
            TotalCount = requests.Count,

            // Calculate status counts
            DraftCount = requests.Count(r => r.Status == RequestStatus.Draft),
            PendingSubmissionCount = requests.Count(r => r.Status == RequestStatus.Draft),
            PendingEndorsementCount = requests.Count(r => r.Status == RequestStatus.ForEndorsement),
            PendingAdminApprovalCount = requests.Count(r => r.Status == RequestStatus.ForAdminVerification),
            PendingAdminRequisitionCount = requests.Count(r => r.Status == RequestStatus.ForRequisition),
            PendingCEOApprovalCount = requests.Count(r => r.Status == RequestStatus.ForCeoApproval),
            PendingClosureCount = requests.Count(r => r.Status == RequestStatus.PendingRequesterClosure),
            AllPendingCount = requests.Count(r => r.Status != RequestStatus.Closed && r.Status != RequestStatus.Cancelled),
            CompletedCount = requests.Count(r =>
                    r.Status == RequestStatus.Closed),
            RejectedCount = requests.Count(r => r.Status == RequestStatus.Rejected),

            // Recent activity
            RecentActivity = [.. requests
                .OrderByDescending(r => r.DateModified)
                .Take(5)
                .Select(r => new ActivityItem
                {
                    Id = r.Id,
                    RequestType = requestType,
                    Title = r.Nature_Of_Request,
                    Status = r.Status,
                    Date = r.DateModified ?? DateTime.Now
                })]
        };

        return metrics;
    }

    private async Task LoadTeamRequests()
    {
        try
        {
            await Task.Delay(100);
            var purchaseTeamRequests = GetTeamRequests<Purchase_RequestVM>(
               _purchaseRequests,
               request => new TeamRequestItem
               {
                   Id = request.Id,
                   RequestType = "purchase",
                   Title = request.Nature_Of_Request,
                   RequestedBy = request.Name,
                   Status = request.Status,
                   Date = request.DateModified ?? DateTime.Now,
                   Priority = request.Priority,
                   ReportType = request.ReportType,
               }
           );

            var jobOrderTeamRequests = GetTeamRequests<Job_OrderVM>(
                _jobOrders,
                request => new TeamRequestItem
                {
                    Id = request.Id,
                    RequestType = "joborder",
                    Title = request.Nature_Of_Request,
                    RequestedBy = request.Name,
                    Status = request.Status,
                    Date = request.DateModified ?? DateTime.Now,
                    Priority = request.Priority,
                    ReportType = request.ReportType,
                }
            );

            var allTeamRequests = purchaseTeamRequests
                .Concat(jobOrderTeamRequests)
                .OrderByDescending(r => r.Date)
                .Take(10)
                .ToList();

            _teamRequestsNeedingAttention = allTeamRequests;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading team requests: {ex.Message}");
            _teamRequestsNeedingAttention = [];
        }
    }

    private List<TeamRequestItem> GetTeamRequests<TRequest>(
    IEnumerable<TRequest> requests,
    Func<TRequest, TeamRequestItem> mapper) where TRequest : BaseRequestVM
    {
        var teamRequests = new List<TRequest>();

        // Common filtering logic
        var forSubmission = requests.Where(e =>
            (e.ReportToDeptSupId == currentUser.UserId || e.UserDepartmentHeadId == currentUser.UserId || e.RequestedById == currentUser.UserId) &&
            (e.Status == RequestStatus.Draft || e.Status == RequestStatus.Rejected || e.Status == RequestStatus.PendingRequesterClosure || e.Status == RequestStatus.ForRequisition));

        var forEndorsement = requests.Where(e =>
            (e.ReportToDivSupId == currentUser.UserId || e.UserDivisionHeadId == currentUser.UserId) &&
            e.Status == RequestStatus.ForEndorsement);

        teamRequests = [.. forSubmission, .. forEndorsement];

        if (currentUser.IsAdmin)
        {
            var forAdminApproval = requests.Where(e =>
                e.Status == RequestStatus.ForAdminVerification ||
                e.Status == RequestStatus.ForRequisition);
            teamRequests = [.. teamRequests, .. forAdminApproval];
        }

        if (currentUser.IsCEO)
        {
            var forCEOApproval = requests.Where(e =>
                e.Status == RequestStatus.ForCeoApproval || e.Status == RequestStatus.ForRequisition);
            teamRequests = [.. teamRequests, .. forCEOApproval];
        }

        return [.. teamRequests.Select(mapper)];
    }

    private async Task LoadApprovedRequests()
    {
        try
        {
            await Task.Delay(100);

            // Local function to create ApprovedRequestItem
            ApprovedRequestItem MapToApprovedItem<TRequest>(TRequest request, string requestType)
                where TRequest : BaseRequestVM
            {
                return new ApprovedRequestItem
                {
                    Id = request.Id,
                    RequestType = requestType,
                    Title = request.Nature_Of_Request,
                    RequestedBy = request.Name,
                    Status = request.Status,
                    Date = request.DateModified ?? DateTime.Now,
                    Priority = request.Priority,
                    ReportType = request.ReportType,
                    PendingSince = CalculatePendingSince(request)
                };
            }

            // Local function to calculate pending date
            DateTime? CalculatePendingSince<TRequest>(TRequest request) where TRequest : BaseRequestVM
            {
                if (request.Status == RequestStatus.Closed || request.Status == RequestStatus.Cancelled || request.Status == RequestStatus.Rejected)
                    return null;

                if (request.RequestedById == currentUser.UserId)
                    return request.DateCreated;

                var approvalStage = DetermineApprovalStage(request);
                return request.ApprovalsBase?
                    .Where(a => a.Stage == approvalStage && a.Action == ApprovalAction.Approve)
                    .LastOrDefault()?
                    .ActionDate;
            }

            // Local function to determine approval stage
            ApprovalStage? DetermineApprovalStage<TRequest>(TRequest request) where TRequest : BaseRequestVM
            {
                ApprovalStage? stage = null;
                if (request.ReportType == "Department")
                    stage = ApprovalStage.DepartmentHead;

                if (request.ReportType == "Division" ||
                    (request.UserDivisionHeadId == currentUser.UserId &&
                     request.Status > RequestStatus.ForEndorsement))
                    stage = ApprovalStage.DivisionHead;

                if (currentUser.IsCEO)
                {
                    if (request.Status > RequestStatus.ForCeoApproval)
                        stage = ApprovalStage.CeoOrAvp;
                }

                if (currentUser.IsAdmin &&
                    request.Status > RequestStatus.ForAdminVerification)
                    stage = ApprovalStage.Admin;

                return stage;
            }

            var allRequests =
                _purchaseRequests.Select(r => MapToApprovedItem(r, "purchase"))
                .Concat(_jobOrders.Select(r => MapToApprovedItem(r, "joborder")))
                .OrderByDescending(r => r.PendingSince)
                .ToList();

            _approveRequestItems = allRequests;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading approved requests: {ex.Message}");
            _approveRequestItems = [];
        }
    }

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

    private string GetRequestLink() => _requestType switch
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

        var result = await _confirmModal!.ShowAsync(options);
        if (result)
        {
            await _confirmModal!.SetLoadingAsync(true);
            var isSuccess = await userSvc.SyncUsers();

            await _confirmModal!.SetLoadingAsync(false);
            await _confirmModal!.HideAsync();
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
        _isLoading = true;
        StateHasChanged();

        await LoadDashboardData();

        _isLoading = false;
        StateHasChanged();
        toastSvc.ShowSuccess("Dashboard refreshed!");
    }

    private static string GetTimeOfDayGreeting()
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
     

    // Carousel methods
    private void NextSlide()
    {
        var totalSlides = GetTotalSlides();
        if (_currentSlide < totalSlides - 1)
        {
            _currentSlide++;
            StateHasChanged();
        }
    }

    private void PrevSlide()
    {
        if (_currentSlide > 0)
        {
            _currentSlide--;
            StateHasChanged();
        }
    }

    private void GoToSlide(int slideIndex)
    {
        _currentSlide = slideIndex;
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
        if (_activeTab == "all")
        {
            var count = 1; // Total Requests

            if (currentUser.IsUser) count += 2; // My Drafts + Pending For Close

            if (_purchaseRequests.Any(e => e.ReportType == "Department")) count += 2; // For Submission + Rejected

            if (_purchaseRequests.Any(e => e.ReportType == "Division")) count += 1; // For Endorsement

            if (currentUser.IsAdmin) count += 2; // For Approval + For Requisition

            if (currentUser.IsCEO) count += 1; // For Approval

            count += 1; // Completed

            return Math.Max(1, count);
        }
        else if (_activeTab == "purchase" || _activeTab == "joborder")
        {
            var count = 1; // Total

            if (currentUser.IsUser) count += 2; // My Drafts + Pending For Close

            if (_purchaseRequests.Any(e => e.ReportType == "Department")) count += 2; // For Submission + Rejected

            if (_purchaseRequests.Any(e => e.ReportType == "Division")) count += 1; // For Endorsement

            if (currentUser.IsAdmin) count += 2; // For Approval + For Requisition

            if (currentUser.IsCEO) count += 1; // For Approval

            count += 1; // Closed

            return Math.Max(1, count);
        }

        return 1;
    }

    #endregion Carousel
}