using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Pages.Settings.SystemConfig.MasterData;

public partial class Vendor_AddDialog : ComponentBase
{
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<VendorVM> OnSave { get; set; }
    [Parameter] public Confirmation__Modal? ConfirmModal { get; set; }

    private VendorVM FormModel { get; set; } = new();

    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    private async void CloseModal()
    {
        await OnClose.InvokeAsync(null);
        ResetForm();
    }

    private async Task HandleSave()
    {
        var isExist = await vendorSvc.IsExistVendor(FormModel.Vendor_Name);

        if (isExist)
        {
            IsShowAlert = true;
            AlertMessage = "Vendor already exists.";
            return;
        }

        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save this vendor to the server?",
            Title = "Save Vendor",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Save",
            CancelText = "Cancel",
        };

        var result = await ConfirmModal!.ShowAsync(options);
        if (result)
        {
            await ConfirmModal!.SetLoadingAsync(true);

            IsShowAlert = false;

            var response = await vendorSvc.AddVendorToServer(FormModel);
            if (response)
            {
                await CloseModalWithLoading();
                await OnSave.InvokeAsync(FormModel);
                ResetForm();
                return;
            }

            IsShowAlert = true;
            AlertMessage = "Something went wrong. Try again.";
        }
    }

    private async Task CloseModalWithLoading()
    {
        await ConfirmModal!.SetLoadingAsync(false);
        await ConfirmModal!.HideAsync();
    }
    private void ResetForm()
    {
        FormModel = new();
        IsShowAlert = false;
    }
}
