using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Enums;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.PurchaseRequest.Modals;

public partial class EditRequest__Modal : ComponentBase
{
    [Parameter] public Purchase_RequestVM RequestModel { get; set; } = new();
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public bool IsResubmit { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }

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
            var response = await purchaseRequestSvc.UpdatePurchaseRequest(RequestModel.Id, RequestModel);
            if (response.Success)
            {
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

        var resubmitResponse = await purchaseRequestSvc.UpdatePurchaseRequest(RequestModel.Id, RequestModel);
        if (resubmitResponse.Success)
        {
            var command = new ApprovePurchaseRequestCommandVM
            {
                RequestId = RequestModel!.Id,
                Stage = ApprovalStage.DepartmentHead,
                ApproverId = currentUser.UserId,
                Action = ApprovalAction.Approve,
                Remarks = string.IsNullOrEmpty(confirmModal.Remarks) ? "Re-submitted after previous rejection" : $"[Re-submitted] {confirmModal.Remarks}"
            };

            var apiResult = await purchaseRequestSvc.ApprovePurchaseRequest(command);
            if (apiResult.Success)
            {
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

    private string GetLastModifiedDisplay(DateTime? dateModified)
    {
        if (dateModified is null) return "Never modified";

        var now = DateTime.Now;
        var timeSpan = now - dateModified.Value;

        return timeSpan.TotalDays switch
        {
            < 1 when timeSpan.TotalHours < 1 => $"Updated {timeSpan.Minutes}m ago",
            < 1 => $"Updated {timeSpan.Hours}h ago",
            < 7 => $"Updated {timeSpan.Days}d ago",
            < 30 => $"Updated {timeSpan.Days / 7}w ago",
            _ => $"Updated {dateModified.Value:MMM dd, yyyy}"
        };
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