using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Users;
using SSDI.RequestMonitoring.UI.Models.Users;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Users;

public class UserSvc : BaseHttpService, IUserSvc
{
    private readonly IMapper _mapper;

    public UserSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<List<SupervisorVM>> GetSupervisors()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllSupervisorsAsync();
            return _mapper.Map<List<SupervisorVM>>(requests);
        });
    }

    public async Task<List<UserVM>> GetUsers()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllUsersAsync();
            return _mapper.Map<List<UserVM>>(requests);
        });
    }

    public async Task<bool> SyncUsers()
    {
        try
        {
            await _client.SyncSystemRoleAsync();
            await _client.SyncApplicationAsync();
            await _client.SyncRoleGrpHeaderAsync();
            await _client.SyncRoleGrpDetailAsync();
            await _client.SyncUserAsync();
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public async Task<byte[]?> GetAttachByte(int attachmentId)
    {
        try
        {
            var fileResponse = await _client.GetUserFileByteAsync(attachmentId);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> GetLastSignatureByte(int attachmentId)
    {
        try
        {
            var fileResponse = await _client.GetUserLastSignatureByteAsync(attachmentId);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteAllSignatureAsync(int userId)
    {
        try
        {
            await _client.DeleteAllUserSignaturesAsync(userId);
            return true;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return false;
        }
    }

    public async Task<Response<Guid>> UploadAsync(int userId, Models.Enums.RequestAttachType type, List<UserFileVM> files)
    {
        try
        {
            var command = new UploadUserFileCommand()
            {
                UserId = userId,
                Type = (Base.RequestAttachType)type,
                Files = _mapper.Map<List<UserFile>>(files)
            };
            await _client.UploadUserFileAsync(command);
            return new Response<Guid>()
            {
                Success = true,
            };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }
}