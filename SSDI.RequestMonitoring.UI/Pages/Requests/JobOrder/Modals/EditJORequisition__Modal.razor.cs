using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.JobOrder.Modals;

public partial class EditJORequisition__Modal : ComponentBase
{
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }
    [Parameter] public Job_OrderVM? PurchaseRequestHeader { get; set; }
    [Parameter] public Job_Order_SlipVM Model { get; set; } = new();

    private Confirmation__Modal? confirmModal;

    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        Model.JobOrderId = PurchaseRequestHeader!.Id;
        Model.RequisitionerId = currentUser.UserId;
        Model.RequisitionerName = currentUser.FullName;
    }

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

            var response = await slipSvc.EditJORequisition(Model);
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
        Model = new();
        _isDisabledBtns = false;
    }
}