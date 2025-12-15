using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.JComponents.Filters;

public partial class Division__Filter : ComponentBase
{
    [Parameter] public EventCallback<HashSet<int>> SelectedDivisionsChanged { get; set; }
    [Parameter] public string AnchorId { get; set; } = "divisionFilter";
    [Parameter] public List<DivisionVM> Divisions { get; set; } = [];

    private bool _isVisible = false;
    private bool IsAllSelected = true;

    private string DisplayedDivisions => IsAllSelected ? "All" :
        string.Join(", ", Divisions.Where(d => d.IsChecked).Select(d => d.Name).Take(2)) +
        (Divisions.Count(d => d.IsChecked) > 2 ? $" (+{Divisions.Count(d => d.IsChecked) - 2})" : "");

    protected override void OnInitialized()
    {
        // Initialize IsChecked property for all divisions
        foreach (var division in Divisions)
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
        foreach (var division in Divisions)
        {
            division.IsChecked = false;
        }
        IsAllSelected = true;
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
        var checkedCount = Divisions.Count(d => d.IsChecked);

        if (checkedCount == 0)
        {
            // If nothing is checked, select "All"
            IsAllSelected = true;
        }
        else if (checkedCount == Divisions.Count)
        {
            // If all are checked, also treat as "All"
            IsAllSelected = true;
            foreach (var division in Divisions)
            {
                division.IsChecked = false;
            }
        }
        else
        {
            // If some are checked, "All" is not selected
            IsAllSelected = false;
        }
    }

    private HashSet<int> GetSelectedDivisionIds()
    {
        if (IsAllSelected)
        {
            return [];
        }

        return [.. Divisions
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

        if (divisionIdSet.Count == 0 || divisionIdSet.SetEquals(Divisions.Select(d => d.Id)))
        {
            // If empty or all divisions selected, treat as "All"
            ToggleAll();
        }
        else
        {
            // Set specific divisions
            foreach (var division in Divisions)
            {
                division.IsChecked = divisionIdSet.Contains(division.Id);
            }
            IsAllSelected = false;
            StateHasChanged();
            NotifySelectionChanged();
        }
    }
}