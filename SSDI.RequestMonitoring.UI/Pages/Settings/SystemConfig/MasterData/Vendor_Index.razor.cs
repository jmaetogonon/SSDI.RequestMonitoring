using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Pages.Settings.SystemConfig.MasterData;

public partial class Vendor_Index : ComponentBase
{
    private List<VendorVM> _allItems = [];
    private List<VendorVM> _filteredItems = [];
    private string _searchValue = "";
    private Confirmation__Modal? _confirmModal;
    private bool _isAddModalVisible = false;

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
        _allItems = await vendorSvc.GetAllVendors();
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
                r.Vendor_Name != null && r.Vendor_Name.Contains(_searchValue, StringComparison.OrdinalIgnoreCase)||
                r.Payment_Details != null && r.Payment_Details.Contains(_searchValue, StringComparison.OrdinalIgnoreCase)
                )];
        }
    }

    private void ClearSearch()
    {
        _searchValue = "";
        ApplyFilter();
    }

    private void OnAdd() => _isAddModalVisible = true;

    private void OnCloseNewVendorModal() => _isAddModalVisible = false;

    private async Task OnSaveNewVendorModal()
    {
        _isAddModalVisible = false;
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

        var result = await _confirmModal!.ShowAsync(options);
        if (!result) return;

        try
        {
            await _confirmModal!.SetLoadingAsync(true);

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
        await _confirmModal!.SetLoadingAsync(false);
        await _confirmModal!.HideAsync();
    }

    private void OnCancel(MouseEventArgs args)
    {
        navigationManager.NavigateTo("/system-configuration");
    }
}