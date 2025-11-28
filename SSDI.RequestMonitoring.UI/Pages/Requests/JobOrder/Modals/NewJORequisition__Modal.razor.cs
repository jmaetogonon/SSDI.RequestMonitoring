using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.JobOrder.Modals;

public partial class NewJORequisition__Modal : ComponentBase
{
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }
    [Parameter] public Job_OrderVM? PurchaseRequestHeader { get; set; }

    private Confirmation__Modal? confirmModal;
    private Job_Order_SlipVM Model { get; set; } = new();
    private ICollection<Job_Order_AttachVM> Attachments { get; set; } = [];

    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        Model.JobOrderId = PurchaseRequestHeader!.Id;
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

            var response = await slipSvc.CreateJORequisition(Model);
            if (response.Success)
            {

                var command = new UploadAttachmentJobOrderCommandVM
                {
                    JobOrderId = PurchaseRequestHeader!.Id,
                    Files = Attachments,
                    Type = RequestAttachType.Requisition,
                    RequisitionId = response.Data
                };

                var res = await attachSvc.UploadAttachPurchase(command);
                if (!res.Success)
                {
                    toastSvc.ShowError("Error uploading attachments. Please try again.");
                }

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
        Model = new();
        _isDisabledBtns = false;
    }
}