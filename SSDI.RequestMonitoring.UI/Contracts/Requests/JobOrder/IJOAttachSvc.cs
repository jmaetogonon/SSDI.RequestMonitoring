using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.JobOrder;

public interface IJOAttachSvc
{
    Task<byte[]?> GetAttachByte(int attachmentId);
    Task<byte[]?> DownloadAllAttachZip(int requestId);
    Task<byte[]?> DownloadAllSlipAttachZip(int requestId);

    Task<Response<Guid>> UploadAttachPurchase(UploadAttachmentJobOrderCommandVM command);

    Task<Response<Guid>> DeleteAttachRequest(int id);
}