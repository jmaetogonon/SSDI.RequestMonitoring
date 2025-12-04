using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.PurchaseRequest.Modals;

public partial class EditRequisition__Modal : ComponentBase
{
    [Parameter] public IRequestDetailVM? RequestHeader { get; set; }
    [Parameter] public ISlipVM Model { get; set; } = default!;
    [Parameter] public IAttachmentSvc AttachSvc { get; set; } = default!;
    [Parameter] public IRequisitionSlipSvc SlipSvc { get; set; } = default!;
    [Parameter] public RequestType RequestType { get; set; }
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }

    protected override void OnParametersSet()
    {
        Model.RequisitionerId = currentUser.UserId;
        Model.RequisitionerName = currentUser.FullName;
    }

    private Confirmation__Modal? confirmModal;

    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    private async Task HandleSave()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save the updated requisition slip?",
            Title = "Update Requisition Slip",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Update",
            CancelText = "No, Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (result)
        {
            await confirmModal!.SetLoadingAsync(true);

            _isDisabledBtns = true;
            IsShowAlert = false;

            var response = await SlipSvc.EditRequisition(Model, RequestHeader!.Id);
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
            await confirmModal!.HideAsync();
        }
    }

    private async void CloseModal()
    {
        await OnClose.InvokeAsync(null);
        ResetForm();
    }

    private void ResetForm()
    {
        Model = RequestType is RequestType.JobOrder ? new Job_Order_SlipVM() : new Purchase_Request_SlipVM();
        IsShowAlert = false;
        _isDisabledBtns = false;
    }
}