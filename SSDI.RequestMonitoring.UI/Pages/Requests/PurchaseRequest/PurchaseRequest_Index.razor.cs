using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Filters;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
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
    private HashSet<string> selectedStatuses = [];
    private HashSet<string> selectedPriorities = [];

    private PaginationState pagination = new() { ItemsPerPage = 10 };
    private GridSort<Purchase_RequestVM> sortStatus = GridSort<Purchase_RequestVM>.ByAscending(x => x.Status).ThenAscending(x => x.Status);


    private Confirmation__Modal? confirmModal;
    private bool isNewRequestModalVisible, isEditRequestModalVisible = false;
    private bool isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (isLoading) return;

        isLoading = true;
        try
        {
            var requests = await purchaseRequestSvc.GetAllPurchaseRequests();
            AllRequests = requests.AsQueryable();
            ApplyFilters();
            BuildStatusSummaries();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OnCloseNewReqModal() => isNewRequestModalVisible = false;

    private void OnCloseEditReqModal() => isEditRequestModalVisible = false;

    private async Task OnSaveNewReqModal()
    {
        isNewRequestModalVisible = false;
        AllRequests = null;
        await LoadDataAsync();
        toastSvc.ShowSuccess("The request has been added successfully.");
    }

    private void OnSaveEditReqModal()
    {
        isEditRequestModalVisible = false;
        toastSvc.ShowSuccess("The request has been updated successfully.");
    }

    private void OnEditRequest(Purchase_RequestVM requestVM)
    {
        editModel = requestVM;
        isEditRequestModalVisible = true;
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
                (r.Division_Department != null && r.Division_Department.ToLower().Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                (r.Status != null && r.Status.ToLower().Contains(searchValue, StringComparison.OrdinalIgnoreCase))
            );
        }

        Requests = query;
    }

    private void OnStatusFilterChanged(HashSet<string> _selectedStatuses)
    {
        selectedStatuses = _selectedStatuses;
        ApplyFilters();
    }

    private void OnPriorityFilterChanged(HashSet<string> _selectedPriorities)
    {
        selectedPriorities = _selectedPriorities;
        ApplyFilters();
    }

    private static string GetRelativeTime(DateTime? date)
    {
        if (date is null) return "Unknown";

        var timeSpan = DateTime.Now - date.Value;

        if (timeSpan.TotalSeconds < 0) return "Future";

        return timeSpan.TotalSeconds switch
        {
            < 60 => "Just now",
            < 3600 => $"{timeSpan.Minutes}m ago",
            < 86400 => $"{timeSpan.Hours}h ago",
            < 604800 => $"{timeSpan.Days}d ago",
            < 2592000 => $"{timeSpan.Days / 7}w ago",
            < 31536000 => $"{timeSpan.Days / 30}mo ago",
            _ => $"{timeSpan.Days / 365}y ago"
        };

        //return timeSpan.TotalDays switch
        //{
        //    >= 365 => $"{(int)(timeSpan.TotalDays / 365)} year{(timeSpan.TotalDays / 365 >= 2 ? "s" : "")} ago",
        //    >= 30 => $"{(int)(timeSpan.TotalDays / 30)} month{(timeSpan.TotalDays / 30 >= 2 ? "s" : "")} ago",
        //    >= 7 => $"{(int)(timeSpan.TotalDays / 7)} week{(timeSpan.TotalDays / 7 >= 2 ? "s" : "")} ago",
        //    >= 1 => $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago",
        //    >= 1 / 24.0 => $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago",
        //    >= 1 / 1440.0 => $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago",
        //    _ => "Just now"
        //};
    }

    private static string GetPriorityDisplay(string priority, string otherPriority)
    {
        return priority.ToLower() switch
        {
            "1" => "24 hours",
            "2" => "This Week",
            _ => string.IsNullOrWhiteSpace(otherPriority) ? "Other" : otherPriority
        };
    }

    private static string GetStatusDisplay(string item)
    {
        return item switch
        {
            TokenCons.Status__PreparedBy => TokenCons.Status__PreparedBy__Desc,
            TokenCons.Status__PurchaseRequest => TokenCons.Status__PurchaseRequest__Desc,
            TokenCons.Status__EndorsedBy => TokenCons.Status__EndorsedBy__Desc,
            TokenCons.Status__Verfied => TokenCons.Status__Verfied__Desc,
            TokenCons.Status__Approved => TokenCons.Status__Approved__Desc,
            TokenCons.Status__Rejected => TokenCons.Status__Rejected__Desc,
            TokenCons.Status__ApprovedToCanvas => TokenCons.Status__ApprovedToCanvas__Desc,
            TokenCons.Status__Closed => TokenCons.Status__Closed__Desc,
            _ => "ambot"
        };
    }

    private static string GetStatusIcon(string status)
    {
        return status switch
        {
            TokenCons.Status__PreparedBy => "bi bi-file-earmark-plus",
            TokenCons.Status__PurchaseRequest => "bi bi-cart",
            TokenCons.Status__EndorsedBy => "bi bi-send",
            TokenCons.Status__Verfied => "bi bi-patch-check",
            TokenCons.Status__Approved => "bi bi-check-circle",
            TokenCons.Status__Rejected => "bi bi-x-circle",
            TokenCons.Status__ApprovedToCanvas => "bi bi-file-check",
            TokenCons.Status__Closed => "bi bi-archive",
            _ => "bi bi-circle" // Default icon
        };
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
            new("Prepared Requests", statusCounts.GetValueOrDefault(TokenCons.Status__PreparedBy, 0), totalCount, "Pending", "bi bi-file-earmark-plus"),
            new("Purchase Requests", statusCounts.GetValueOrDefault(TokenCons.Status__PurchaseRequest, 0), totalCount, "Pending", "bi bi-cart"),
            new("Endorsed Requests", statusCounts.GetValueOrDefault(TokenCons.Status__EndorsedBy, 0), totalCount, "Pending", "bi bi-hourglass-split"),
            new("Verified Requests", statusCounts.GetValueOrDefault(TokenCons.Status__Verfied, 0), totalCount, "Pending", "bi bi-patch-check"),
            new("Completed Tasks", statusCounts.GetValueOrDefault(TokenCons.Status__Closed, 0), totalCount, "Completed", "bi bi-check2-circle")
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

    public void Dispose()
    {
        AllRequests = null;
        Requests = null;
        StatusSummaries?.Clear();
    }

    private record StatusSummary(string Label, int Count, int TotalCount, string Status, string Icon);
}