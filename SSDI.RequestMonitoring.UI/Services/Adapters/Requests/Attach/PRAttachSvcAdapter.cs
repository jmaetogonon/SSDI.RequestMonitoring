using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Base;
using RequestAttachType = SSDI.RequestMonitoring.UI.Models.Enums.RequestAttachType;

namespace SSDI.RequestMonitoring.UI.Services.Adapters.Requests;

public class PRAttachSvcAdapter : IAttachmentSvc
{
    private readonly IPRAttachSvc _svc;

    public PRAttachSvcAdapter(IPRAttachSvc svc) => _svc = svc;

    public Task<byte[]?> GetBytesAsync(int attachmentId) => _svc.GetAttachByte(attachmentId);

    public Task<byte[]?> DownloadAllZipAsync(int requestId, RequestAttachType attachType) => _svc.DownloadAllAttachZip(requestId);

    public Task<byte[]?> DownloadAllSlipZipAsync(int requestId) => _svc.DownloadAllSlipAttachZip(requestId);

    public async Task<Response<Guid>> UploadAsync(IRequestDetailVM request, IEnumerable<IAttachmentVM> files, RequestAttachType attachType, int? requisitionId = null, decimal? receiptAmount = null, string receiptremarks = "")
    {
        if (request is not Purchase_RequestVM pr) return new Response<Guid>() { Success = false };

        // Map UI IAttachmentVM to UploadAttachmentPurchaseCommandVM expected by service
        var command = new UploadAttachmentPurchaseCommandVM
        {
            PurchaseRequestId = pr.Id,
            Type = attachType,
            RequisitionId = requisitionId ?? 0,
            ReceiptAmount = receiptAmount ?? 0m,
            ReceiptRemarks = receiptremarks,
            Files = files.Select(f => new Purchase_Request_AttachVM
            {
                Id = f.Id,
                UniqId = f.UniqId,
                PurchaseRequestId = pr.Id,
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