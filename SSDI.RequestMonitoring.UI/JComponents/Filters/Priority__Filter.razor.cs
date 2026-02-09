using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Models.Enums;

namespace SSDI.RequestMonitoring.UI.JComponents.Filters;

public partial class Priority__Filter : ComponentBase
{
    [Parameter] public EventCallback<HashSet<RequestPriority>> SelectedPrioritiesChanged { get; set; }
    [Parameter] public string AnchorId { get; set; } = "priorityFilter";

    private bool _isVisible = false;
    private bool _isAllSelected = true;

    private readonly List<PriorityOption> _priorityOptions =
    [
        new PriorityOption { Value = RequestPriority.Hrs24, DisplayName = "24 hours", IsChecked = false },
        new PriorityOption { Value = RequestPriority.ThisWeek, DisplayName = "This week", IsChecked = false },
        new PriorityOption { Value = RequestPriority.Others, DisplayName = "Others", IsChecked = false }
    ];

    private string DisplayedPriorities => _isAllSelected ? "All" :
        string.Join(", ", _priorityOptions.Where(p => p.IsChecked).Select(p => p.DisplayName).Take(2)) +
        (_priorityOptions.Count(p => p.IsChecked) > 2 ? $" (+{_priorityOptions.Count(p => p.IsChecked) - 2})" : "");

    protected override void OnInitialized()
    {
        // Initialize all as unselected (which means "All" is selected)
        foreach (var priority in _priorityOptions)
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
        foreach (var priority in _priorityOptions)
        {
            priority.IsChecked = false;
        }
        _isAllSelected = true;
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
        var checkedCount = _priorityOptions.Count(p => p.IsChecked);

        if (checkedCount == 0)
        {
            // If nothing is checked, select "All"
            _isAllSelected = true;
        }
        else if (checkedCount == _priorityOptions.Count)
        {
            // If all are checked, also treat as "All"
            _isAllSelected = true;
            foreach (var priority in _priorityOptions)
            {
                priority.IsChecked = false;
            }
        }
        else
        {
            // If some are checked, "All" is not selected
            _isAllSelected = false;
        }
    }

    private HashSet<RequestPriority> GetSelectedPriorities()
    {
        if (_isAllSelected)
        {
            return [];
        }

        return [.. _priorityOptions
            .Where(p => p.IsChecked)
            .Select(p => p.Value)];
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

        if (prioritySet.Count == 0 || prioritySet.SetEquals(_priorityOptions.Select(p => p.Value)))
        {
            // If empty or all priorities selected, treat as "All"
            ToggleAll();
        }
        else
        {
            // Set specific priorities
            foreach (var priority in _priorityOptions)
            {
                priority.IsChecked = prioritySet.Contains(priority.Value);
            }
            _isAllSelected = false;
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