using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Filters;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Pages.Settings.SystemConfig.MasterData;

public partial class Department_Index : ComponentBase
{
    private List<DepartmentVM> _allItems = [];
    private List<DepartmentVM> _filteredItems = [];
    private List<DivisionVM> _divisions = [];
    private string _searchValue = "";
    private bool _showValidations = false;
    private Confirmation__Modal? _confirmModal;
    private Division__Filter? _divisionFilter;
    private HashSet<int> _selectedDivisions = [];

    protected override async Task OnInitializedAsync()
    {
        _divisions = await divisionSvc.GetAllDivisions();
        await LoadDepartments();
    }

    private async Task LoadDepartments()
    {
        _allItems = await departmentSvc.GetAllDepartments();
        _filteredItems = [.. _allItems];
    }

    private async Task HandleSearch()
    {
        ApplyFilter();
        await InvokeAsync(StateHasChanged);
    }

    private void ApplyFilter()
    {
        var query = _allItems.ToList();

        //apply division filter
        if (_selectedDivisions.Count > 0)
        {
            query = [.. query.Where(r => _selectedDivisions.Contains(r.DivisionId))];
        }

        if (!string.IsNullOrWhiteSpace(_searchValue))
        {
            query = [.. query.Where(r =>
                (r.Name != null && r.Name.Contains(_searchValue, StringComparison.OrdinalIgnoreCase))
            )];
        }

        _filteredItems = [.. query];
    }

    private void ClearAllFilters()
    {
        _searchValue = "";
        _divisionFilter?.Reset();
        _selectedDivisions.Clear();
        ApplyFilter();
        StateHasChanged();
    }

    private void ClearSearch()
    {
        _searchValue = "";
        ApplyFilter();
    }

    private void OnDivisionFilterChanged(HashSet<int> _selectedDivisions)
    {
        this._selectedDivisions = _selectedDivisions;
        ApplyFilter();
    }

    private async void OnAdd()
    {
        var newDept = new DepartmentVM
        {
            Id = 0,
            Name = "",
            DivisionId = 0
        };
        _allItems.Add(newDept);

        // Update filtered list if search is active
        if (string.IsNullOrWhiteSpace(_searchValue) ||
            (newDept.Name != null && newDept.Name.Contains(_searchValue, StringComparison.OrdinalIgnoreCase)))
        {
            _filteredItems.Add(newDept);
        }

        // Wait for the UI to update, then scroll
        await Task.Delay(100);
        await ScrollToNewItems();
    }

    private async Task ScrollToNewItems()
    {
        try
        {
            await InvokeAsync(StateHasChanged);
            await Task.Delay(50);
            await jsRuntime.InvokeVoidAsync("scrollTableToBottom");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scrolling to new items: {ex.Message}");
        }
    }

    private void OnRemoveNew(DepartmentVM department)
    {
        _allItems.Remove(department);
        _filteredItems.Remove(department);
        ApplyFilter();
    }

    private void OnDelete(DepartmentVM department)
    {
        _allItems.Remove(department);
        _filteredItems.Remove(department);
    }

    private void OnInputKeyDown(KeyboardEventArgs e, DepartmentVM department)
    {
        if (e.Key == "Enter")
        {
            // Optionally save on Enter
            Console.WriteLine($"Enter pressed on department ID {department.Name}");
        }
    }

    private async Task OnSave(MouseEventArgs args)
    {
        _showValidations = true;

        // Validate all departments, not just filtered ones
        var hasErrors = _allItems.Any(d =>
            string.IsNullOrWhiteSpace(d.Name) ||
            d.DivisionId == 0
        );

        if (hasErrors)
        {
            toastSvc.ShowError("Please fill in all required fields.");
            return;
        }

        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save the changes?",
            Title = "Save Changes",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Save",
            CancelText = "No, Cancel",
        };

        var result = await _confirmModal!.ShowAsync(options);
        if (!result) return;

        try
        {
            await _confirmModal!.SetLoadingAsync(true);

            var response = await departmentSvc.BulkUpsertDepartments(_allItems);

            await CloseModalWithLoading();

            if (!response.Success)
            {
                // Check if it's a foreign key constraint error
                if (response.Details.Contains("REFERENCE constraint") ||
                    response.Details.Contains("FK_") ||
                    response.Details.Contains("foreign key"))
                {
                    var message = "Cannot delete department(s) because they are currently in use. Reload the page and try again.";
                    await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "DEPARTMENTSC");
                }
                else
                {
                    var message = $"An error occurred while saving changes: {response.Message}";
                    await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "DEPARTMENTSC");
                }
                return;
            }

            await LoadDepartments();
            toastSvc.ShowSuccess("Changes applied successfully.");

            _showValidations = false;
        }
        catch (Exception ex)
        {
            await CloseModalWithLoading();
            var message = $"An error occurred while saving changes: {ex.Message}";
            await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "DEPARTMENTSC");
        }
    }

    private async Task CloseModalWithLoading()
    {
        await _confirmModal!.SetLoadingAsync(false);
        await _confirmModal!.HideAsync();
    }

    private void OnCancel(MouseEventArgs args)
    {
        navigationManager.NavigateTo("/system-configuration");
    }
}