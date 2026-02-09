using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.JobOrder.Modals;

public partial class NewJORequest__Modal : ComponentBase
{
    [Parameter] public List<DivisionVM> Divisions { get; set; } = [];
    [Parameter] public List<DepartmentVM> Departments { get; set; } = [];
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }
    [Parameter] public Confirmation__Modal? ConfirmModal { get; set; }

    private Job_OrderVM RequestModel { get; set; } = new();

    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        if (currentUser.IsUser)
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

        if (RequestModel.BusinessUnitId == 0)
        {
            IsShowAlert = true;
            AlertMessage = "Please select a business unit.";
            return;
        }

        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save this job order?",
            Title = "Save Job Order",
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
            RequestModel.Status = currentUser.IsSupervisor ? RequestStatus.ForEndorsement : RequestStatus.Draft;
            RequestModel.DateRequested = DateTime.Now;
            RequestModel.RequestedById = currentUser.UserId;
            RequestModel.RequestedByDeptHeadId = currentUser.IsSupervisor ? currentUser.UserId : null;

            var response = await jobOrderSvc.CreateJobOrder(RequestModel);
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

    private void ResetForm()
    {
        RequestModel = new();
        _isDisabledBtns = false;
    }
}