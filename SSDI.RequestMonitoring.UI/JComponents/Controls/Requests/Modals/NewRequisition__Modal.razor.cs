using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls.Requests.Modals;

public partial class NewRequisition__Modal : ComponentBase
{
    [Parameter] public IRequestDetailVM? RequestHeader { get; set; }
    [Parameter] public IAttachmentSvc AttachSvc { get; set; } = default!;
    [Parameter] public IRequisitionSlipSvc SlipSvc { get; set; } = default!;
    [Parameter] public RequestType RequestType { get; set; }
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }

    private ISlipVM Model { get; set; } = default!;
    private ICollection<IAttachmentVM> Attachments { get; set; } = [];
    private Confirmation__Modal? confirmModal;

    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        Model = RequestType is RequestType.JobOrder ? new Job_Order_SlipVM() : new Purchase_Request_SlipVM();

        Model.RequisitionerId = currentUser.UserId;
        Model.RequisitionerName = currentUser.FullName;
        Model.DateOfRequest = DateTime.Now;
    }

    private async void CloseModal()
    {
        await OnClose.InvokeAsync(null);
        ResetForm();
    }

    private async Task HandleSave()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save this requisition slip?",
            Title = "Save Requisition Slip",
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

            var response = await SlipSvc.CreateRequisition(Model, RequestHeader!.Id);
            if (response.Success)
            {
                var res = await AttachSvc.UploadAsync(RequestHeader, Attachments, RequestAttachType.Requisition, response.Data);
                if (!res.Success)
                {
                    toastSvc.ShowError("Error uploading attachments. Please try again.");
                }

                //var command = new UploadAttachmentPurchaseCommandVM
                //{
                //    PurchaseRequestId = RequestHeader!.Id,
                //    Files = Attachments,
                //    Type = RequestAttachType.Requisition,
                //    RequisitionId = response.Data
                //};

                //var res = await attachSvc.UploadAttachPurchase(command);
                //if (!res.Success)
                //{
                //    toastSvc.ShowError("Error uploading attachments. Please try again.");
                //}

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

    private void ResetForm()
    {
        Model = RequestType is RequestType.JobOrder ? new Job_Order_SlipVM() : new Purchase_Request_SlipVM();
        Attachments.Clear();
        IsShowAlert = false;
        _isDisabledBtns = false;
    }
}