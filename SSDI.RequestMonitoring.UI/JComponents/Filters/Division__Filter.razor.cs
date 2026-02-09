using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.JComponents.Filters;

public partial class Division__Filter : ComponentBase
{
    [Parameter] public EventCallback<HashSet<int>> SelectedDivisionsChanged { get; set; }
    [Parameter] public string AnchorId { get; set; } = "divisionFilter";
    [Parameter] public List<DivisionVM> _divisions { get; set; } = [];

    private bool _isVisible = false;
    private bool _isAllSelected = true;

    private string DisplayedDivisions => _isAllSelected ? "All" :
        string.Join(", ", _divisions.Where(d => d.IsChecked).Select(d => d.Name).Take(2)) +
        (_divisions.Count(d => d.IsChecked) > 2 ? $" (+{_divisions.Count(d => d.IsChecked) - 2})" : "");

    protected override void OnInitialized()
    {
        // Initialize IsChecked property for all divisions
        foreach (var division in _divisions)
        {
            division.IsChecked = false;
        }
    }

    private void ToggleVisibility()
    {
        _isVisible = !_isVisible;
    }

    private void ToggleAll()
    {
        // When "All" is selected, uncheck all individual divisions
        foreach (var division in _divisions)
        {
            division.IsChecked = false;
        }
        _isAllSelected = true;
        NotifySelectionChanged();
        StateHasChanged();
    }

    private void HandleDivisionChecked(DivisionVM division, bool isChecked)
    {
        // Set the checked state directly
        division.IsChecked = isChecked;

        // Update "All" selection state
        UpdateAllSelectionState();

        NotifySelectionChanged();
        StateHasChanged();
    }

    private void UpdateAllSelectionState()
    {
        var checkedCount = _divisions.Count(d => d.IsChecked);

        if (checkedCount == 0)
        {
            // If nothing is checked, select "All"
            _isAllSelected = true;
        }
        else if (checkedCount == _divisions.Count)
        {
            // If all are checked, also treat as "All"
            _isAllSelected = true;
            foreach (var division in _divisions)
            {
                division.IsChecked = false;
            }
        }
        else
        {
            // If some are checked, "All" is not selected
            _isAllSelected = false;
        }
    }

    private HashSet<int> GetSelectedDivisionIds()
    {
        if (_isAllSelected)
        {
            return [];
        }

        return [.. _divisions
            .Where(d => d.IsChecked)
            .Select(d => d.Id)];
    }

    private async void NotifySelectionChanged()
    {
        var selectedDivisionIds = GetSelectedDivisionIds();
        await SelectedDivisionsChanged.InvokeAsync(selectedDivisionIds);
    }

    // Public method to reset the filter
    public void Reset()
    {
        ToggleAll();
    }

    // Public method to set specific division IDs
    public void SetSelectedDivisionIds(IEnumerable<int> divisionIds)
    {
        var divisionIdSet = divisionIds.ToHashSet();

        if (divisionIdSet.Count == 0 || divisionIdSet.SetEquals(_divisions.Select(d => d.Id)))
        {
            // If empty or all divisions selected, treat as "All"
            ToggleAll();
        }
        else
        {
            // Set specific divisions
            foreach (var division in _divisions)
            {
                division.IsChecked = divisionIdSet.Contains(division.Id);
            }
            _isAllSelected = false;
            StateHasChanged();
            NotifySelectionChanged();
        }
    }
}