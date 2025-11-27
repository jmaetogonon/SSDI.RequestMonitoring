using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.Purchase;

public interface IPRAttachSvc
{
    Task<byte[]?> GetAttachByte(int attachmentId);
    Task<byte[]?> DownloadAllAttachZip(int requestId);

    Task<Response<Guid>> UploadAttachPurchase(UploadAttachmentPurchaseCommandVM command);

    Task<Response<Guid>> DeleteAttachRequest(int id);
}
