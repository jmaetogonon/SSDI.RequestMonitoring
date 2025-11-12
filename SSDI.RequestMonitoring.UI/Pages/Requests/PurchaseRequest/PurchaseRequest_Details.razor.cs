using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.PurchaseRequest;

public partial class PurchaseRequest_Details : ComponentBase
{
    [Parameter] public int paramId { get; set; }
    [Parameter] public string? ReportType { get; set; }

    private Purchase_RequestVM? Request { get; set; }
    private bool IsLoading { get; set; } = true;
    private bool CanApproveDeptHead => CheckDeptApprovalPermissions();
    private bool CanApproveDivtHead => CheckDivApprovalPermissions();
    private bool CanApproveAdmin => CheckAdminApprovalPermissions();
    private bool CanEdit => CheckEditPermissions();
    private bool isEditRequestModalVisible = false;
    private string EditBtnText => SetEditBtnText();

    private Confirmation__Modal? confirmModal;


    private string? pdfBase64;
    private bool isPdfModalVisible = false;

    private string ActiveTab = "details";

    protected override async Task OnParametersSetAsync()
    {
        await LoadRequestDetails();
    }

    private async Task LoadRequestDetails()
    {
        IsLoading = true;
        try
        {
            Request = await purchaseRequestSvc.GetByIdPurchaseRequest(paramId);
        }
        catch (Exception ex)
        {
            toastSvc.ShowError("Error: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void NavigateToEdit()
    {
        isEditRequestModalVisible = true;
    }

    private async Task SubmitRequestByDept()
    {
        var result = await confirmModal!.ShowSubmitAsync(Request!.Id);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.DepartmentHead, ApprovalAction.Approve, "submitted");
    }

    private async Task CancelRequestByDept()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to cancel this purchase request? This action will pernamently remove the request.",
            Title = "Cancel Purchase Request",
            Variant = ConfirmationModalVariant.warning,
            ConfirmText = "Yes, Cancel It",
            CancelText = "No, Keep It",
            ConfirmIcon = "bi bi-x-lg",
            ShowRemarksField = true,
            RemarksRequired = true,
            RemarksLabel = "Cancellation Reason",
            RemarksPlaceholder = "Please provide a reason for cancelling this request..."
        };

        var result = await confirmModal!.ShowAsync(options);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.DepartmentHead, ApprovalAction.Cancel, "cancelled");
    }

    private async Task EndorseRequest()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to endorse this purchase request? Once endorsed, it will be forwarded for the next level of approval.",
            Title = "Endorse Purchase Request",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Endorse",
            CancelText = "No, Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.DivisionHead, ApprovalAction.Approve, "endorsed");
    }

    private async Task RejectEndorseRequest()
    {
        var result = await confirmModal!.ShowRejectAsync(Request!.Id);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.DivisionHead, ApprovalAction.Reject, "rejected");
    }

    private async Task VerifyByadminRequest()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to verify this purchase request? Once verified, it will be forwarded for the next level of approval.",
            Title = "Verify Purchase Request",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Verify",
            CancelText = "No, Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.Admin, ApprovalAction.Approve, "verified");
    }

    private async Task RejectVerifyByAdminRequest()
    {
        var result = await confirmModal!.ShowRejectAsync(Request!.Id);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.Admin, ApprovalAction.Reject, "rejected");
    }

    private async Task OnApproveRequest(ApprovalStage stage, ApprovalAction action, string toastText)
    {
        await confirmModal!.SetLoadingAsync(true);

        var command = new ApprovePurchaseRequestCommandVM
        {
            RequestId = Request!.Id,
            Stage = stage,
            ApproverId = currentUser.UserId,
            Action = action,
            Remarks = confirmModal!.Remarks
        };

        var apiResult = await purchaseRequestSvc.ApprovePurchaseRequest(command);

        await CloseModalWithLoading();

        if (apiResult.Success)
        {
            await LoadRequestDetails();
            toastSvc.ShowSuccess($"The request has been {toastText} successfully.");
        }
        else
        {
            toastSvc.ShowError("Error: " + apiResult.Message);
        }
    }

    private async Task OnSaveEditReqModal()
    {
        await LoadRequestDetails();
        isEditRequestModalVisible = false;
        toastSvc.ShowSuccess("The request has been updated successfully.");
    }

    private async Task ExportToPdf()
    {
        try
        {
            if (Request == null)
            {
                toastSvc.ShowError("No request details to export.");
                return;
            }

            // Generate the PDF bytes
            var pdfBytes = await purchaseRequestSvc.GeneratePurchaseRequestPdf(Request.Id);
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                toastSvc.ShowError("Failed to generate PDF.");
                return;
            }

            pdfBase64 = Convert.ToBase64String(pdfBytes);
            isPdfModalVisible = true;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            toastSvc.ShowError($"Error exporting PDF: {ex.Message}");
        }
    }
    private void ClosePdfModal()
    {
        isPdfModalVisible = false;
        pdfBase64 = null;
    }

    private void NavigateBack()
    {
        navigationManager.NavigateTo("/requests/purchase-requests");
    }

    private bool CheckDeptApprovalPermissions()
    {
        if (Request == null) return false;
        return (Request.Status == RequestStatus.Draft || Request.Status == RequestStatus.Rejected) && ReportType == "Department" && !utils.IsUser();
    }

    private bool CheckDivApprovalPermissions()
    {
        if (Request == null) return false;
        return Request.Status == RequestStatus.ForEndorsement && ReportType == "Division" && !utils.IsUser();
    }

    private bool CheckAdminApprovalPermissions()
    {
        if (Request == null) return false;
        return Request.Status == RequestStatus.ForAdminVerification && utils.IsAdmin();
    }

    private bool CheckEditPermissions()
    {
        if (Request == null) return false;

        return (Request.Status == RequestStatus.Draft || Request.Status == RequestStatus.Rejected) && ReportType == "Department";
    }

    private string SetEditBtnText()
    {
        if (Request == null) return "Edit";

        return (Request.Status == RequestStatus.Rejected) ? "Edit to Resubmit" : "Edit";
    }

    private void OnCloseEditReqModal() => isEditRequestModalVisible = false;

    private async Task CloseModalWithLoading()
    {
        await confirmModal!.SetLoadingAsync(false);
        await confirmModal!.HideAsync();
    }

    private void SetActiveTab(string tab)
    {
        ActiveTab = tab;
        StateHasChanged();
    }
}