using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Filters;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.JobOrder;

public partial class JobOrder_Index : ComponentBase
{
    private IQueryable<Job_OrderVM>? AllRequests;
    private IQueryable<Job_OrderVM>? Requests;
    private List<StatusSummary> StatusSummaries = [];
    private Job_OrderVM editModel = new();

    private string searchValue = "";
    private Status__Filter? statusFilter;
    private Priority__Filter? priorityFilter;
    private HashSet<RequestStatus> selectedStatuses = [];
    private HashSet<RequestPriority> selectedPriorities = [];
    private RequestStatus? lastClickedStatus = null;

    private PaginationState pagination = new() { ItemsPerPage = 10 };
    private GridSort<Job_OrderVM> sortStatus = GridSort<Job_OrderVM>.ByAscending(x => x.Status).ThenAscending(x => x.Status);

    private Confirmation__Modal? confirmModal;
    private bool isNewRequestModalVisible = false;
    private bool isLoading = false;
    private bool isShowNewBtn => CheckNewBtnPermission();

    protected override async Task OnInitializedAsync()
    {
        await currentUser.InitializeAsync();
        currentUser.OnUserChanged += Refresh;
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (isLoading) return;

        isLoading = true;
        try
        {
            if (utils.IsUser())
            {
                var requests = await jobOrderSvc.GetAllJobOrdersByUser(currentUser.UserId);
                AllRequests = requests.AsQueryable();
            }
            else if (utils.IsAdmin())
            {
                var requests = await jobOrderSvc.GetAllJobOrdersByAdmin();
                AllRequests = requests.AsQueryable();
            }
            else
            {
                var requests = await jobOrderSvc.GetAllJobOrderBySupervisor(currentUser.UserId, true, true);
                AllRequests = requests.AsQueryable();
                if (requests.FirstOrDefault()?.ReportType == "Division")
                {
                    AllRequests = requests.Where(e => e.Status == RequestStatus.ForEndorsement).AsQueryable();
                }

                if (utils.IsCEO())
                {
                    var ceoRequests = await jobOrderSvc.GetAllJobOrderByCeo();
                    AllRequests = AllRequests.Concat(ceoRequests).AsQueryable();
                }
            }
            ApplyFilters();
            BuildStatusSummaries();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void Refresh() => InvokeAsync(StateHasChanged);

    private void OnViewDetails(FluentDataGridRow<Job_OrderVM>? row)
    {
        if (row?.Item is null) return;
        navigationManager.NavigateTo($"/requests/job-orders/{row?.Item?.Id}/{row?.Item?.ReportType}");
    }

    private void OnCloseNewReqModal() => isNewRequestModalVisible = false;

    private async Task OnSaveNewReqModal()
    {
        isNewRequestModalVisible = false;
        AllRequests = null;
        await LoadDataAsync();
        toastSvc.ShowSuccess("The request has been added successfully.");
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

    public void Dispose()
    {
        AllRequests = null;
        Requests = null;
        StatusSummaries?.Clear();
        currentUser.OnUserChanged -= Refresh;
    }

    private record StatusSummary(RequestStatus Status, string Label, int Count, int TotalCount, string Icon);
}