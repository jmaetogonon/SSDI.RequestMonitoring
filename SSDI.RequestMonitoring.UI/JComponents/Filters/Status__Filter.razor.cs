using Microsoft.AspNetCore.Components;

namespace SSDI.RequestMonitoring.UI.JComponents.Filters;

public partial class Status__Filter : ComponentBase
{
    [Parameter] public EventCallback<HashSet<RequestStatus>> SelectedStatusesChanged { get; set; }
    [Parameter] public string AnchorId { get; set; } = "statusFilter";

    private bool _isVisible = false;
    private bool _isAllSelected = true;

    private readonly List<StatusOption> _statusOptions =
    [
        new StatusOption { Value = RequestStatus.Draft, DisplayName = TokenCons.Status__Draft, IsChecked = false },
        new StatusOption { Value = RequestStatus.ForEndorsement, DisplayName = TokenCons.Status__ForEndorsement, IsChecked = false },
        new StatusOption { Value = RequestStatus.ForAdminVerification, DisplayName = TokenCons.Status__ForAdminVerification, IsChecked = false },
        new StatusOption { Value = RequestStatus.ForCeoApproval, DisplayName = TokenCons.Status__ForCeoApproval, IsChecked = false },
        new StatusOption { Value = RequestStatus.ForRequisition, DisplayName = TokenCons.Status__ForRequisition, IsChecked = false },
         new StatusOption { Value = RequestStatus.Rejected, DisplayName = TokenCons.Status__Rejected, IsChecked = false },
        new StatusOption { Value = RequestStatus.Cancelled, DisplayName = TokenCons.Status__Cancelled, IsChecked = false },
        new StatusOption { Value = RequestStatus.PendingRequesterClosure, DisplayName = TokenCons.Status__PendingClose, IsChecked = false },
        new StatusOption { Value = RequestStatus.Closed, DisplayName = TokenCons.Status__Closed, IsChecked = false },
    ];

    private string DisplayedStatuses => _isAllSelected ? "All" :
        string.Join(", ", _statusOptions.Where(p => p.IsChecked).Select(p => p.DisplayName).Take(2)) +
        (_statusOptions.Count(p => p.IsChecked) > 2 ? $" (+{_statusOptions.Count(p => p.IsChecked) - 2})" : "");

    protected override void OnInitialized()
    {
        // Initialize all as unselected (which means "All" is selected)
        foreach (var item in _statusOptions)
        {
            item.IsChecked = false;
        }
    }

    private void ToggleVisibility()
    {
        _isVisible = !_isVisible;
    }

    private void ToggleAll()
    {
        // When "All" is selected, uncheck all individual priorities
        foreach (var item in _statusOptions)
        {
            item.IsChecked = false;
        }
        _isAllSelected = true;
        NotifySelectionChanged();
        StateHasChanged();
    }

    private void HandleStatusChecked(StatusOption statusOption, bool isChecked)
    {
        // Set the checked state directly
        statusOption.IsChecked = isChecked;

        // Update "All" selection state
        UpdateAllSelectionState();

        NotifySelectionChanged();
        StateHasChanged();
    }

    private void UpdateAllSelectionState()
    {
        var checkedCount = _statusOptions.Count(p => p.IsChecked);

        if (checkedCount == 0)
        {
            // If nothing is checked, select "All"
            _isAllSelected = true;
        }
        else if (checkedCount == _statusOptions.Count)
        {
            // If all are checked, also treat as "All"
            _isAllSelected = true;
            foreach (var item in _statusOptions)
            {
                item.IsChecked = false;
            }
        }
        else
        {
            // If some are checked, "All" is not selected
            _isAllSelected = false;
        }
    }

    private HashSet<RequestStatus> GetSelectedStatus()
    {
        if (_isAllSelected)
        {
            return [];
        }

        return [.. _statusOptions
            .Where(p => p.IsChecked)
            .Select(p => p.Value)];
    }

    private async void NotifySelectionChanged()
    {
        var selectedItems = GetSelectedStatus();
        await SelectedStatusesChanged.InvokeAsync(selectedItems);
    }

    public void Reset()
    {
        ToggleAll();
    }

    public void SetSelectedStatus(IEnumerable<RequestStatus> items)
    {
        var prioritySet = items.ToHashSet();

        if (prioritySet.Count == 0 || prioritySet.SetEquals(_statusOptions.Select(p => p.Value)))
        {
            // If empty or all priorities selected, treat as "All"
            ToggleAll();
        }
        else
        {
            // Set specific priorities
            foreach (var status in _statusOptions)
            {
                status.IsChecked = prioritySet.Contains(status.Value);
            }
            _isAllSelected = false;
            StateHasChanged();
            NotifySelectionChanged();
        }
    }

    private class StatusOption
    {
        public RequestStatus Value { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }
}