using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls.JobOrder;

public partial class JobOrderRequisitionSlips__Control : ComponentBase
{
    [Parameter] public Job_OrderVM Request { get; set; } = new();
    [Parameter] public Confirmation__Modal ConfirmModal { get; set; } = default!;

    private bool showNewSlipModal, showEditSlipModal = false;
    private bool showPdfModal = false;
    private bool isDownloadingAll = false;
    private Job_Order_SlipVM? EditModel = new();
    private Job_Order_SlipVM? selectedSlip = null;
    private string? currentPdfBase64 = null;
    private Job_Order_SlipVM slipForm = new();
    private HashSet<int> expandedSlips = new();
    private string activeAttachmentTab = "requisition";

    private Dictionary<int, bool> viewLoading = [];
    private Dictionary<int, bool> editLoading = [];
    private Dictionary<int, bool> downloadLoading = [];
    private Dictionary<int, bool> approveLoading = [];
    private Dictionary<int, bool> rejectLoading = [];
    private int currentDownloadIndex = 0;

    private bool showPreview = false;
    private Job_Order_AttachVM? selectedAttachment;
    private string? previewUrl;
    private List<string> blobUrls = [];
    private bool isLoadingPreview = false;
    private HashSet<string> downloadingAttachments = [];

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

    private async Task ApproveSlip(Job_Order_SlipVM slip)
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

            var response = await slipSvc.ApproveJORequisition(slip, ApprovalAction.Approve, currentUser.UserId);
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

    private async Task RejectSlip(Job_Order_SlipVM slip)
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

            var response = await slipSvc.ApproveJORequisition(slip, ApprovalAction.Reject, currentUser.UserId);
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
        slipForm = new Job_Order_SlipVM
        {
            JobOrderId = Request.Id,
            DateOfRequest = DateTime.Today
        };
        showNewSlipModal = true;
    }

    private void EditSlip(Job_Order_SlipVM slip)
    {
        EditModel = slip;
        showEditSlipModal = true;
    }

    private async Task SaveNewSlip()
    {
        Request = await purchaseRequestSvc.GetByIdJobOrder(Request.Id);
        showNewSlipModal = false;
        toastSvc.ShowSuccess("Requisition slip successfully added.");
        StateHasChanged();
    }

    private async Task SaveEditSlip()
    {
        Request = await purchaseRequestSvc.GetByIdJobOrder(Request.Id);
        showEditSlipModal = false;
        toastSvc.ShowSuccess("Requisition slip successfully updated.");
        StateHasChanged();
    }

    private void CloseSlipModal() => showNewSlipModal = showEditSlipModal = false;

    private async Task ViewSlip(Job_Order_SlipVM slip)
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
            var pdfBytes = await slipSvc.GenerateJORequisitionPdf(slip.Id);
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

    private async Task DownloadSlipPdf(Job_Order_SlipVM slip)
    {
        try
        {
            downloadLoading[slip.Id] = true;
            StateHasChanged();

            // Generate and download PDF
            var pdfBytes = await slipSvc.GenerateJORequisitionPdf(slip.Id);
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

    private async Task DeleteSlip(Job_Order_SlipVM slip)
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

            var response = await slipSvc.DeleteJORequisition(slip.Id);
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

    private async Task AddRequisitionAttach(Job_Order_SlipVM slip, InputFileChangeEventArgs e)
    {
        var dummies = new List<Job_Order_AttachVM>();

        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to upload the selected files/?",
            Title = "Upload Requisition Attachment",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Upload",
            CancelText = "No, Cancel",
        };

        var result = await ConfirmModal!.ShowAsync(options);
        if (!result) return;

        foreach (var file in e.GetMultipleFiles())
        {
            if (file.Size > 10 * 1024 * 1024)
                continue; // skip >10MB

            byte[] fileBytes;
            using (var stream = file.OpenReadStream(10 * 1024 * 1024)) // 10MB limit
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            var attachVM = new Job_Order_AttachVM
            {
                UniqId = utils.GenerateUniqId(),
                JobOrderId = Request.Id,
                FileName = file.Name,
                ContentType = file.ContentType,
                ImgData = fileBytes,
                Size = file.Size,
                AttachType = RequestAttachType.Requisition
            };

            dummies.Add(attachVM);
        }

        var command = new UploadAttachmentJobOrderCommandVM
        {
            PurchaseRequestId = Request!.Id,
            Files = dummies,
            Type = RequestAttachType.Requisition,
            RequisitionId = slip.Id
        };

        var res = await attachSvc.UploadAttachPurchase(command);
        if (!res.Success)
        {
            toastSvc.ShowError("Error uploading attachments. Please try again.");
        }

        Request = await purchaseRequestSvc.GetByIdJobOrder(Request.Id);
        StateHasChanged();
    }

    private async Task AddReceiptAttach(Job_Order_SlipVM slip, InputFileChangeEventArgs e)
    {
        var file = e.File; // direct single file

        if (file == null)
            return;

        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to upload this file?",
            Title = "Upload Receipt",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Upload",
            CancelText = "No, Cancel",
            ShowNumberField = true,
            NumberRequired = true,
            NumberLabel = "Receipt Amount",
            NumberPlaceholder = "Please enter a valid amount...",
        };

        var result = await ConfirmModal!.ShowAsync(options);
        if (!result) return;

        if (file.Size > 10 * 1024 * 1024)
        {
            toastSvc.ShowError("File is larger than 10MB.");
            return;
        }

        byte[] fileBytes;
        using (var stream = file.OpenReadStream(10 * 1024 * 1024))
        using (var ms = new MemoryStream())
        {
            await stream.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        var attach = new Job_Order_AttachVM
        {
            UniqId = utils.GenerateUniqId(),
            JobOrderId = Request.Id,
            FileName = file.Name,
            ContentType = file.ContentType,
            ImgData = fileBytes,
            Size = file.Size,
            AttachType = RequestAttachType.Receipt,
            ReceiptAmount = ConfirmModal.Number
        };

        var command = new UploadAttachmentJobOrderCommandVM
        {
            PurchaseRequestId = Request.Id,
            Files = new List<Job_Order_AttachVM> { attach },
            Type = RequestAttachType.Receipt,
            RequisitionId = slip.Id,
            ReceiptAmount = ConfirmModal.Number
        };

        var res = await attachSvc.UploadAttachPurchase(command);
        if (!res.Success)
        {
            toastSvc.ShowError("Error uploading attachments. Please try again.");
        }

        Request = await purchaseRequestSvc.GetByIdJobOrder(Request.Id);
        StateHasChanged();
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

    private async Task ViewAttachment(Job_Order_AttachVM attachment, MouseEventArgs? e = null)
    {
        selectedAttachment = attachment;
        isLoadingPreview = true;
        showPreview = true;
        StateHasChanged();

        try
        {
            if (IsPdf(attachment.FileName))
            {
                previewUrl = await GetPdfBlobUrl(attachment);
            }
            else if (IsImage(attachment.FileName))
            {
                previewUrl = await GetImageDataUrl(attachment);
            }
            else
            {
                previewUrl = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading preview: {ex.Message}");
            previewUrl = null;
        }
        finally
        {
            isLoadingPreview = false;
            StateHasChanged();
        }
    }

    private async Task DownloadAttachment(Job_Order_AttachVM attachment, MouseEventArgs? e = null)
    {
        if (downloadingAttachments.Contains(attachment.Id.ToString()))
            return;

        downloadingAttachments.Add(attachment.Id.ToString());
        StateHasChanged();

        try
        {
            var fileBytes = await attachSvc.GetAttachByte(attachment.Id);
            var fileName = attachment.FileName;

            if (fileBytes != null && fileBytes.Length > 0)
            {
                await JS.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading attachment: {ex.Message}");
        }
        finally
        {
            downloadingAttachments.Remove(attachment.Id.ToString());
            StateHasChanged();
        }
    }

    private async Task<string> GetPdfBlobUrl(Job_Order_AttachVM attachment)
    {
        try
        {
            var bytes = await attachSvc.GetAttachByte(attachment.Id);
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var base64 = Convert.ToBase64String(bytes);
            var blobUrl = await JS.InvokeAsync<string>("createBlobUrl", base64, "application/pdf");

            if (!string.IsNullOrEmpty(blobUrl))
            {
                blobUrls.Add(blobUrl);
            }

            return blobUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating blob URL: {ex.Message}");
            return string.Empty;
        }
    }

    private async Task<string> GetImageDataUrl(Job_Order_AttachVM attachment)
    {
        try
        {
            var bytes = await attachSvc.GetAttachByte(attachment.Id);
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var ext = Path.GetExtension(attachment.FileName).ToLower();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            var base64 = Convert.ToBase64String(bytes);
            return $"data:{contentType};base64,{base64}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating data URL: {ex.Message}");
            return string.Empty;
        }
    }

    private bool IsImage(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp";
    }

    private bool IsPdf(string fileName)
    {
        return Path.GetExtension(fileName).ToLower() == ".pdf";
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

    private async Task CloseAttachmentPreview()
    {
        showPreview = false;
        selectedAttachment = new();
        previewUrl = null;
        isLoadingPreview = false;

        // Clean up any blob URLs
        foreach (var blobUrl in blobUrls)
        {
            if (!string.IsNullOrEmpty(blobUrl))
            {
                try
                {
                    await JS.InvokeVoidAsync("revokeBlobUrl", blobUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error revoking blob URL: {ex.Message}");
                }
            }
        }
        blobUrls.Clear();

        StateHasChanged();
        await Task.Delay(100); // Small delay to ensure clean state
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var blobUrl in blobUrls)
        {
            if (!string.IsNullOrEmpty(blobUrl))
            {
                try
                {
                    await JS.InvokeVoidAsync("revokeBlobUrl", blobUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error revoking blob URL: {ex.Message}");
                }
            }
        }
        blobUrls.Clear();
    }
}