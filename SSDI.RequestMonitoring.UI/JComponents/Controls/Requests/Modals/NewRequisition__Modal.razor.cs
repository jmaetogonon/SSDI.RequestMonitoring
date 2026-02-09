using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls.Requests.Modals;

public partial class NewRequisition__Modal : ComponentBase
{
    [Parameter] public IRequestDetailVM? RequestHeader { get; set; }
    [Parameter] public IAttachSvc AttachSvc { get; set; } = default!;
    [Parameter] public IRSSlipSvc RSSlipSvc { get; set; } = default!;
    [Parameter] public RequestType RequestType { get; set; }
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }
    [Parameter] public Confirmation__Modal? ConfirmModal { get; set; }

    private Request_RS_SlipVM Model { get; set; } = default!;
    private bool IsPR => RequestType is RequestType.Purchase;
    private ICollection<Request_AttachVM> Attachments { get; set; } = []; 

    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;
    private AlertType CurrentAlertType { get; set; } = AlertType.error;

    private EventCallback<(string Message, AlertType Type)> _onShowAlertCallback;

    protected override void OnParametersSet()
    {
        Model = new()
        {
            RequisitionerId = currentUser.UserId,
            RequisitionerName = currentUser.FullName,
            DateOfRequest = DateTime.Now
        };

        base.OnInitialized();
        // Create once, reuse always
        _onShowAlertCallback = EventCallback.Factory.Create<(string, AlertType)>(this, HandleFormAlert);
    }

    private async void CloseModal()
    {
        await OnClose.InvokeAsync(null);
        ResetForm();
    }

    private async Task HandleSave()
    {
        if (IsInvalidModel()) return;

        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save this requisition slip?",
            Title = "Save Requisition Slip",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Save",
            CancelText = "Cancel",
        };

        var result = await ConfirmModal!.ShowAsync(options);
        if (result)
        {
            await ConfirmModal!.SetLoadingAsync(true);

            _isDisabledBtns = true;
            IsShowAlert = false;

            Model.Purchase_RequestId = RequestType is RequestType.JobOrder ? null : RequestHeader!.Id;
            Model.Job_OrderId = RequestType is RequestType.JobOrder ? RequestHeader!.Id : null;

            var response = await RSSlipSvc.CreateRequisition(Model, RequestType);
            if (response.Success)
            {
                if (Attachments.Where(e => e.AttachType == RequestAttachType.Requisition).Any())
                {
                    var res = await AttachSvc.UploadAsync(RequestHeader!.Id, IsPR, Attachments, RequestAttachType.Requisition, response.Data);
                    if (!res.Success)
                    {
                        toastSvc.ShowError("Error uploading attachments. Please try again.");
                    }
                }

                ResetForm();
                await ConfirmModal!.SetLoadingAsync(false);
                await ConfirmModal!.HideAsync();
                await OnSave.InvokeAsync(null);
                return;
            }

            HandleFormAlert((response.Message, AlertType.error));
            _isDisabledBtns = false;
            await ConfirmModal!.SetLoadingAsync(false);
            await ConfirmModal!.HideAsync();
        }
    }

    private bool IsInvalidModel()
    {
        if (Model.AmountRequested == 0)
        {
            HandleFormAlert(("Amount Requested is Required", AlertType.error));
            return true;
        }
        if (Model.NoOfdaysToLiquidate == 0)
        {
            HandleFormAlert(("No. of Days to Liquidate is Required", AlertType.error));
            return true;
        }
        if (Model.Payment_PayeeId == 0)
        {
            HandleFormAlert(("Payee is Required", AlertType.error));
            return true;
        }
        if (string.IsNullOrEmpty(Model.Payment_PaymentDetails))
        {
            HandleFormAlert(("Payment Details is Required", AlertType.error));
            return true;
        }
        return false;
    }

    private void HandleFormAlert((string Message, AlertType Type) alert)
    {
        AlertMessage = alert.Message;
        CurrentAlertType = alert.Type;
        IsShowAlert = true;
        StateHasChanged();

        // Auto-hide success alerts without async void
        if (alert.Type == AlertType.success)
        {
            Task.Delay(3000).ContinueWith(_ =>
            {
                InvokeAsync(() =>
                {
                    IsShowAlert = false;
                    StateHasChanged();
                });
            });
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