using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Pages.Settings.SystemConfig.MasterData;

public partial class Vendor_Index : ComponentBase
{
    private List<VendorVM> AllItems = [];
    private List<VendorVM> FilteredItems = [];
    private string searchValue = "";
    private Confirmation__Modal? confirmModal;
    private bool IsAddModalVisible = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadItems();
    }

    private async Task HandleSearch()
    {
        ApplyFilter();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadItems()
    {
        AllItems = await vendorSvc.GetAllVendors();
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
                r.Vendor_Name != null && r.Vendor_Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase)||
                r.Payment_Details != null && r.Payment_Details.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                )];
        }
    }

    private void ClearSearch()
    {
        searchValue = "";
        ApplyFilter();
    }

    private void OnAdd() => IsAddModalVisible = true;

    private void OnCloseNewVendorModal() => IsAddModalVisible = false;

    private async Task OnSaveNewVendorModal()
    {
        IsAddModalVisible = false;
        toastSvc.ShowSuccess("The new vendor has been added successfully.");

        var syncResponse = await vendorSvc.SyncVendors();
        if (!syncResponse)
        {
            var message = "Failed to sync vendors from server. Please sync again.";
            await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "VENDORSC");
            return;
        }

        await LoadItems();
    }

    private async Task OnSync(MouseEventArgs args)
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to sync vendors from server?",
            Title = "Sync Vendors",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Sync",
            CancelText = "No, Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (!result) return;

        try
        {
            await confirmModal!.SetLoadingAsync(true);

            var response = await vendorSvc.SyncVendors();

            await CloseModalWithLoading();

            if (!response)
            {
                var message = "Something went wrong. Reload the page and try again.";
                await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "VENDORSC");
                return;
            }

            await LoadItems();
            toastSvc.ShowSuccess("Synced successfully.");
        }
        catch (Exception ex)
        {
            await CloseModalWithLoading();
            var message = $"An error occurred while syncing: {ex.Message}";
            await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "VENDORSC");
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