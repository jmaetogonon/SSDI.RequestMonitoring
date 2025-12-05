using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.PurchaseRequest.Modals;

public partial class NewRequest__Modal : ComponentBase
{
    [Parameter] public List<DivisionVM> Divisions { get; set; } = [];
    [Parameter] public List<DepartmentVM> Departments { get; set; } = [];
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }

    private Confirmation__Modal? confirmModal;
    private Purchase_RequestVM RequestModel { get; set; } = new();

    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        if (utils.IsUser())
        {
            RequestModel.Name = currentUser.FullName;
        }
    }

    private async void CloseModal()
    {
        await OnClose.InvokeAsync(null);
        ResetForm();
    }

    private async Task HandleSave()
    {
        if (RequestModel.DepartmentId is 0)
        {
            IsShowAlert = true;
            AlertMessage = "Please select a department.";
            return;
        }
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
            RequestModel.Status = utils.IsSupervisor() ? RequestStatus.ForEndorsement : RequestStatus.Draft;
            RequestModel.DateRequested = DateTime.Now;
            RequestModel.RequestedById = currentUser.UserId;
            RequestModel.RequestedByDeptHeadId = utils.IsSupervisor() ? currentUser.UserId : null;

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
            await confirmModal!.HideAsync();
        }
    }

    private void ResetForm()
    {
        RequestModel = new();
        _isDisabledBtns = false;
    }
}