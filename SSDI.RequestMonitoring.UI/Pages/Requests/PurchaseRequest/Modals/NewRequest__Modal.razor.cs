using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests;
using static SSDI.RequestMonitoring.UI.JComponents.Modals.Confirmation__Modal;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.PurchaseRequest.Modals;

public partial class NewRequest__Modal : ComponentBase
{
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }

    private Confirmation__Modal? confirmModal;
    private Purchase_RequestVM RequestModel { get; set; } = new();

    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    private async void CloseModal()
    {
        await OnClose.InvokeAsync(null);
        ResetForm();
    }

    private async Task HandleSave()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save this request?",
            Title = "Save Request",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Save",
            CancelText = "Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (result)
        {
            await confirmModal!.SetLoadingAsync(true); 
            
            
            _isDisabledBtns = true;
            IsShowAlert = false;
            RequestModel.Status = TokenCons.Status__PreparedBy;
            RequestModel.DateRequested = DateTime.Now;

            var response = await purchaseRequestSvc.CreatePurchaseRequest(RequestModel);
            if (response.Success)
            {
                ResetForm();
                await confirmModal!.SetLoadingAsync(false);
                await confirmModal!.HideAsync();
                await OnSave.InvokeAsync(null);
                return;
            }

            IsShowAlert = true;
            AlertMessage = response.Message;
            _isDisabledBtns = false;
            await confirmModal!.SetLoadingAsync(false);
        }


            
    }

    private void ResetForm()
    {
        RequestModel = new();
        _isDisabledBtns = false;
    }
}
