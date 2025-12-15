using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.MasterData;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.MasterData;

public class SystemConfigSvc : BaseHttpService, ISystemConfigSvc
{
    private readonly IMapper _mapper;

    public SystemConfigSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<SystemConfigVM> GetSystemConfigAsync()
    {
        return await SafeApiCall(async () =>
        {
            var config = await _client.GetSystemConfigAsync();
            return _mapper.Map<SystemConfigVM>(config);
        });
    }
}