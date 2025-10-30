using Microsoft.AspNetCore.Components;

namespace SSDI.RequestMonitoring.UI.JComponents.Filters;

public partial class Status__Filter : ComponentBase
{
    [Parameter] public EventCallback<HashSet<string>> SelectedStatusesChanged { get; set; }
    [Parameter] public string AnchorId { get; set; } = "statusFilter";

    private bool _isVisible = false;
    private bool IsAllSelected = true;

    private List<StatusOption> StatusOptions =
    [
        new StatusOption { Value = "1", DisplayName = TokenCons.Status__PreparedBy__Desc, IsChecked = false },
        new StatusOption { Value = "2", DisplayName = TokenCons.Status__PurchaseRequest__Desc, IsChecked = false },
        new StatusOption { Value = "3", DisplayName = TokenCons.Status__EndorsedBy__Desc, IsChecked = false },
        new StatusOption { Value = "4", DisplayName = TokenCons.Status__Verfied__Desc, IsChecked = false },
        new StatusOption { Value = "5", DisplayName = TokenCons.Status__Approved__Desc, IsChecked = false },
        new StatusOption { Value = "6", DisplayName = TokenCons.Status__Rejected__Desc, IsChecked = false },
        new StatusOption { Value = "7", DisplayName = TokenCons.Status__ApprovedToCanvas__Desc, IsChecked = false },
        new StatusOption { Value = "8", DisplayName = TokenCons.Status__Closed__Desc, IsChecked = false }
    ];

    private string DisplayedStatuses => IsAllSelected ? "All" :
        string.Join(", ", StatusOptions.Where(p => p.IsChecked).Select(p => p.DisplayName).Take(2)) +
        (StatusOptions.Count(p => p.IsChecked) > 2 ? $" (+{StatusOptions.Count(p => p.IsChecked) - 2})" : "");

    protected override void OnInitialized()
    {
        // Initialize all as unselected (which means "All" is selected)
        foreach (var item in StatusOptions)
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
        foreach (var item in StatusOptions)
        {
            item.IsChecked = false;
        }
        IsAllSelected = true;
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
        var checkedCount = StatusOptions.Count(p => p.IsChecked);

        if (checkedCount == 0)
        {
            // If nothing is checked, select "All"
            IsAllSelected = true;
        }
        else if (checkedCount == StatusOptions.Count)
        {
            // If all are checked, also treat as "All"
            IsAllSelected = true;
            foreach (var item in StatusOptions)
            {
                item.IsChecked = false;
            }
        }
        else
        {
            // If some are checked, "All" is not selected
            IsAllSelected = false;
        }
    }

    private HashSet<string> GetSelectedStatus()
    {
        if (IsAllSelected)
        {
            return new HashSet<string>();
        }

        return StatusOptions
            .Where(p => p.IsChecked)
            .Select(p => p.Value)
            .ToHashSet();
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

    public void SetSelectedStatus(IEnumerable<string> items)
    {
        var prioritySet = items.ToHashSet();

        if (prioritySet.Count == 0 || prioritySet.SetEquals(StatusOptions.Select(p => p.Value)))
        {
            // If empty or all priorities selected, treat as "All"
            ToggleAll();
        }
        else
        {
            // Set specific priorities
            foreach (var status in StatusOptions)
            {
                status.IsChecked = prioritySet.Contains(status.Value);
            }
            IsAllSelected = false;
            StateHasChanged();
            NotifySelectionChanged();
        }
    }

    private class StatusOption
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }
}