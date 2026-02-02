using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls.Requests.Modals;

public partial class NewPO__Modal : ComponentBase
{
    [Parameter] public IRequestDetailVM? RequestHeader { get; set; }
    [Parameter] public IAttachSvc AttachSvc { get; set; } = default!;
    [Parameter] public IPOSlipSvc POSlipSvc { get; set; } = default!;
    [Parameter] public RequestType RequestType { get; set; }
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }

    private Request_PO_SlipVM Model { get; set; } = new();
    private ICollection<Request_AttachVM> Attachments { get; set; } = [];
    private Confirmation__Modal? confirmModal;
    private bool isPR => RequestType is RequestType.Purchase;

    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        Model.PreparedById = currentUser.UserId;
        Model.Date_Issued = DateTime.Now;
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
            Message = "Are you sure you want to save this purchase order slip?",
            Title = "Save Purchase Order Slip",
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

            Model.Purchase_RequestId = RequestType is RequestType.JobOrder ? null : RequestHeader!.Id;
            Model.Job_OrderId = RequestType is RequestType.JobOrder ? RequestHeader!.Id : null;

            var response = await POSlipSvc.CreatePOSlip(Model, RequestType);
            if (response.Success)
            {
                if (Attachments.Count != 0)
                {
                    var res = await AttachSvc.UploadAsync(RequestHeader!.Id, isPR, Attachments, RequestAttachType.PurchaseOrder, poID: response.Data);
                    if (!res.Success)
                    {
                        toastSvc.ShowError("Error uploading attachments. Please try again.");
                    }
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
        Attachments.Clear();
        IsShowAlert = false;
        _isDisabledBtns = false;
    }
}