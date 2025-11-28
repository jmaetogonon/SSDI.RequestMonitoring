using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Services.Base;
using RequestAttachType = SSDI.RequestMonitoring.UI.Models.Enums.RequestAttachType;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.Common;

public interface IAttachmentSvc
{
    Task<byte[]?> GetBytesAsync(int attachmentId);
    Task<byte[]?> DownloadAllZipAsync(int requestId, RequestAttachType attachType);
    Task<Response<Guid>> UploadAsync(IRequestDetailVM request, IEnumerable<IAttachmentVM> files, RequestAttachType attachType, int? requisitionId = null, decimal? receiptAmount = null);
    Task<Response<Guid>> DeleteAsync(int attachmentId);
}
