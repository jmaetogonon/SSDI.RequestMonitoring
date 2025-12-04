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
}