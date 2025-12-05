using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Filters;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using System.Web;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.PurchaseRequest;

public partial class PurchaseRequest_Index : ComponentBase
{
    private IQueryable<Purchase_RequestVM>? AllRequests;
    private IQueryable<Purchase_RequestVM>? Requests;
    private List<StatusSummary> StatusSummaries = [];
    private Purchase_RequestVM editModel = new();

    private List<DivisionVM> divisions = [];
    private List<DepartmentVM> departments = [];

    private string searchValue = "";
    private Status__Filter? statusFilter;
    private Priority__Filter? priorityFilter;
    private HashSet<RequestStatus> selectedStatuses = [];
    private HashSet<RequestPriority> selectedPriorities = [];
    private RequestStatus? lastClickedStatus = null;

    private PaginationState pagination = new() { ItemsPerPage = 10 };
    private GridSort<Purchase_RequestVM> sortStatus = GridSort<Purchase_RequestVM>.ByAscending(x => x.Status).ThenAscending(x => x.Status);

    private Confirmation__Modal? confirmModal;
    private bool isNewRequestModalVisible = false;
    private bool isLoading = false;
    private bool isShowNewBtn => CheckNewBtnPermission();

    //carousel fields
    private int currentSlide = 0;

    private ElementReference carouselContainer;
    private DotNetObjectReference<PurchaseRequest_Index>? dotNetRef;
    private IJSObjectReference? jsModule;

    protected override async Task OnInitializedAsync()
    {
        await currentUser.InitializeAsync();
        currentUser.OnUserChanged += Refresh;
        divisions = await divisionSvc.GetAllDivisions();
        departments = await departmentSvc.GetAllDepartments();
        await LoadDataAsync();
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

    private async Task LoadDataAsync()
    {
        if (isLoading) return;

        isLoading = true;
        try
        {
            if (utils.IsUser())
            {
                var requests = await purchaseRequestSvc.GetAllPurchaseRequestsByUser(currentUser.UserId);
                AllRequests = requests.AsQueryable();
            }
            else if (utils.IsAdmin())
            {
                var requests = await purchaseRequestSvc.GetAllPurchaseRequestsByAdmin();
                AllRequests = requests.AsQueryable();
                OnStatusFilterChanged([RequestStatus.ForAdminVerification, RequestStatus.ForRequisition]);
                statusFilter?.SetSelectedStatus(selectedStatuses);
            }
            else
            {
                var requests = await purchaseRequestSvc.GetAllPurchaseReqBySupervisor(currentUser.UserId, true, true);
                AllRequests = requests.AsQueryable();
                if (requests.Any(e => e.ReportType == "Division"))
                {
                    AllRequests = requests.Where(e => e.Status == RequestStatus.ForEndorsement).AsQueryable();
                    OnStatusFilterChanged([RequestStatus.ForEndorsement]);
                    statusFilter?.SetSelectedStatus(selectedStatuses);
                }

                if (utils.IsCEO())
                {
                    var ceoRequests = await purchaseRequestSvc.GetAllPurchaseReqByCeo();
                    AllRequests = AllRequests.Concat(ceoRequests).DistinctBy(r => r.Id).AsQueryable();
                    OnStatusFilterChanged([RequestStatus.ForEndorsement, RequestStatus.ForCeoApproval]);
                    statusFilter?.SetSelectedStatus(selectedStatuses);
                }
            }

            PreLoadFilter();
            ApplyFilters();
            BuildStatusSummaries();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void Refresh() => InvokeAsync(StateHasChanged);

    private void PreLoadFilter()
    {
        var uri = new Uri(navigationManager.Uri);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        var statusValue = queryParams["status"];
        var actionValue = queryParams["action"];

        if (Enum.TryParse<RequestStatus>(statusValue, out var parsedStatus))
        {
            selectedStatuses = [parsedStatus];
            statusFilter?.SetSelectedStatus(selectedStatuses);
        }

        if (actionValue == "new")
        {
            isNewRequestModalVisible = true;
        }
    }

    private void OnViewDetails(FluentDataGridRow<Purchase_RequestVM>? row)
    {
        if (row?.Item is null) return;
        navigationManager.NavigateTo($"/requests/purchase-requests/{row?.Item?.Id}/{row?.Item?.ReportType}");
    }

    private void OnCloseNewReqModal() => isNewRequestModalVisible = false;

    private async Task OnSaveNewReqModal()
    {
        isNewRequestModalVisible = false;
        AllRequests = null;
        await LoadDataAsync();
        toastSvc.ShowSuccess("The request has been added successfully.");
    }

    private async Task OnDeleteRequest(int id)
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to permanently delete this item? This action cannot be undone.",
            Title = "Delete Item",
            Variant = ConfirmationModalVariant.delete,
            ConfirmText = "Delete",
            CancelText = "Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);

        if (result)
        {
            await confirmModal!.SetLoadingAsync(true);

            var response = await purchaseRequestSvc.DeletePurchaseRequest(id);
            if (response.Success)
            {
                await confirmModal!.SetLoadingAsync(false);
                await confirmModal!.HideAsync();
                AllRequests = null;
                await LoadDataAsync();
                toastSvc.ShowSuccess("The request has been deleted successfully.");
            }
            else
            {
                await confirmModal!.SetLoadingAsync(false);
                await confirmModal!.HideAsync();
                toastSvc.ShowError(response.Message);
            }
        }
    }

    private void OnAddNewRequest() => isNewRequestModalVisible = true;

    private void HandleSearch()
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (AllRequests == null) return;

        var query = AllRequests.AsQueryable();

        // Apply status filter
        if (selectedStatuses.Count > 0)
        {
            query = query?.Where(r => selectedStatuses.Contains(r.Status));
        }
        // Apply priority filter
        if (selectedPriorities.Count > 0)
        {
            query = query?.Where(r => selectedPriorities.Contains(r.Priority));
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            query = query?.Where(r =>
                (r.Name != null && r.Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                (r.Nature_Of_Request != null && r.Nature_Of_Request.ToLower().Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                (r.Justification != null && r.Justification.ToLower().Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                (r.Division_Department != null && r.Division_Department.ToLower().Contains(searchValue, StringComparison.OrdinalIgnoreCase))
            );
        }

        Requests = query;
    }

    private void OnStatusFilterChanged(HashSet<RequestStatus> _selectedStatuses)
    {
        selectedStatuses = _selectedStatuses;
        ApplyFilters();
    }

    private void OnPriorityFilterChanged(HashSet<RequestPriority> _selectedPriorities)
    {
        selectedPriorities = _selectedPriorities;
        ApplyFilters();
    }

    private void OnStatusCardClick(RequestStatus status)
    {
        if (lastClickedStatus == status && selectedStatuses.Count == 1 && selectedStatuses.Contains(status))
        {
            ClearAllFilters();
            lastClickedStatus = null;
        }
        else
        {
            ClearAllFilters();
            selectedStatuses = new HashSet<RequestStatus> { status };
            statusFilter?.SetSelectedStatus(selectedStatuses);
            ApplyFilters();
            lastClickedStatus = status;
        }

        StateHasChanged();
    }

    private void BuildStatusSummaries()
    {
        if (AllRequests == null) return;

        var totalCount = AllRequests.Count();

        // Single pass to get all counts
        var statusCounts = AllRequests
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        if (utils.IsAdmin())
        {
            StatusSummaries =
            [
                new(RequestStatus.ForAdminVerification,TokenCons.Status__ForAdminVerification, statusCounts.GetValueOrDefault(RequestStatus.ForAdminVerification, 0), totalCount , utils.GetStatusIcon(RequestStatus.ForAdminVerification)),
                new(RequestStatus.ForRequisition,TokenCons.Status__ForRequisition, statusCounts.GetValueOrDefault(RequestStatus.ForRequisition, 0), totalCount, utils.GetStatusIcon(RequestStatus.ForRequisition)),
                new(RequestStatus.PendingRequesterClosure,TokenCons.Status__PendingClose, statusCounts.GetValueOrDefault(RequestStatus.PendingRequesterClosure, 0), totalCount, utils.GetStatusIcon(RequestStatus.PendingRequesterClosure)),
                new(RequestStatus.Closed,TokenCons.Status__Closed, statusCounts.GetValueOrDefault(RequestStatus.Closed, 0), totalCount, utils.GetStatusIcon(RequestStatus.Closed)),
            ];
        }
        else if (utils.IsCEO())
        {
            StatusSummaries =
            [
                new(RequestStatus.ForEndorsement, TokenCons.Status__ForEndorsement, statusCounts.GetValueOrDefault(RequestStatus.ForEndorsement, 0), totalCount , utils.GetStatusIcon(RequestStatus.ForEndorsement)),
                new(RequestStatus.ForCeoApproval, TokenCons.Status__ForCeoApproval, statusCounts.GetValueOrDefault(RequestStatus.ForCeoApproval, 0), totalCount , utils.GetStatusIcon(RequestStatus.ForCeoApproval)),
                new(RequestStatus.ForRequisition, TokenCons.Status__ForRequisition, statusCounts.GetValueOrDefault(RequestStatus.ForRequisition, 0), totalCount, utils.GetStatusIcon(RequestStatus.ForRequisition)),
                new(RequestStatus.Closed, TokenCons.Status__Closed, statusCounts.GetValueOrDefault(RequestStatus.Closed, 0), totalCount, utils.GetStatusIcon(RequestStatus.Closed)),
            ];
        }
        else if (utils.IsSupervisor() && !utils.IsCEO())
        {
            StatusSummaries =
            [
                new(RequestStatus.Draft, TokenCons.Status__Draft, statusCounts.GetValueOrDefault(RequestStatus.Draft, 0), totalCount , utils.GetStatusIcon(RequestStatus.Draft)),
                new(RequestStatus.Rejected, TokenCons.Status__Rejected, statusCounts.GetValueOrDefault(RequestStatus.Rejected, 0), totalCount , utils.GetStatusIcon(RequestStatus.Rejected)),
                new(RequestStatus.ForRequisition, TokenCons.Status__ForRequisition, statusCounts.GetValueOrDefault(RequestStatus.ForRequisition, 0), totalCount, utils.GetStatusIcon(RequestStatus.ForRequisition)),
                new(RequestStatus.PendingRequesterClosure, TokenCons.Status__PendingClose, statusCounts.GetValueOrDefault(RequestStatus.PendingRequesterClosure, 0), totalCount, utils.GetStatusIcon(RequestStatus.PendingRequesterClosure)),
                new(RequestStatus.Closed, TokenCons.Status__Closed, statusCounts.GetValueOrDefault(RequestStatus.Closed, 0), totalCount, utils.GetStatusIcon(RequestStatus.Closed)),
            ];
        }
        else
        {
            StatusSummaries =
            [
                new(RequestStatus.Draft, TokenCons.Status__Draft, statusCounts.GetValueOrDefault(RequestStatus.Draft, 0), totalCount , utils.GetStatusIcon(RequestStatus.Draft)),
                new(RequestStatus.Rejected, TokenCons.Status__Rejected, statusCounts.GetValueOrDefault(RequestStatus.Rejected, 0), totalCount , utils.GetStatusIcon(RequestStatus.Rejected)),
                new(RequestStatus.ForRequisition, TokenCons.Status__ForRequisition, statusCounts.GetValueOrDefault(RequestStatus.ForRequisition, 0), totalCount, utils.GetStatusIcon(RequestStatus.ForRequisition)),
                new(RequestStatus.Closed, TokenCons.Status__Closed, statusCounts.GetValueOrDefault(RequestStatus.Closed, 0), totalCount, utils.GetStatusIcon(RequestStatus.Closed)),
            ];
        }
    }

    private async Task OnPageSizeChanged()
    {
        pagination.ItemsPerPage = uiStateSvc.PageSize;
        await pagination.SetCurrentPageIndexAsync(0);
    }

    private void ClearAllFilters()
    {
        searchValue = "";
        statusFilter?.Reset();
        priorityFilter?.Reset();
        selectedStatuses.Clear();
        selectedPriorities.Clear();
        ApplyFilters();
        StateHasChanged();
    }

    public bool CheckNewBtnPermission()
    {
        return utils.IsUser() || AllRequests?.FirstOrDefault()?.ReportType == "Department" || (AllRequests?.FirstOrDefault() is null && utils.IsSupervisor() && !utils.IsAdmin());
    }

    #region Carousel

    private async Task InitializeCarousel()
    {
        try
        {
            // Add data-carousel attribute to the carousel container
            await JSRuntime.InvokeVoidAsync("eval",
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
                                if (window.__purchaseRequestDotNetHelper) {
                                    window.__purchaseRequestDotNetHelper.invokeMethodAsync('NextSlideJS');
                                }
                            } else {
                                // Swipe right - previous slide
                                if (window.__purchaseRequestDotNetHelper) {
                                    window.__purchaseRequestDotNetHelper.invokeMethodAsync('PrevSlideJS');
                                }
                            }
                        }
                    }
                })();
                ");

            // Store the .NET helper globally with unique name
            //await JSRuntime.InvokeVoidAsync("eval",
            //    "window.__purchaseRequestDotNetHelper = arguments[0];", dotNetRef);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing carousel: {ex.Message}");
        }
    }

    // Carousel methods
    private void NextSlide()
    {
        if (StatusSummaries.Count > 0 && currentSlide < StatusSummaries.Count - 1)
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
        if (slideIndex >= 0 && slideIndex < StatusSummaries.Count)
        {
            currentSlide = slideIndex;
            StateHasChanged();
        }
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
            var isMobile = await JSRuntime.InvokeAsync<bool>("eval",
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

    #endregion Carousel

    public void Dispose()
    {
        AllRequests = null;
        Requests = null;
        StatusSummaries?.Clear();
        currentUser.OnUserChanged -= Refresh;
    }

    private record StatusSummary(RequestStatus Status, string Label, int Count, int TotalCount, string Icon);
}