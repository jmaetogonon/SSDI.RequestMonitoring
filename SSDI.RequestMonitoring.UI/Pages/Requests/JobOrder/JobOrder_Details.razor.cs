using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Requests.Purchase;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.JobOrder;

public partial class JobOrder_Details : ComponentBase
{
    [Parameter] public int paramId { get; set; }
    [Parameter] public string? ReportType { get; set; }

    private Job_OrderVM? Request { get; set; }
    private bool IsLoading { get; set; } = true;
    private bool CanApproveDeptHead => CheckDeptApprovalPermissions();
    private bool CanApproveDivtHead => CheckDivApprovalPermissions();
    private bool CanApproveAdmin => CheckAdminApprovalPermissions();
    private bool CanEdit => CheckEditPermissions();
    private bool CanClose => CheckClosePermission();
    private bool CanCEOApproveSlips => CheckCEOApproveSlipPermission();
    private bool isEditRequestModalVisible = false;
    private string EditBtnText => SetEditBtnText();

    private Confirmation__Modal? confirmModal;

    private List<DivisionVM> divisions = [];
    private List<DepartmentVM> departments = [];

    private string? pdfBase64;
    private bool isPdfModalVisible = false;

    private string ActiveTab = "details";
    public DateTime? awaitingApprovalDate => GetAwaitingApprovalDate();

    protected override async Task OnInitializedAsync()
    {
        divisions = await divisionSvc.GetAllDivisions();
        departments = await departmentSvc.GetAllDepartments();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadRequestDetails();
    }

    private async Task LoadRequestDetails()
    {
        IsLoading = true;
        try
        {
            Request = await jobOrderSvc.GetByIdJobOrder(paramId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            toastSvc.ShowError("Error: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task Reload()
    {
        Request = await jobOrderSvc.GetByIdJobOrder(paramId);
    }

    private void NavigateToEdit()
    {
        Request?.Attachments.Clear();
        isEditRequestModalVisible = true;
    }

    private async Task SubmitRequestByDept()
    {
        var result = await confirmModal!.ShowSubmitJobOrderAsync(Request!.RequestNumber);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.DepartmentHead, ApprovalAction.Approve, "submitted");
    }

    private async Task CancelRequestByDept()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to cancel this job order? This action will pernamently remove the request.",
            Title = "Cancel Job Order",
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
            Message = "Are you sure you want to endorse this job order? Once endorsed, it will be forwarded for the next level of approval.",
            Title = "Endorse Job Order",
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
        var result = await confirmModal!.ShowRejectAsync(Request!.RequestNumber);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.DivisionHead, ApprovalAction.Reject, "rejected");
    }

    private async Task VerifyByAdminRequest()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to verify this job order? Once verified, it will be forwarded for the next level of approval.",
            Title = "Verify Job Order",
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
        var result = await confirmModal!.ShowRejectAsync(Request!.RequestNumber);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.Admin, ApprovalAction.Reject, "rejected");
    }

    private async Task ApproveByCeoRequest()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to approve this job order?",
            Title = "Approve Job Order",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Approve",
            CancelText = "No, Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.CeoOrAvp, ApprovalAction.Approve, "approved");
    }

    private async Task RejectByCeoRequest()
    {
        var result = await confirmModal!.ShowRejectAsync(Request!.RequestNumber);
        if (!result) return;

        await OnApproveRequest(ApprovalStage.CeoOrAvp, ApprovalAction.Reject, "rejected");
    }

    private async Task OnApproveRequest(ApprovalStage stage, ApprovalAction action, string toastText)
    {
        await confirmModal!.SetLoadingAsync(true);

        var command = new ApproveJobOrderCommandVM
        {
            RequestId = Request!.Id,
            Stage = stage,
            ApproverId = currentUser.UserId,
            Action = action,
            Remarks = confirmModal!.Remarks
        };

        var apiResult = await jobOrderSvc.ApproveJobOrder(command);

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

    private async Task RequestClose()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to request closure of this job order? This will ask the requester to confirm within 3 days.",
            Title = "Request Close",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Request Close",
            CancelText = "No, Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (!result) return;

        var apiResult = await jobOrderSvc.InitiateCloseJobOrder(Request!.Id, currentUser.UserId);
        if (apiResult.Success)
        {
            toastSvc.ShowSuccess("Close request sent to requester. It will auto-close in 3 days if no response.");
            await LoadRequestDetails();
        }
        else
            toastSvc.ShowError("Error: " + apiResult.Message);
    }

    private async Task ConfirmClose()
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Confirm closing this request? This action is final.",
            Title = "Confirm Close",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Close",
            CancelText = "No, Cancel",
        };

        var result = await confirmModal!.ShowAsync(options);
        if (!result) return;

        var apiResult = await jobOrderSvc.ConfirmCloseJobOrder(Request!.Id, currentUser.UserId);
        if (apiResult.Success)
        {
            toastSvc.ShowSuccess("Request closed.");
            await LoadRequestDetails();
        }
        else toastSvc.ShowError(apiResult.Message);
    }

    private async Task KeepOpen()
    {
        var result = await confirmModal!.ShowAsync(new ConfirmationModalOptions
        {
            Title = "Keep Open",
            Message = "Do you want to keep this request open?",
            ConfirmText = "Yes, Keep Open",
            CancelText = "Cancel",
            Variant = ConfirmationModalVariant.confirmation
        });

        if (!result) return;

        var apiResult = await jobOrderSvc.CancelPendingCloseJobOrder(Request!.Id, currentUser.UserId);
        if (apiResult.Success)
        {
            toastSvc.ShowSuccess("Request kept open.");
            await LoadRequestDetails();
        }
        else toastSvc.ShowError(apiResult.Message);
    }

    private async Task OnSaveEditReqModal()
    {
        await LoadRequestDetails();
        isEditRequestModalVisible = false;
        Request = await jobOrderSvc.GetByIdJobOrder(paramId);
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

            byte[] pdfBytes;

            var result = await confirmModal!.ShowPdfExportOptionsAsync($"JO{Request.SeriesNumber}.pdf");
            await confirmModal!.SetLoadingAsync(true);

            if (result)
            {
                // Generate the PDF bytes
                pdfBytes = await jobOrderSvc.GenerateJobOrderPdf(Request.Id, true);
            }
            else
            {
                pdfBytes = await jobOrderSvc.GenerateJobOrderPdf(Request.Id, false);
            }

            await CloseModalWithLoading();
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
        navigationManager.NavigateTo("/requests/job-orders");
    }

    private bool CheckDeptApprovalPermissions()
    {
        if (Request == null) return false;
        return (Request.Status == RequestStatus.Draft || Request.Status == RequestStatus.Rejected) && ReportType == "Department" && !currentUser.IsUser;
    }

    private bool CheckDivApprovalPermissions()
    {
        if (Request == null) return false;

        var isSameSupervisor = Request.ReportToDeptSupId == Request.ReportToDivSupId || (!Request.IsNoUser && Request.UserDepartmentHeadId == Request.UserDivisionHeadId);
        if (isSameSupervisor)
        {
            return Request.Status == RequestStatus.ForEndorsement && (Request.UserDivisionHeadId == currentUser.UserId || Request.ReportToDivSupId == currentUser.UserId) && !currentUser.IsUser;
        }
        return Request.Status == RequestStatus.ForEndorsement && ReportType == "Division" && !currentUser.IsUser;
    }

    private bool CheckAdminApprovalPermissions()
    {
        if (Request == null) return false;
        return Request.Status == RequestStatus.ForAdminVerification && currentUser.IsAdmin;
    }

    private bool CheckEditPermissions()
    {
        if (Request == null) return false;

        return (Request.Status == RequestStatus.Draft || Request.Status == RequestStatus.Rejected) && (ReportType == "Department" || (Request.RequestedById == currentUser.UserId && currentUser.IsSupervisor));
    }

    private bool CheckClosePermission()
    {
        if (Request == null) return false;

        // Check if we have any slips at all
        var hasRequisitionSlips = Request.RequisitionSlips?.Count > 0;
        var hasPOSlips = Request.POSlips?.Count > 0;

        if (!hasRequisitionSlips && !hasPOSlips)
            return false;

        // Pre-calculate receipt amounts per requisition
        var receiptAmountsByRequisitionId = Request.Attachments?
            .Where(e => e.AttachType == RequestAttachType.Receipt)
            .GroupBy(e => e.RequisitionId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(e => e.ReceiptAmount)
            ) ?? [];

        // Pre-calculate receipt amounts per PO
        var receiptAmountsByPOId = Request.Attachments?
            .Where(e => e.AttachType == RequestAttachType.Receipt)
            .GroupBy(e => e.POId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(e => e.ReceiptAmount)
            ) ?? [];

        // Check requisition slips
        if (hasRequisitionSlips)
        {
            foreach (var slip in Request.RequisitionSlips!.Where(e => e.Approval != ApprovalAction.Reject))
            {
                // Check for pending approvals
                if (slip.Approval == ApprovalAction.Pending)
                    return false;

                // Check amount match
                var totalReceiptAmount = receiptAmountsByRequisitionId
                    .GetValueOrDefault(slip.Id, 0m);

                if (slip.AmountRequested > totalReceiptAmount)
                    return false;
            }
        }

        // Check PO slips
        if (hasPOSlips)
        {
            foreach (var slip in Request.POSlips!.Where(e => e.Approval != ApprovalAction.Reject))
            {
                // Check for pending approvals
                if (slip.Approval == ApprovalAction.Pending)
                    return false;

                // Check amount match
                var totalReceiptAmount = receiptAmountsByPOId
                    .GetValueOrDefault(slip.Id, 0m);

                if (slip.Total_Amount > totalReceiptAmount)
                    return false;
            }
        }

        return true;
    }

    private bool CheckCEOApproveSlipPermission()
    {
        if (Request == null) return false;
        return (currentUser.IsCEO && CheckSlipsPending());
    }

    private bool CheckSlipsPending()
    {
        if (Request == null) return false;
        return (Request.RequisitionSlips.Any(e => e.Approval == ApprovalAction.Pending) || Request.POSlips.Any(e => e.Approval == ApprovalAction.Pending));
    }

    private DateTime? GetAwaitingApprovalDate()
    {
        if (currentUser.IsUser)
        {
            return null;
        }

        if (Request?.Status == RequestStatus.Rejected)
        {
            return GetLastRejectionDate();
        }

        if (currentUser.IsAdmin && Request?.Status == RequestStatus.ForRequisition)
            return GetLastApprovalDate(ApprovalStage.CeoOrAvp);

        // Determine which approval stage is awaiting based on user capabilities
        return DetermineAwaitingStage() switch
        {
            ApprovalStage.DepartmentHead => Request?.DateCreated,
            ApprovalStage.DivisionHead => GetLastApprovalDate(ApprovalStage.DepartmentHead),
            ApprovalStage.Admin => GetLastApprovalDate(ApprovalStage.DivisionHead),
            ApprovalStage.CeoOrAvp => GetLastApprovalDate(ApprovalStage.Admin),
            _ => null
        };

        DateTime? GetLastRejectionDate()
        {
            return Request?.Approvals?
                .LastOrDefault(a => a.Action == ApprovalAction.Reject)?
                .ActionDate;
        }
        ApprovalStage? DetermineAwaitingStage()
        {
            if (CanApproveDeptHead)
                return ApprovalStage.DepartmentHead;

            if (CanApproveDivtHead)
                return ApprovalStage.DivisionHead;

            if (CanApproveAdmin)
                return DetermineAdminStage();

            if (currentUser.IsCEO && Request?.Status == RequestStatus.ForCeoApproval)
                return ApprovalStage.CeoOrAvp;

            return null;
        }
        ApprovalStage DetermineAdminStage()
        {
            // Admin can act in different roles
            if (CanApproveDeptHead)
                return ApprovalStage.DepartmentHead;

            if (CanApproveDivtHead)
                return ApprovalStage.DivisionHead;

            // Default admin role
            return ApprovalStage.Admin;
        }

        DateTime? GetLastApprovalDate(ApprovalStage stage)
        {
            var approval = Request?.Approvals?
                .LastOrDefault(a => a.Stage == stage &&
                                   a.Action == ApprovalAction.Approve)?
                .ActionDate;

            if (approval == null && stage == ApprovalStage.DepartmentHead)
            {
                approval = Request?.DateCreated;
            }

            return approval;
        }
    }

    private string SetEditBtnText()
    {
        if (Request == null) return "Edit";

        return (Request.Status == RequestStatus.Rejected) ? "Edit to Resubmit" : "Edit";
    }

    private async Task OnCloseEditReqModal()
    {
        Request = await jobOrderSvc.GetByIdJobOrder(paramId);
        isEditRequestModalVisible = false;
    }

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