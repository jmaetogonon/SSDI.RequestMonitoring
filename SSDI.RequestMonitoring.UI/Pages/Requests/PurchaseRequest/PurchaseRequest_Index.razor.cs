using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Filters;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Enums;
using SSDI.RequestMonitoring.UI.Models.Requests;
using static SSDI.RequestMonitoring.UI.JComponents.Modals.Confirmation__Modal;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.PurchaseRequest;

public partial class PurchaseRequest_Index : ComponentBase
{
    private IQueryable<Purchase_RequestVM>? AllRequests;
    private IQueryable<Purchase_RequestVM>? Requests;
    private List<StatusSummary> StatusSummaries = [];
    private Purchase_RequestVM editModel = new();

    private string searchValue = "";
    private Status__Filter? statusFilter;
    private Priority__Filter? priorityFilter;
    private HashSet<RequestStatus> selectedStatuses = [];
    private HashSet<RequestPriority> selectedPriorities = [];

    private PaginationState pagination = new() { ItemsPerPage = 10 };
    private GridSort<Purchase_RequestVM> sortStatus = GridSort<Purchase_RequestVM>.ByAscending(x => x.Status).ThenAscending(x => x.Status);

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
                var requests = await purchaseRequestSvc.GetAllPurchaseRequestsByUser(currentUser.UserId);
                AllRequests = requests.AsQueryable();
            }
            else if (utils.IsAdmin())
            {
                var requests = await purchaseRequestSvc.GetAllPurchaseRequestsByAdmin();
                AllRequests = requests.AsQueryable();
            }
            else
            {
                var requests = await purchaseRequestSvc.GetAllPurchaseReqBySupervisor(currentUser.UserId, true, true);
                AllRequests = requests.AsQueryable();
                if (requests.FirstOrDefault()?.ReportType == "Division")
                {
                    AllRequests = requests.Where(e=>e.Status == RequestStatus.ForEndorsement).AsQueryable();
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

    private void BuildStatusSummaries()
    {
        if (AllRequests == null) return;

        var totalCount = AllRequests.Count();

        // Single pass to get all counts
        var statusCounts = AllRequests
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        StatusSummaries =
        [
            new("Draft", statusCounts.GetValueOrDefault(RequestStatus.Draft, 0), totalCount, "Pending", "bi bi-file-earmark-plus"),
            new("For Endorsing", statusCounts.GetValueOrDefault(RequestStatus.ForEndorsement, 0), totalCount, "Pending", "bi bi-cart"),
            new("For Admin Verification", statusCounts.GetValueOrDefault(RequestStatus.ForAdminVerification, 0), totalCount, "Pending", "bi bi-hourglass-split"),
            new("For Ceo Approval", statusCounts.GetValueOrDefault(RequestStatus.ForCeoApproval, 0), totalCount, "Pending", "bi bi-patch-check"),
            new("For Finance Approval", statusCounts.GetValueOrDefault(RequestStatus.ForFinanceApproval, 0), totalCount, "Completed", "bi bi-check2-circle"),
        ];
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
        return utils.IsUser() || AllRequests?.FirstOrDefault()?.ReportType == "Department";
    }

    public void Dispose()
    {
        AllRequests = null;
        Requests = null;
        StatusSummaries?.Clear();
        currentUser.OnUserChanged -= Refresh;
    }

    private record StatusSummary(string Label, int Count, int TotalCount, string Status, string Icon);
}