using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Pages.Settings.SystemConfig.MasterData;

public partial class Division_Index : ComponentBase
{
    private List<DivisionVM> AllItems = [];
    private List<DivisionVM> FilteredItems = [];
    private string searchValue = "";
    private bool showValidations = false;
    private Confirmation__Modal? confirmModal;

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
        AllItems = await divisionSvc.GetAllDivisions();
        FilteredItems = [.. AllItems];
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(searchValue))
        {
            FilteredItems = [.. AllItems];
        }
        else
        {
            var term = searchValue.ToLower();
            FilteredItems = [.. AllItems.Where(r =>
                r.Name != null && r.Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                )];
        }
    }

    private void ClearSearch()
    {
        searchValue = "";
        ApplyFilter();
    }

    private async void OnAdd()
    {
        var newDivision = new DivisionVM { Id = 0, Name = "" };
        AllItems.Add(newDivision);

        // Update filtered list if search is active
        if (string.IsNullOrWhiteSpace(searchValue) ||
            (newDivision.Name != null && newDivision.Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase)))
        {
            FilteredItems.Add(newDivision);
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
        AllItems.Remove(division);
        FilteredItems.Remove(division);
        ApplyFilter();
    }

    private void OnDelete(DivisionVM division)
    {
        AllItems.Remove(division);
        FilteredItems.Remove(division);
    }

    private void OnInputKeyDown(KeyboardEventArgs e, DivisionVM division)
    {
        if (e.Key == "Enter")
        {
            // Optionally save on Enter
        }
    }

    private async Task OnSave(MouseEventArgs args)
    {
        showValidations = true;

        // Validate all divisions, not just filtered ones
        var hasErrors = AllItems.Any(d => string.IsNullOrWhiteSpace(d.Name));
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


            var response = await divisionSvc.BulkUpsertDivisions(AllItems);

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

            showValidations = false;
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
        await confirmModal!.SetLoadingAsync(false);
        await confirmModal!.HideAsync();
    }

    private void OnCancel(MouseEventArgs args)
    {
        navigationManager.NavigateTo("/system-configuration");
    }
}
