using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Pages.Settings.SystemConfig.MasterData;

public partial class Division_Index : ComponentBase
{
    private List<DivisionVM> _allItems = [];
    private List<DivisionVM> _filteredItems = [];
    private string _searchValue = "";
    private bool _showValidations = false;
    private Confirmation__Modal? _confirmModal;

    protected override async Task OnInitializedAsync()
    {
        await LoadDivisions();
    }

    private async Task HandleSearch()
    {
        ApplyFilter();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadDivisions()
    {
        _allItems = await divisionSvc.GetAllDivisions();
        _filteredItems = [.. _allItems];
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(_searchValue))
        {
            _filteredItems = [.. _allItems];
        }
        else
        {
            var term = _searchValue.ToLower();
            _filteredItems = [.. _allItems.Where(r =>
                r.Name != null && r.Name.Contains(_searchValue, StringComparison.OrdinalIgnoreCase)
                )];
        }
    }

    private void ClearSearch()
    {
        _searchValue = "";
        ApplyFilter();
    }

    private async void OnAdd()
    {
        var newDivision = new DivisionVM { Id = 0, Name = "" };
        _allItems.Add(newDivision);

        // Update filtered list if search is active
        if (string.IsNullOrWhiteSpace(_searchValue) ||
            (newDivision.Name != null && newDivision.Name.Contains(_searchValue, StringComparison.OrdinalIgnoreCase)))
        {
            _filteredItems.Add(newDivision);
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

    private void OnRemoveNew(DivisionVM division)
    {
        _allItems.Remove(division);
        _filteredItems.Remove(division);
        ApplyFilter();
    }

    private void OnDelete(DivisionVM division)
    {
        _allItems.Remove(division);
        _filteredItems.Remove(division);
    }

    private static void OnInputKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            // Optionally save on Enter
        }
    }

    private async Task OnSave(MouseEventArgs args)
    {
        _showValidations = true;

        // Validate all divisions, not just filtered ones
        var hasErrors = _allItems.Any(d => string.IsNullOrWhiteSpace(d.Name));
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


            var response = await divisionSvc.BulkUpsertDivisions(_allItems);

            await CloseModalWithLoading();

            if (!response.Success)
            {
                // Check if it's a foreign key constraint error
                if (response.Details.Contains("REFERENCE constraint") ||
                    response.Details.Contains("FK_") ||
                    response.Details.Contains("foreign key"))
                {

                    var message = "Cannot delete divisions(s) because they are currently in use. Reload the page and try again.";
                    await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "DIVISIONSC");
                }
                else
                {
                    var message = $"An error occurred while saving changes: {response.Message}";
                    await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "DIVISIONSC");
                }
                return;
            }

            await LoadDivisions();
            toastSvc.ShowSuccess("Changes applied successfully.");

            _showValidations = false;
        }
        catch (Exception ex)
        {
            await CloseModalWithLoading();
            var message = $"An error occurred while saving changes: {ex.Message}";
            await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "DIVISIONSC");
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
