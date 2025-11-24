using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls;

public partial class RequestRequisitionSlips__Control : ComponentBase
{
    [Parameter] public Purchase_RequestVM Request { get; set; } = new();
    [Parameter] public Confirmation__Modal ConfirmModal { get; set; } = default!;

    private bool showNewSlipModal, showEditSlipModal = false;
    private bool showPdfModal = false;
    private bool isDownloadingAll = false;
    private Purchase_Request_SlipVM? EditModel = new();
    private Purchase_Request_SlipVM? selectedSlip = null;
    private string? currentPdfBase64 = null;
    private Purchase_Request_SlipVM slipForm = new();
    private HashSet<int> expandedSlips = new();
    private string activeAttachmentTab = "requisition";

    private Dictionary<int, bool> viewLoading = [];
    private Dictionary<int, bool> editLoading = [];
    private Dictionary<int, bool> downloadLoading = [];
    private Dictionary<int, bool> approveLoading = [];
    private Dictionary<int, bool> rejectLoading = [];
    private int currentDownloadIndex = 0;

    private void ToggleSlip(int slipId)
    {
        if (expandedSlips.Contains(slipId))
        {
            expandedSlips.Remove(slipId);
        }
        else
        {
            expandedSlips.Add(slipId);
        }
        StateHasChanged();
    }

    private void SwitchAttachmentTab(string tab)
    {
        activeAttachmentTab = tab;
        StateHasChanged();
    }

    private async Task ApproveSlip(Purchase_Request_SlipVM slip)
    {
        approveLoading[slip.Id] = true;
        StateHasChanged();

        try
        {
            var options = new ConfirmationModalOptions
            {
                Message = $"Approve this <b>{GetSlipDisplayName(slip.RequisitionSlip_For)}</b> slip?",
                Title = "Approve Slip",
                Variant = ConfirmationModalVariant.confirmation,
                ConfirmText = "Yes, Approve",
                CancelText = "No, Cancel"
            };

            var result = await ConfirmModal!.ShowAsync(options);
            if (!result) return;

            await ConfirmModal.SetLoadingAsync(true);

            var response = await slipSvc.ApprovePRRequisition(slip, ApprovalAction.Approve, currentUser.UserId);
            await ConfirmModal.SetLoadingAsync(false);
            await ConfirmModal.HideAsync();

            if (response.Success)
            {
                slip.Approval = ApprovalAction.Approve;
                slip.SlipApproverName = currentUser.FullName;
                slip.SlipApprovalDate = DateTime.Now;
                toastSvc.ShowSuccess("Slip approved successfully.");
            }
            else
            {
                toastSvc.ShowError(response.Message);
            }
        }
        finally
        {
            approveLoading[slip.Id] = false;
            StateHasChanged();
        }
    }

    private async Task RejectSlip(Purchase_Request_SlipVM slip)
    {
        rejectLoading[slip.Id] = true;
        StateHasChanged();
        try
        {
            var options = new ConfirmationModalOptions
            {
                Message = $"Reject this <b>{GetSlipDisplayName(slip.RequisitionSlip_For)}</b> slip?",
                Title = "Reject Slip",
                Variant = ConfirmationModalVariant.confirmation,
                ConfirmText = "Yes, Reject",
                CancelText = "No, Cancel"
            };

            var result = await ConfirmModal.ShowAsync(options);
            if (!result) return;

            await ConfirmModal.SetLoadingAsync(true);

            var response = await slipSvc.ApprovePRRequisition(slip, ApprovalAction.Reject, currentUser.UserId);
            await ConfirmModal.SetLoadingAsync(false);
            await ConfirmModal.HideAsync();

            if (response.Success)
            {
                slip.Approval = ApprovalAction.Reject;
                slip.SlipApproverName = currentUser.FullName;
                slip.SlipApprovalDate = DateTime.Now;
                toastSvc.ShowSuccess("Slip rejected.");
            }
            else
            {
                toastSvc.ShowError(response.Message);
            }
        }
        finally
        {
            rejectLoading[slip.Id] = false;
            StateHasChanged();
        }
    }

    private void AddNewSlip()
    {
        slipForm = new Purchase_Request_SlipVM
        {
            PurchaseRequestId = Request.Id,
            DateOfRequest = DateTime.Today
        };
        showNewSlipModal = true;
    }

    private void EditSlip(Purchase_Request_SlipVM slip)
    {
        EditModel = slip;
        showEditSlipModal = true;
    }

    private async Task SaveNewSlip()
    {
        Request = await purchaseRequestSvc.GetByIdPurchaseRequest(Request.Id);
        showNewSlipModal = false;
        toastSvc.ShowSuccess("Requisition slip successfully added.");
        StateHasChanged();
    }

    private async Task SaveEditSlip()
    {
        Request = await purchaseRequestSvc.GetByIdPurchaseRequest(Request.Id);
        showEditSlipModal = false;
        toastSvc.ShowSuccess("Requisition slip successfully updated.");
        StateHasChanged();
    }

    private void CloseSlipModal() => showNewSlipModal = showEditSlipModal = false;

    private async Task ViewSlip(Purchase_Request_SlipVM slip)
    {
        try
        {
            if (Request == null)
            {
                toastSvc.ShowError("No request details to export.");
                return;
            }
            viewLoading[slip.Id] = true;
            StateHasChanged();

            // Generate the PDF bytes
            var pdfBytes = await slipSvc.GeneratePRRequisitionPdf(slip.Id);
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                toastSvc.ShowError("Failed to generate PDF.");
                return;
            }

            currentPdfBase64 = Convert.ToBase64String(pdfBytes);
            showPdfModal = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            toastSvc.ShowError($"Error exporting PDF: {ex.Message}");
        }
        finally
        {
            viewLoading[slip.Id] = false;
            StateHasChanged();
        }
    }

    private async Task DownloadSlipPdf(Purchase_Request_SlipVM slip)
    {
        try
        {
            downloadLoading[slip.Id] = true;
            StateHasChanged();

            // Generate and download PDF
            var pdfBytes = await slipSvc.GeneratePRRequisitionPdf(slip.Id);
            var pdfBase64 = Convert.ToBase64String(pdfBytes);
            await JS.InvokeVoidAsync("downloadBase64File", "application/pdf", pdfBase64, $"{Request.Name}_RequisitionSlip_{slip.Id}.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading slip PDF: {ex.Message}");
        }
        finally
        {
            downloadLoading[slip.Id] = false;
            StateHasChanged();
        }
    }

    private async Task DownloadAllAsPdf()
    {
        if (Request.RequisitionSlips.Count == 0) return;

        isDownloadingAll = true;
        currentDownloadIndex = 0;
        StateHasChanged();

        try
        {
            foreach (var attachment in Request.RequisitionSlips)
            {
                currentDownloadIndex++;
                StateHasChanged();

                await DownloadSlipPdf(attachment);
                await Task.Delay(200);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading all slips: {ex.Message}");
        }
        finally
        {
            isDownloadingAll = false;
            currentDownloadIndex = 0;
            StateHasChanged();
        }
    }

    private async Task DeleteSlip(Purchase_Request_SlipVM slip)
    {
        var options = new ConfirmationModalOptions
        {
            Message = $"Are you sure you want to delete this <b>{GetSlipDisplayName(slip.RequisitionSlip_For)}</b> requisition slip?",
            Title = "Delete Requisition Slip",
            Variant = ConfirmationModalVariant.delete,
            ConfirmText = "Yes, Delete",
            CancelText = "No, Cancel",
        };

        var result = await ConfirmModal!.ShowAsync(options);

        if (result)
        {
            await ConfirmModal!.SetLoadingAsync(true);

            var response = await slipSvc.DeletePRRequisition(slip.Id);
            if (response.Success)
            {
                await ConfirmModal!.SetLoadingAsync(false);
                await ConfirmModal!.HideAsync();
                Request.RequisitionSlips.Remove(slip);
                toastSvc.ShowSuccess("The requisition slip has been deleted successfully.");
            }
            else
            {
                await ConfirmModal!.SetLoadingAsync(false);
                await ConfirmModal!.HideAsync();
                toastSvc.ShowError(response.Message);
            }
        }
    }

    private void ClosePdfModal()
    {
        showPdfModal = false;
        selectedSlip = null;
        currentPdfBase64 = null;
    }

    // Attachment Methods
    private string GetFileIcon(string contentType)
    {
        if (contentType.Contains("image"))
            return "bi-file-image";
        if (contentType.Contains("pdf"))
            return "bi-file-pdf";
        if (contentType.Contains("word") || contentType.Contains("document"))
            return "bi-file-word";
        if (contentType.Contains("excel") || contentType.Contains("spreadsheet"))
            return "bi-file-excel";
        return "bi-file-earmark";
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private async Task ViewAttachment(Purchase_Request_AttachVM attachment)
    {
        try
        {
            await JS.InvokeVoidAsync("open", attachment.URL, "_blank");
        }
        catch (Exception ex)
        {
            toastSvc.ShowError($"Error opening file: {ex.Message}");
        }
    }

    private async Task DownloadAttachment(Purchase_Request_AttachVM attachment)
    {
        try
        {
            await JS.InvokeVoidAsync("downloadFileFromUrl", attachment.URL, attachment.FileName);
        }
        catch (Exception ex)
        {
            toastSvc.ShowError($"Error downloading file: {ex.Message}");
        }
    }

    private string GetSlipIcon(RequisitionSlip_For slipFor)
    {
        return slipFor switch
        {
            RequisitionSlip_For.CashPayment => "bi-cash",
            RequisitionSlip_For.CheckPayment => "bi-credit-card",
            RequisitionSlip_For.Advances => "bi-cash-coin",
            RequisitionSlip_For.Others => "bi-file-earmark",
            _ => "bi-file-earmark"
        };
    }

    private string GetSlipDisplayName(RequisitionSlip_For slipFor)
    {
        return slipFor switch
        {
            RequisitionSlip_For.CashPayment => "Cash Payment",
            RequisitionSlip_For.CheckPayment => "Check Payment",
            RequisitionSlip_For.Advances => "Cash Advances",
            RequisitionSlip_For.Others => "Others",
            _ => slipFor.ToString()
        };
    }

    private string GetDeptDisplayName(RequisitionSlip_Dept dept)
    {
        return dept switch
        {
            RequisitionSlip_Dept.Acctg => "Acctg. Dept.",
            RequisitionSlip_Dept.Sales => "Sales Dept.",
            RequisitionSlip_Dept.Warehouse => "Warehouse Dept.",
            RequisitionSlip_Dept.Satellite => "Satellite",
            _ => dept.ToString()
        };
    }
}