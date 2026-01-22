using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.Common;

public interface IAttachSvc
{
    Task<byte[]?> GetAttachByte(int attachmentId);

    Task<byte[]?> DownloadAllReqZipAsync(int requestId, bool isPR);

    Task<byte[]?> DownloadAllRSZipAsync(int requestId, bool isPR);

    Task<byte[]?> DownloadAllPOZipAsync(int requestId, bool isPR);

    Task<Response<Guid>> UploadAsync(int headerId, bool isPR, IEnumerable<Request_AttachVM> files, Models.Enums.RequestAttachType attachType, int requisitionId = 0, decimal receiptAmount = 0m, string receiptremarks = "", int? poID = null);

    Task<Response<Guid>> DeleteAsync(int attachmentId);
}