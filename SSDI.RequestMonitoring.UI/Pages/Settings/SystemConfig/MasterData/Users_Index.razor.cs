using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Users;

namespace SSDI.RequestMonitoring.UI.Pages.Settings.SystemConfig.MasterData;

public partial class Users_Index : ComponentBase
{
    private List<UserVM> _allItems = [];
    private List<UserVM> _filteredItems = [];
    private string _searchValue = "";
    private Confirmation__Modal? _confirmModal;

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
        _allItems = await userSvc.GetUsers();
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
                r.FullName != null && r.FullName.Contains(_searchValue, StringComparison.OrdinalIgnoreCase) ||
                r.RoleDesc != null && r.RoleDesc.Contains(_searchValue, StringComparison.OrdinalIgnoreCase)
                )];
        }
    }

    private void ClearSearch()
    {
        _searchValue = "";
        ApplyFilter();
    }

    private async Task OnSync(MouseEventArgs args)
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to sync users from server?",
            Title = "Sync Users",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Sync",
            CancelText = "No, Cancel",
        };

        var result = await _confirmModal!.ShowAsync(options);
        if (!result) return;

        try
        {
            await _confirmModal!.SetLoadingAsync(true);

            var response = await userSvc.SyncUsers();

            await CloseModalWithLoading();

            if (!response)
            {
                var message = "Something went wrong. Reload the page and try again.";
                await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "USERSC");
                return;
            }

            await LoadItems();
            toastSvc.ShowSuccess("Synced successfully.");
        }
        catch (Exception ex)
        {
            await CloseModalWithLoading();
            var message = $"An error occurred while syncing: {ex.Message}";
            await messageSvc.ShowMessageBarAsync(message, MessageIntent.Error, "USERSC");
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