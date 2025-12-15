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
    private List<DepartmentVM> AllItems = [];
    private List<DepartmentVM> FilteredItems = [];
    private List<DivisionVM> Divisions = [];
    private string searchValue = "";
    private bool showValidations = false;
    private Confirmation__Modal? confirmModal;
    private Division__Filter? divisionFilter;
    private HashSet<int> selectedDivisions = [];

    protected override async Task OnInitializedAsync()
    {
        Divisions = await divisionSvc.GetAllDivisions();
        await LoadDepartments();
    }

    private async Task LoadDepartments()
    {
        AllItems = await departmentSvc.GetAllDepartments();
        FilteredItems = [.. AllItems];
    }

    private async Task HandleSearch()
    {
        ApplyFilter();
        await InvokeAsync(StateHasChanged);
    }

    private void ApplyFilter()
    {
        var query = AllItems.ToList();

        //apply division filter
        if (selectedDivisions.Count > 0)
        {
            query = [.. query.Where(r => selectedDivisions.Contains(r.DivisionId))];
        }

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            query = [.. query.Where(r =>
                (r.Name != null && r.Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
            )];
        }

        FilteredItems = [.. query];
    }

    private void ClearAllFilters()
    {
        searchValue = "";
        divisionFilter?.Reset();
        selectedDivisions.Clear();
        ApplyFilter();
        StateHasChanged();
    }

    private void ClearSearch()
    {
        searchValue = "";
        ApplyFilter();
    }

    private void OnDivisionFilterChanged(HashSet<int> _selectedDivisions)
    {
        selectedDivisions = _selectedDivisions;
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
        AllItems.Add(newDept);

        // Update filtered list if search is active
        if (string.IsNullOrWhiteSpace(searchValue) ||
            (newDept.Name != null && newDept.Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase)))
        {
            FilteredItems.Add(newDept);
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
        AllItems.Remove(department);
        FilteredItems.Remove(department);
        ApplyFilter();
    }

    private void OnDelete(DepartmentVM department)
    {
        AllItems.Remove(department);
        FilteredItems.Remove(department);
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
        showValidations = true;

        // Validate all departments, not just filtered ones
        var hasErrors = AllItems.Any(d =>
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

        var result = await confirmModal!.ShowAsync(options);
        if (!result) return;

        try
        {
            await confirmModal!.SetLoadingAsync(true);

            var response = await departmentSvc.BulkUpsertDepartments(AllItems);

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

            showValidations = false;
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
        await confirmModal!.SetLoadingAsync(false);
        await confirmModal!.HideAsync();
    }

    private void OnCancel(MouseEventArgs args)
    {
        navigationManager.NavigateTo("/system-configuration");
    }
}