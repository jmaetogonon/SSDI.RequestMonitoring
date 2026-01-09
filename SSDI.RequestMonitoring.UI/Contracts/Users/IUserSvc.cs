using SSDI.RequestMonitoring.UI.Models.Users;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Contracts.Users;

public interface IUserSvc
{
    Task<List<SupervisorVM>> GetSupervisors();

    Task<bool> SyncUsers();

    Task<byte[]?> GetAttachByte(int attachmentId);

    Task<byte[]?> GetLastSignatureByte(int attachmentId);

    Task<bool> DeleteAllSignatureAsync(int userId);

    Task<Response<Guid>> UploadAsync(int userId, Models.Enums.RequestAttachType type, List<UserFileVM> files);
}