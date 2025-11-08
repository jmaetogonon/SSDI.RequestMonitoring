using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Models.Enums;

namespace SSDI.RequestMonitoring.UI.JComponents.Filters;

public partial class Priority__Filter : ComponentBase
{
    [Parameter] public EventCallback<HashSet<RequestPriority>> SelectedPrioritiesChanged { get; set; }
    [Parameter] public string AnchorId { get; set; } = "priorityFilter";

    private bool _isVisible = false;
    private bool IsAllSelected = true;

    private List<PriorityOption> PriorityOptions =
    [
        new PriorityOption { Value = RequestPriority.Hrs24, DisplayName = "24 hours", IsChecked = false },
        new PriorityOption { Value = RequestPriority.ThisWeek, DisplayName = "This week", IsChecked = false },
        new PriorityOption { Value = RequestPriority.Others, DisplayName = "Others", IsChecked = false }
    ];

    private string DisplayedPriorities => IsAllSelected ? "All" :
        string.Join(", ", PriorityOptions.Where(p => p.IsChecked).Select(p => p.DisplayName).Take(2)) +
        (PriorityOptions.Count(p => p.IsChecked) > 2 ? $" (+{PriorityOptions.Count(p => p.IsChecked) - 2})" : "");

    protected override void OnInitialized()
    {
        // Initialize all as unselected (which means "All" is selected)
        foreach (var priority in PriorityOptions)
        {
            priority.IsChecked = false;
        }
    }

    private void ToggleVisibility()
    {
        _isVisible = !_isVisible;
    }

    private void ToggleAll()
    {
        // When "All" is selected, uncheck all individual priorities
        foreach (var priority in PriorityOptions)
        {
            priority.IsChecked = false;
        }
        IsAllSelected = true;
        NotifySelectionChanged();
        StateHasChanged();
    }

    private void HandlePriorityChecked(PriorityOption priorityOption, bool isChecked)
    {
        // Set the checked state directly
        priorityOption.IsChecked = isChecked;

        // Update "All" selection state
        UpdateAllSelectionState();

        NotifySelectionChanged();
        StateHasChanged();
    }

    private void UpdateAllSelectionState()
    {
        var checkedCount = PriorityOptions.Count(p => p.IsChecked);

        if (checkedCount == 0)
        {
            // If nothing is checked, select "All"
            IsAllSelected = true;
        }
        else if (checkedCount == PriorityOptions.Count)
        {
            // If all are checked, also treat as "All"
            IsAllSelected = true;
            foreach (var priority in PriorityOptions)
            {
                priority.IsChecked = false;
            }
        }
        else
        {
            // If some are checked, "All" is not selected
            IsAllSelected = false;
        }
    }

    private HashSet<RequestPriority> GetSelectedPriorities()
    {
        if (IsAllSelected)
        {
            return new HashSet<RequestPriority>();
        }

        return PriorityOptions
            .Where(p => p.IsChecked)
            .Select(p => p.Value)
            .ToHashSet();
    }

    private async void NotifySelectionChanged()
    {
        var selectedPriorities = GetSelectedPriorities();
        await SelectedPrioritiesChanged.InvokeAsync(selectedPriorities);
    }

    // Public method to reset the filter
    public void Reset()
    {
        ToggleAll();
    }

    // Public method to set specific priorities
    public void SetSelectedPriorities(IEnumerable<RequestPriority> priorities)
    {
        var prioritySet = priorities.ToHashSet();

        if (prioritySet.Count == 0 || prioritySet.SetEquals(PriorityOptions.Select(p => p.Value)))
        {
            // If empty or all priorities selected, treat as "All"
            ToggleAll();
        }
        else
        {
            // Set specific priorities
            foreach (var priority in PriorityOptions)
            {
                priority.IsChecked = prioritySet.Contains(priority.Value);
            }
            IsAllSelected = false;
            StateHasChanged();
            NotifySelectionChanged();
        }
    }

    private class PriorityOption
    {
        public RequestPriority Value { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }
}