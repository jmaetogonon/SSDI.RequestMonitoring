using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.Contracts.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;
using RequestAttachType = SSDI.RequestMonitoring.UI.Models.Enums.RequestAttachType;

namespace SSDI.RequestMonitoring.UI.Services.Adapters.Requests;

public class JOAttachSvcAdapter : IAttachmentSvc
{
    private readonly IJOAttachSvc _svc;

    public JOAttachSvcAdapter(IJOAttachSvc svc) => _svc = svc;

    public Task<byte[]?> GetBytesAsync(int attachmentId) => _svc.GetAttachByte(attachmentId);

    public Task<byte[]?> DownloadAllZipAsync(int requestId, RequestAttachType attachType) => _svc.DownloadAllAttachZip(requestId);

    public Task<byte[]?> DownloadAllSlipZipAsync(int requestId) => _svc.DownloadAllSlipAttachZip(requestId);

    public async Task<Response<Guid>> UploadAsync(IRequestDetailVM request, IEnumerable<IAttachmentVM> files, RequestAttachType attachType, int? requisitionId = null, decimal? receiptAmount = null, string receiptremarks = "")
    {
        if (request is not Job_OrderVM pr) return new Response<Guid>() { Success = false };

        // Map UI IAttachmentVM to UploadAttachmentPurchaseCommandVM expected by service
        var command = new UploadAttachmentJobOrderCommandVM
        {
            JobOrderId = pr.Id,
            Type = attachType,
            RequisitionId = requisitionId ?? 0,
            ReceiptAmount = receiptAmount ?? 0m,
            ReceiptRemarks = receiptremarks,
            Files = files.Select(f => new Job_Order_AttachVM
            {
                Id = f.Id,
                UniqId = f.UniqId,
                JobOrderId = request.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                ImgData = f.ImgData,
                Size = f.Size,
                AttachType = f.AttachType,
                RequisitionId = requisitionId ?? 0,
                ReceiptAmount = receiptAmount ?? 0m,
                ReceiptRemarks = receiptremarks,
            }).ToList()
        };

        return await _svc.UploadAttachPurchase(command);
    }

    public Task<Response<Guid>> DeleteAsync(int id)
        => _svc.DeleteAttachRequest(id);
}