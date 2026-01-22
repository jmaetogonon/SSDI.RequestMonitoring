using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.JobOrder.Modals;

public partial class EditJORequest__Modal : ComponentBase
{
    [Parameter] public Job_OrderVM RequestModel { get; set; } = new();
    [Parameter] public List<DivisionVM> Divisions { get; set; } = [];
    [Parameter] public List<DepartmentVM> Departments { get; set; } = [];
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public bool IsResubmit { get; set; }
    [Parameter] public RequestType RequestType { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }

    private bool isPR => RequestType == RequestType.Purchase;
    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    private Confirmation__Modal? confirmModal;

    private async void CloseModal()
    {
        await OnClose.InvokeAsync(null);
        ResetForm();
    }

    private async Task HandleSave()
    {
        _isDisabledBtns = true;
        IsShowAlert = false;
        RequestModel.DateRequested = DateTime.Now;

        ConfirmationModalOptions options = SetModalOptions();

        var result = await confirmModal!.ShowAsync(options);

        if (result && !IsResubmit)
        {
            var response = await jobOrderSvc.UpdateJobOrder(RequestModel.Id, RequestModel);
            if (response.Success)
            {
                var res = await attachSvc.UploadAsync(RequestModel.Id, isPR, RequestModel.Attachments, RequestAttachType.Request);
                if (!res.Success)
                {
                    if (RequestModel.Attachments.Count != 0)
                    {
                        toastSvc.ShowError("Error uploading attachments. Please try again.");
                    }
                }

                ResetForm();
                await OnSave.InvokeAsync(null);
                return;
            }

            IsShowAlert = true;
            AlertMessage = response.Message;
            _isDisabledBtns = false;
            return;
        }

        _isDisabledBtns = false;
        if (!result) return;

        var resubmitResponse = await jobOrderSvc.UpdateJobOrder(RequestModel.Id, RequestModel);
        if (resubmitResponse.Success)
        {
            var command = new ApproveJobOrderCommandVM
            {
                RequestId = RequestModel!.Id,
                Stage = ApprovalStage.DepartmentHead,
                ApproverId = currentUser.UserId,
                Action = ApprovalAction.Approve,
                Remarks = string.IsNullOrEmpty(confirmModal.Remarks) ? "Re-submitted after previous rejection" : $"[Re-submitted] {confirmModal.Remarks}"
            };

            var apiResult = await jobOrderSvc.ApproveJobOrder(command);
            if (apiResult.Success)
            {
                var res = await attachSvc.UploadAsync(RequestModel.Id, isPR, RequestModel.Attachments, RequestAttachType.Request);
                if (!res.Success)
                {
                    toastSvc.ShowError("Error uploading attachments. Please try again.");
                }

                ResetForm();
                await OnSave.InvokeAsync(null);
                return;
            }

            IsShowAlert = true;
            AlertMessage = apiResult.Message;
            _isDisabledBtns = false;
        }
        else
        {
            IsShowAlert = true;
            AlertMessage = resubmitResponse.Message;
            _isDisabledBtns = false;
        }
    }

    private void ResetForm()
    {
        RequestModel = new();
        _isDisabledBtns = false;
    }

    private ConfirmationModalOptions SetModalOptions()
    {
        return IsResubmit ? new ConfirmationModalOptions
        {
            Message = $"Are you sure you want to resubmit purchase request '#{RequestModel.Id}'?<br>Once submitted, it will be forwarded again for endorsement.",
            Title = "Resubmit Purchase Request",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Resubmit",
            CancelText = "No, Cancel",
            ConfirmIcon = "bi bi-send-check",
            ShowRemarksField = true,
            RemarksLabel = "Resubmission Reason",
            RemarksPlaceholder = "Explain what changes were made or why this is being resubmitted...",
        } : new ConfirmationModalOptions
        {
            Message = "Are you sure you want to save the update?",
            Title = "Save Update",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Save",
            CancelText = "Cancel",
        };
    }

    private async Task CloseModalWithLoading()
    {
        await confirmModal!.SetLoadingAsync(false);
        await confirmModal!.HideAsync();
    }
}