using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls.Requests;

public partial class RequestAttachments__Control : ComponentBase
{
    [Parameter] public IRequestDetailVM Request { get; set; } = default!;
    [Parameter] public IAttachSvc AttachSvc { get; set; } = default!;
    [Parameter] public EventCallback OnRequestChanged { get; set; }

    [Parameter] public Confirmation__Modal ConfirmModal { get; set; } = new();
    [Parameter] public string ReportType { get; set; } = string.Empty;
    [Parameter] public RequestAttachType AttachType { get; set; }
    [Parameter] public RequestType RequestType { get; set; }

    private bool IsPR => RequestType is RequestType.Purchase;
    private Request_AttachVM? _selectedAttachment;
    private bool _showPreview = false;
    private string? _previewUrl;
    private readonly List<string> _blobUrls = [];

    // Loading states
    private bool _isLoadingPreview = false;

    private string? _loadingPreviewId = null;
    private bool _isDownloadingAll = false;
    private int _currentDownloadIndex = 0;
    private readonly HashSet<string> _downloadingAttachments = [];

    private async Task Refresh()
    { if (OnRequestChanged.HasDelegate) await OnRequestChanged.InvokeAsync(); }

    private async Task OnFilesSelected(InputFileChangeEventArgs e)
    {
        List<Request_AttachVM> temps = [];
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

            var attachVM = new Request_AttachVM
            {
                FileName = file.Name,
                ContentType = file.ContentType,
                ImgData = fileBytes,
                Size = file.Size,
                UniqId = Utils.GenerateUniqId(),
                AttachType = AttachType
            };

            temps.Add(attachVM);
        }

        var res = await AttachSvc.UploadAsync(Request.Id, IsPR, temps, AttachType);
        if (!res.Success)
        {
            toastSvc.ShowError("Error uploading attachments. Please try again.");
        }

        await Refresh();
    }

    private async Task ViewAttachment(Request_AttachVM attachment)
    {
        _selectedAttachment = attachment;
        _loadingPreviewId = attachment.Id.ToString();
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
            _loadingPreviewId = null;
            StateHasChanged();
        }
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

    private async Task CloseAttachmentPreview()
    {
        _showPreview = false;
        //selectedAttachment = new();
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

        await AttachSvc.DeleteAsync(attachment.Id);
        toastSvc.ShowSuccess("Attachment deleted successfully.");
        await Refresh();
    }

    private async Task DownloadAll()
    {
        if (_isDownloadingAll || Request.Attachments == null)
            return;

        _isDownloadingAll = true;
        _currentDownloadIndex = 0;
        StateHasChanged();

        try
        {
            var fileBytes = await AttachSvc.DownloadAllReqZipAsync(Request.Id, IsPR);
            var fileName = $"{(IsPR ? "PR" : "JO")}{Request.SeriesNumber} Attachments.zip";

            if (fileBytes != null && fileBytes.Length > 0)
            {
                await jsRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading all attachments: {ex.Message}");
        }
        finally
        {
            _isDownloadingAll = false;
            _currentDownloadIndex = 0;
            StateHasChanged();
        }
    }

    private static string GetFileIcon(string fileName)
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

    private static bool IsImage(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp";
    }

    private static bool IsPdf(string fileName)
    {
        return Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public async ValueTask DisposeAsync()
    {
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
        GC.SuppressFinalize(this);
    }
}