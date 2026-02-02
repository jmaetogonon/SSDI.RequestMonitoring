using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Pages.Settings.SystemConfig.MasterData;

public partial class BusinessUnit_Index : ComponentBase
{
    private List<BusinessUnitVM> AllItems = [];
    private List<BusinessUnitVM> FilteredItems = [];
    private string searchValue = "";
    private bool showValidations = false;
    private Confirmation__Modal? confirmModal;

    protected override async Task OnInitializedAsync()
    {
        await LoadBusinessUnits();
    }

    private async Task HandleSearch()
    {
        ApplyFilter();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadBusinessUnits()
    {
        AllItems = await buSvc.GetAllBusinessUnits();
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
                r.BU_Code != null && r.BU_Code.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                r.BU_Desc != null && r.BU_Desc.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                )];
        }
    }

    private void ClearSearch()
    {
        searchValue = "";
        ApplyFilter();
    }

    private async Task OnSync(MouseEventArgs args)
    {
        showValidations = true;

        // Validate all divisions, not just filtered ones
        var hasErrors = AllItems.Any(d => string.IsNullOrWhiteSpace(d.BU_Code));
        if (hasErrors)
        {
            toastSvc.ShowError("Please fill in all required fields.");
            return;
        }

        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to sync business unit from server?",
            Title = "Sync Business Units",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Sync",
            CancelText = "No, Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (!result) return;

        try
        {
            await confirmModal!.SetLoadingAsync(true);


            var response = await buSvc.SyncBusinessUnits();

            await CloseModalWithLoading();

            if (!response)
            {
                var message = "Something went wrong. Reload the page and try again.";
                await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "BUSINESSUNITSC");
                return;
            }

            await LoadBusinessUnits();
            toastSvc.ShowSuccess("Synced successfully.");

            showValidations = false;
        }
        catch (Exception ex)
        {
            await CloseModalWithLoading();
            var message = $"An error occurred while syncing: {ex.Message}";
            await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "BUSINESSUNITSC");
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
