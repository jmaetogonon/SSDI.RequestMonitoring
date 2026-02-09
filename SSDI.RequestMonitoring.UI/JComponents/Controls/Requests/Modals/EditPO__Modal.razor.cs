using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls.Requests.Modals;

public partial class EditPO__Modal : ComponentBase
{
    [Parameter] public IRequestDetailVM? RequestHeader { get; set; }
    [Parameter] public Request_PO_SlipVM Model { get; set; } = default!;
    [Parameter] public IAttachSvc AttachSvc { get; set; } = default!;
    [Parameter] public IPOSlipSvc POSlipSvc { get; set; } = default!;
    [Parameter] public RequestType RequestType { get; set; }
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }
    [Parameter] public Confirmation__Modal? ConfirmModal { get; set; }


    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    private async Task HandleSave()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save the updated purchase order slip?",
            Title = "Update Purchase Order Slip",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Update",
            CancelText = "No, Cancel",
        };

        var result = await ConfirmModal!.ShowAsync(options);
        if (result)
        {
            await ConfirmModal!.SetLoadingAsync(true);

            _isDisabledBtns = true;
            IsShowAlert = false;

            var response = await POSlipSvc.EditPO(Model);
            if (response.Success)
            {
                ResetForm();
                await ConfirmModal!.SetLoadingAsync(false);
                await ConfirmModal!.HideAsync();
                await OnSave.InvokeAsync(null);
                return;
            }

            IsShowAlert = true;
            AlertMessage = response.Message;
            _isDisabledBtns = false;
            await ConfirmModal!.SetLoadingAsync(false);
            await ConfirmModal!.HideAsync();
        }
    }

    private async void CloseModal()
    {
        await OnClose.InvokeAsync(null);
        ResetForm();
    }

    private void ResetForm()
    {
        Model = new();
        IsShowAlert = false;
        _isDisabledBtns = false;
    }
}