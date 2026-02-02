using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.MasterData;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.MasterData;

public class BusinessUnitSvc : BaseHttpService, IBusinessUnitSvc
{
    private readonly IMapper _mapper;

    public BusinessUnitSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<List<BusinessUnitVM>> GetAllBusinessUnits()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllBusinessUnitsAsync();
            return _mapper.Map<List<BusinessUnitVM>>(requests).Where(e => e.BU_Code != "NONE" && e.BU_Code != "ALL").ToList();
        });
    }

    public async Task<bool> SyncBusinessUnits()
    {
        try
        {
            await _client.SyncBusinessUnitAsync();
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }
}