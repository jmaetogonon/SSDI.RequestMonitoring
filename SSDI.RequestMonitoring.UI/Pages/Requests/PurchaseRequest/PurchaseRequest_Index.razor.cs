using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.PurchaseRequest;

public partial class PurchaseRequest_Index : ComponentBase
{
    private IQueryable<Purchase_RequestVM> AllRequests = default!;
    private IQueryable<Purchase_RequestVM> Requests = default!;
    private List<StatusSummary> StatusSummaries = [];

    private PaginationState pagination = new() { ItemsPerPage = 12 };

    private bool _isNewRequestModalFormVisible = false;
    private bool _isVisibleStatus = false;
    private string _searchValue = "";

    #region status filter

    private readonly List<string> individualStatuses = [TokenCons.Status__PreparedBy, TokenCons.Status__PurchaseRequest, TokenCons.Status__EndorsedBy, TokenCons.Status__Verfied, TokenCons.Status__Approved, TokenCons.Status__Rejected, TokenCons.Status__ApprovedToCanvas, TokenCons.Status__Closed];

    private Dictionary<string, bool> _statusChecked = [];

    private HashSet<string> _selectedStatuses = [];

    private bool _isAllStatusSelected = true;

    private string DisplayedStatuses => _isAllStatusSelected ? "All" : string.Join(", ", _selectedStatuses.Take(2)) + (_selectedStatuses.Count > 2 ? $" (+{_selectedStatuses.Count - 2})" : "");

    #endregion status filter

    protected override async Task OnInitializedAsync()
    {
        AllRequests = (await purchaseRequestSvc.GetAllPurchaseRequests()).AsQueryable();
        foreach (var status in individualStatuses)
            _statusChecked[status] = false;
        ApplyFilters();
        BuildStatusSummaries();
    }

    private void ToggleAddRequestModal()
    {
        _isNewRequestModalFormVisible = !_isNewRequestModalFormVisible;
    }
    private async Task RefreshData()
{
    AllRequests = (await purchaseRequestSvc.GetAllPurchaseRequests()).AsQueryable();
    ApplyFilters();
    BuildStatusSummaries();
    StateHasChanged();
}

    private void HandleSearch()
    {
        ApplyFilters();
    }

    private void ToggleAll()
    {
        _selectedStatuses.Clear();
        foreach (var status in individualStatuses)
            _statusChecked[status] = false;
        ApplyFilters();
        StateHasChanged();
    }

    private void ToggleStatus(string status)
    {
        if (_selectedStatuses.Contains(status))
        {
            _selectedStatuses.Remove(status);
        }
        else
        {
            _selectedStatuses.Add(status);

            _isAllStatusSelected = _selectedStatuses.Count == individualStatuses.Count;
            if (_isAllStatusSelected)
            {
                ToggleAll();
            }
        }

        ApplyFilters();
        StateHasChanged();
    }

    private void ApplyFilters()
    {
        var query = AllRequests.AsQueryable();

        // Apply status filter
        if (_selectedStatuses.Count > 0)
        {
            query = query.Where(r => _selectedStatuses.Contains(r.Status));
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(_searchValue))
        {
            var searchTerm = _searchValue.ToLower();
            query = query.Where(r =>
                (r.Name != null && r.Name.ToLower().Contains(searchTerm)) ||
                (r.Nature_Of_Request != null && r.Nature_Of_Request.ToLower().Contains(searchTerm)) ||
                (r.Justification != null && r.Justification.ToLower().Contains(searchTerm)) ||
                (r.Division_Department != null && r.Division_Department.ToLower().Contains(searchTerm)) ||
                (r.Status != null && r.Status.ToLower().Contains(searchTerm))
            );
        }

        Requests = query;
    }
    private void BuildStatusSummaries()
    {
        int totalCount = AllRequests.Count();

        // Efficient grouping — only one pass over AllRequests
        var groupedCounts = AllRequests
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        StatusSummaries =
        [
            new("Prepared Requests", groupedCounts.GetValueOrDefault(TokenCons.Status__PreparedBy, 0), totalCount, "Pending", "bi bi-file-earmark-plus"),
            new("Purchase Requests", groupedCounts.GetValueOrDefault(TokenCons.Status__PurchaseRequest, 0), totalCount, "Pending", "bi bi-cart"),
            new("Endorsed Requests", groupedCounts.GetValueOrDefault(TokenCons.Status__EndorsedBy, 0), totalCount, "Pending", "bi bi-hourglass-split"),
            new("Verified Requests", groupedCounts.GetValueOrDefault(TokenCons.Status__Verfied, 0), totalCount, "Pending", "bi bi-patch-check"),
            new("Completed Tasks", groupedCounts.GetValueOrDefault(TokenCons.Status__Closed, 0), totalCount, "Completed", "bi bi-check2-circle")
        ];
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
    private record StatusSummary(string Label, int Count, int TotalCount, string Status, string Icon);

}