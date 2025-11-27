using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls.JobOrder;

public partial class JobOrderAttachments__Control : ComponentBase
{
    [Parameter] public Job_OrderVM Request { get; set; } = new();
    [Parameter] public Confirmation__Modal ConfirmModal { get; set; } = new();
    [Parameter] public string ReportType { get; set; } = string.Empty;

    private Job_Order_AttachVM? selectedAttachment;
    private bool showPreview = false;
    private string? previewUrl;
    private List<string> blobUrls = [];

    // Loading states
    private bool isLoadingPreview = false;

    private string? loadingPreviewId = null;
    private bool isDownloadingAll = false;
    private int currentDownloadIndex = 0;
    private HashSet<string> downloadingAttachments = [];

    private async Task OnFilesSelected(InputFileChangeEventArgs e)
    {
        var options = new ConfirmationModalOptions
        {
            Message = "Are you sure you want to upload the selected files/?",
            Title = "Upload Attachments",
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
                PurchaseRequestId = Request.Id,
                FileName = file.Name,
                ContentType = file.ContentType,
                ImgData = fileBytes,
                Size = file.Size,
                AttachType = RequestAttachType.Request
            };

            Request.Attachments.Add(attachVM);
        }

        var command = new UploadAttachmentJobOrderCommandVM
        {
            PurchaseRequestId = Request.Id,
            Files = Request.Attachments,
            Type = RequestAttachType.Request
        };

        var res = await attachSvc.UploadAttachPurchase(command);
        if (!res.Success)
        {
            toastSvc.ShowError("Error uploading attachments. Please try again.");
        }

        Request = await jobOrderSvc.GetByIdJobOrder(Request.Id);
        StateHasChanged();
    }

    private async Task ViewAttachment(Job_Order_AttachVM attachment)
    {
        selectedAttachment = attachment;
        loadingPreviewId = attachment.Id.ToString();
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
            loadingPreviewId = null;
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

    private async Task DeleteAttachment(Job_Order_AttachVM attachment, MouseEventArgs e)
    {
        var options = new ConfirmationModalOptions
        {
            Message = $"Are you sure you want to delete the attachment '{attachment.FileName}'?",
            Title = "Delete Attachment",
            Variant = ConfirmationModalVariant.confirmation,
            ConfirmText = "Yes, Delete",
            CancelText = "No, Cancel",
        };

        var result = await ConfirmModal!.ShowAsync(options);
        if (!result) return;

        await attachSvc.DeleteAttachRequest(attachment.Id);
        Request.Attachments.Remove(attachment);
        toastSvc.ShowSuccess("Attachment deleted successfully.");
    }

    private async Task DownloadAll()
    {
        if (isDownloadingAll || Request.Attachments == null)
            return;

        isDownloadingAll = true;
        currentDownloadIndex = 0;
        StateHasChanged();

        try
        {
            var fileBytes = await attachSvc.DownloadAllAttachZip(Request.Id);
            var fileName = $"PR#{Request.Id}_{Request.Name}_Attachments.zip";

            if (fileBytes != null && fileBytes.Length > 0)
            {
                await JS.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
            }
            //foreach (var attachment in Request.Attachments)
            //{
            //    currentDownloadIndex++;
            //    StateHasChanged();

            //    await DownloadAttachment(attachment);
            //    await Task.Delay(200); // Small delay between downloads
            //}
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading all attachments: {ex.Message}");
        }
        finally
        {
            isDownloadingAll = false;
            currentDownloadIndex = 0;
            StateHasChanged();
        }
    }

    private string GetFileIcon(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".pdf" => "bi bi-file-pdf",
            ".doc" or ".docx" => "bi bi-file-word",
            ".xls" or ".xlsx" => "bi bi-file-excel",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "bi bi-file-image",
            ".zip" or ".rar" => "bi bi-file-zip",
            ".txt" => "bi bi-file-text",
            _ => "bi bi-file-earmark"
        };
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

    private bool IsImage(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp";
    }

    private bool IsPdf(string fileName)
    {
        return Path.GetExtension(fileName).ToLower() == ".pdf";
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