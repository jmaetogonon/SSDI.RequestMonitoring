using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls.Requests;

public partial class RequestRequisition__Control : ComponentBase, IAsyncDisposable
{
    [Parameter] public IRequestDetailVM Request { get; set; } = default!;
    [Parameter] public IAttachSvc AttachSvc { get; set; } = default!;
    [Parameter] public IRSSlipSvc RSSlipSvc { get; set; } = default!;
    [Parameter] public EventCallback OnRequestChanged { get; set; }
    [Parameter] public RequestType RequestType { get; set; }
    [Parameter] public Confirmation__Modal ConfirmModal { get; set; } = default!;

    private bool IsPR => RequestType == RequestType.Purchase;
    private bool _showNewSlipModal, _showEditSlipModal = false;
    private bool _showPdfModal = false;
    private bool _isDownloadingAll = false;
    private Request_RS_SlipVM? _editModel = default!;
    private Request_RS_SlipVM? _selectedSlip = null;
    private string? _currentPdfBase64 = null;
    private readonly HashSet<int> _expandedSlips = [];
    private string _activeAttachmentTab = "receipts";

    private readonly Dictionary<int, bool> _viewLoading = [];
    private readonly Dictionary<int, bool> _editLoading = [];
    private readonly Dictionary<int, bool> _downloadLoading = [];
    private readonly Dictionary<int, bool> _approveLoading = [];
    private readonly Dictionary<int, bool> _rejectLoading = [];
    private int _currentDownloadIndex = 0;

    private bool _showPreview = false;
    private Request_AttachVM? _selectedAttachment;
    private string? _previewUrl;
    private readonly List<string> _blobUrls = [];
    private bool _isLoadingPreview = false;
    private readonly HashSet<string> _downloadingAttachments = [];
    private readonly HashSet<string> _deletingAttachments = [];
    private bool _disposed;

    private async Task Refresh()
    { if (OnRequestChanged.HasDelegate) await OnRequestChanged.InvokeAsync(); }

    private void ToggleSlip(int slipId)
    {
        if (!_expandedSlips.Remove(slipId))
        {
            _expandedSlips.Add(slipId);
        }
        StateHasChanged();
    }

    private void SwitchAttachmentTab(string tab)
    {
        _activeAttachmentTab = tab;
        StateHasChanged();
    }

    private async Task ApproveSlip(Request_RS_SlipVM slip)
    {
        _approveLoading[slip.Id] = true;
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

            var response = await RSSlipSvc.ApproveRequisition(slip, ApprovalAction.Approve, currentUser.UserId);
            await ConfirmModal.SetLoadingAsync(false);
            await ConfirmModal.HideAsync();

            if (response.Success)
            {
                await Refresh();
                toastSvc.ShowSuccess("Slip approved successfully.");
            }
            else
            {
                toastSvc.ShowError(response.Message);
            }
        }
        finally
        {
            _approveLoading[slip.Id] = false;
            StateHasChanged();
        }
    }

    private async Task RejectSlip(Request_RS_SlipVM slip)
    {
        _rejectLoading[slip.Id] = true;
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

            var response = await RSSlipSvc.ApproveRequisition(slip, ApprovalAction.Reject, currentUser.UserId);
            await ConfirmModal.SetLoadingAsync(false);
            await ConfirmModal.HideAsync();

            if (response.Success)
            {
                await Refresh();
                toastSvc.ShowSuccess("Slip rejected.");
            }
            else
            {
                toastSvc.ShowError(response.Message);
            }
        }
        finally
        {
            _rejectLoading[slip.Id] = false;
            StateHasChanged();
        }
    }

    private void AddNewSlip() => _showNewSlipModal = true;

    private void EditSlip(Request_RS_SlipVM slip)
    {
        _editModel = slip;
        _showEditSlipModal = true;
    }

    private async Task SaveNewSlip()
    {
        await Refresh();
        _showNewSlipModal = false;
        toastSvc.ShowSuccess("Requisition slip successfully added.");
        StateHasChanged();
    }

    private async Task SaveEditSlip()
    {
        await Refresh();
        _showEditSlipModal = false;
        toastSvc.ShowSuccess("Requisition slip successfully updated.");
        StateHasChanged();
    }

    private void CloseSlipModal() => _showNewSlipModal = _showEditSlipModal = false;

    private async Task ViewSlip(Request_RS_SlipVM slip)
    {
        try
        {
            if (Request == null)
            {
                toastSvc.ShowError("No request details to export.");
                return;
            }

            _selectedSlip = slip;
            _viewLoading[slip.Id] = true;
            StateHasChanged();

            // Generate the PDF bytes
            var pdfBytes = await RSSlipSvc.GenerateRequisitionPdf(slip.Id, Request.BusinessUnitCode);
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                toastSvc.ShowError("Failed to generate PDF.");
                return;
            }

            _currentPdfBase64 = Convert.ToBase64String(pdfBytes);
            _showPdfModal = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            toastSvc.ShowError($"Error exporting PDF: {ex.Message}");
        }
        finally
        {
            _viewLoading[slip.Id] = false;
            StateHasChanged();
        }
    }

    private async Task DownloadSlipPdf(Request_RS_SlipVM slip)
    {
        try
        {
            _downloadLoading[slip.Id] = true;
            StateHasChanged();

            // Generate and download PDF
            var pdfBytes = await RSSlipSvc.GenerateRequisitionPdf(slip.Id, Request.BusinessUnitCode);
            var pdfBase64 = Convert.ToBase64String(pdfBytes);
            await jsRuntime.InvokeVoidAsync("downloadBase64File", "application/pdf", pdfBase64, $"R{Request.SeriesNumber}_Reqslip_{slip.Id}.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading slip PDF: {ex.Message}");
        }
        finally
        {
            _downloadLoading[slip.Id] = false;
            StateHasChanged();
        }
    }

    private async Task DownloadAllAsPdf()
    {
        if (Request.RequisitionSlips?.Count == 0) return;

        _isDownloadingAll = true;
        _currentDownloadIndex = 0;
        StateHasChanged();

        try
        {
            var fileBytes = await AttachSvc.DownloadAllRSZipAsync(Request.Id, IsPR);
            var fileName = $"{(IsPR ? "PR" : "JO")}{Request.SeriesNumber} Requisition Slips.zip";

            if (fileBytes != null && fileBytes.Length > 0)
            {
                await jsRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading all slips: {ex.Message}");
        }
        finally
        {
            _isDownloadingAll = false;
            _currentDownloadIndex = 0;
            StateHasChanged();
        }
    }

    private async Task DeleteSlip(Request_RS_SlipVM slip)
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

            var response = await RSSlipSvc.DeleteRequisition(slip.Id);
            if (response.Success)
            {
                foreach (var attach in Request.Attachments.Where(a => a.RequisitionId == slip.Id).ToList())
                {
                    await AttachSvc.DeleteAsync(attach.Id);
                }

                await ConfirmModal!.SetLoadingAsync(false);
                await ConfirmModal!.HideAsync();
                await Refresh();
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

    private async Task AddRequisitionAttach(Request_RS_SlipVM slip, InputFileChangeEventArgs e)
    {
        var dummies = new List<Request_AttachVM>();

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

        await ConfirmModal.SetLoadingAsync(true);

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

            var attach = new Request_AttachVM
            {
                UniqId = Utils.GenerateUniqId(),
                FileName = file.Name,
                ContentType = file.ContentType,
                ImgData = fileBytes,
                Size = file.Size,
                AttachType = RequestAttachType.Requisition,
            };

            dummies.Add(attach);
        }

        var res = await AttachSvc.UploadAsync(Request.Id, IsPR, dummies, RequestAttachType.Requisition, slip.Id);
        if (!res.Success)
        {
            toastSvc.ShowError("Error uploading attachments. Please try again.");
        }

        await ConfirmModal.SetLoadingAsync(false);
        await ConfirmModal.HideAsync();
        await Refresh();
        StateHasChanged();
    }

    private async Task AddReceiptAttach(Request_RS_SlipVM slip, InputFileChangeEventArgs e)
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
            ShowRemarksField = true,
            RemarksRequired = true,
        };

        var result = await ConfirmModal!.ShowAsync(options);
        if (!result) return;

        await ConfirmModal.SetLoadingAsync(true);

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

        var attach = new Request_AttachVM
        {
            UniqId = Utils.GenerateUniqId(),
            FileName = file.Name,
            ContentType = file.ContentType,
            ImgData = fileBytes,
            Size = file.Size,
            AttachType = RequestAttachType.Receipt,
            ReceiptAmount = ConfirmModal.Number,
            ReceiptRemarks = ConfirmModal.Remarks
        };

        var res = await AttachSvc.UploadAsync(Request.Id, IsPR, [attach], RequestAttachType.Receipt, slip.Id, ConfirmModal.Number, ConfirmModal.Remarks);
        if (!res.Success)
        {
            toastSvc.ShowError("Error uploading attachments. Please try again.");
        }

        await ConfirmModal.SetLoadingAsync(false);
        await ConfirmModal.HideAsync();
        await Refresh();
        StateHasChanged();
    }

    private void ClosePdfModal()
    {
        _showPdfModal = false;
        _selectedSlip = null;
        _currentPdfBase64 = null;
    }

    // Attachment Methods
    private static string GetFileIcon(string contentType)
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

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private async Task ViewAttachment(Request_AttachVM attachment)
    {
        _selectedAttachment = attachment;
        _isLoadingPreview = true;
        _showPreview = true;
        StateHasChanged();

        try
        {
            if (IsPdf(attachment.FileName))
            {
                _previewUrl = await GetPdfBlobUrl(attachment);
            }
            else if (IsImage(attachment.FileName))
            {
                _previewUrl = await GetImageDataUrl(attachment);
            }
            else
            {
                _previewUrl = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading preview: {ex.Message}");
            _previewUrl = null;
        }
        finally
        {
            _isLoadingPreview = false;
            StateHasChanged();
        }
    }

    private async Task DownloadAttachment(Request_AttachVM attachment)
    {
        if (_downloadingAttachments.Contains(attachment.Id.ToString()))
            return;

        _downloadingAttachments.Add(attachment.Id.ToString());
        StateHasChanged();

        try
        {
            var fileBytes = await AttachSvc.GetAttachByte(attachment.Id);
            var fileName = attachment.FileName;

            if (fileBytes != null && fileBytes.Length > 0)
            {
                await jsRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading attachment: {ex.Message}");
        }
        finally
        {
            _downloadingAttachments.Remove(attachment.Id.ToString());
            StateHasChanged();
        }
    }

    private async Task DeleteAttachment(Request_AttachVM attachment)
    {
        if (!CanDeleteAttachment())
            return;

        var options = new ConfirmationModalOptions
        {
            Title = $"Delete {(attachment.AttachType == RequestAttachType.Receipt ? "Receipt" : "Attachment")}",
            Message = $"Are you sure you want to delete '{attachment.FileName}' {(attachment.AttachType == RequestAttachType.Receipt ? $"w/ <b>{attachment.ReceiptAmount:N2}</b> amount" : "")}?",
            Variant = ConfirmationModalVariant.delete,
            ConfirmText = "Yes, Delete",
            CancelText = "No, Cancel",
        };

        var result = await ConfirmModal!.ShowAsync(options);
        if (!result) return;

        await ConfirmModal.SetLoadingAsync(true);
        _deletingAttachments.Add(attachment.Id.ToString());

        var deleteResult = await AttachSvc.DeleteAsync(attachment.Id);
        if (deleteResult.Success)
        {
            //Request!.Attachments.Remove(editLoading =>);
            toastSvc.ShowSuccess($"{(attachment.AttachType == RequestAttachType.Receipt ? "Receipt" : "Attachment")} deleted successfully");
        }
        else
        {
            toastSvc.ShowError(deleteResult.Message);
        }

        await ConfirmModal.SetLoadingAsync(false);
        await ConfirmModal.HideAsync();
        _deletingAttachments.Remove(attachment.Id.ToString());
        await Refresh();
    }

    private bool CanDeleteAttachment()
    {
        // Only admin/CEO can delete during requisition phase
        return (currentUser.IsAdmin || currentUser.IsCEO) &&
               Request?.Status == RequestStatus.ForRequisition;
    }

    private async Task<string> GetPdfBlobUrl(Request_AttachVM attachment)
    {
        try
        {
            var bytes = await AttachSvc.GetAttachByte(attachment.Id);
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var base64 = Convert.ToBase64String(bytes);
            var blobUrl = await jsRuntime.InvokeAsync<string>("createBlobUrl", base64, "application/pdf");

            if (!string.IsNullOrEmpty(blobUrl))
            {
                _blobUrls.Add(blobUrl);
            }

            return blobUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating blob URL: {ex.Message}");
            return string.Empty;
        }
    }

    private async Task<string> GetImageDataUrl(Request_AttachVM attachment)
    {
        try
        {
            var bytes = await AttachSvc.GetAttachByte(attachment.Id);
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

    private static bool IsImage(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp";
    }

    private static bool IsPdf(string fileName)
    {
        return Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetSlipIcon(RequisitionSlip_For slipFor)
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

    private static string GetSlipDisplayName(RequisitionSlip_For slipFor)
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

    private static string GetDeptDisplayName(RequisitionSlip_Dept dept)
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

    private bool IsSlipPastDue(Request_RS_SlipVM slip)
    {
        if (slip == null)
            return false;

        // Only check if slip is approved
        if (slip.Approval != ApprovalAction.Approve)
            return false;

        // Need approval date to calculate
        if (!slip.SlipApprovalDate.HasValue || slip.NoOfdaysToLiquidate <= 0)
            return false;

        // Calculate due date: approval date + NoOfDaysToLiquidate
        var dueDate = slip.SlipApprovalDate.Value.AddDays(slip.NoOfdaysToLiquidate);

        // Check if current date is past due date
        if (DateTime.Now.Date <= dueDate.Date)
            return false; // Not past due yet

        // Get total receipt amount for this slip
        var totalReceipts = Request.Attachments
            .Where(e => e.AttachType == RequestAttachType.Receipt && e.RequisitionId == slip.Id)
            .Sum(e => e.ReceiptAmount);

        // A slip is past due if:
        // 1. The due date has passed, AND
        // 2. It's not fully liquidated (some amount still pending)
        return slip.AmountRequested > totalReceipts;
    }

    private int GetDaysPastDue(Request_RS_SlipVM slip)
    {
        if (!IsSlipPastDue(slip))
            return 0;

        var dueDate = slip.SlipApprovalDate!.Value.AddDays(slip.NoOfdaysToLiquidate);
        var daysPastDue = (DateTime.Now.Date - dueDate.Date).Days;

        return Math.Max(0, daysPastDue);
    }

    private string GetApprovalText(Request_RS_SlipVM slip)
    {
        if (slip.Approval == ApprovalAction.Approve)
        {
            return slip.SlipApproverId == currentUser.UserId
                ? "Approved by You"
                : $"Approved by {Utils.FormatNameShort(slip.SlipApproverName)}";
        }
        else if (slip.Approval == ApprovalAction.Reject)
        {
            return slip.SlipApproverId == currentUser.UserId
                ? "Rejected by You"
                : $"Rejected by {Utils.FormatNameShort(slip.SlipApproverName)}";
        }
        return "Pending Approval";
    }

    private async Task CloseAttachmentPreview()
    {
        _showPreview = false;
        _selectedAttachment = null;
        _previewUrl = null;
        _isLoadingPreview = false;

        // Clean up any blob URLs
        foreach (var blobUrl in _blobUrls)
        {
            if (!string.IsNullOrEmpty(blobUrl))
            {
                try
                {
                    await jsRuntime.InvokeVoidAsync("revokeBlobUrl", blobUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error revoking blob URL: {ex.Message}");
                }
            }
        }
        _blobUrls.Clear();

        StateHasChanged();
        await Task.Delay(100); // Small delay to ensure clean state
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;

        foreach (var blobUrl in _blobUrls)
        {
            if (!string.IsNullOrEmpty(blobUrl))
            {
                try
                {
                    await jsRuntime.InvokeVoidAsync("revokeBlobUrl", blobUrl);
                }
                catch
                {
                    // swallow — JS runtime may already be gone
                }
            }
        }

        _blobUrls.Clear();
        GC.SuppressFinalize(this);
    }

    private void SafeStateHasChanged()
    {
        if (!_disposed)
            InvokeAsync(StateHasChanged);
    }
}