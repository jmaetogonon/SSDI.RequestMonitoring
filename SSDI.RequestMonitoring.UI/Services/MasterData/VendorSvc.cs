using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.MasterData;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.MasterData;

public class VendorSvc : BaseHttpService, IVendorSvc
{
    private readonly IMapper _mapper;

    public VendorSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<bool> AddVendorToServer(VendorVM vendor)
    {
        var vendorRequest = _mapper.Map<AddVendorToServerCommand>(vendor);

        try
        {
            await _client.AddVendorToServerAsync(vendorRequest);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<VendorVM>> GetAllVendors()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllVendorsAsync();
            return _mapper.Map<List<VendorVM>>(requests);
        });
    }

    public async Task<bool> SyncVendors()
    {
        try
        {
            await _client.SyncVendorAsync();
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public async Task<bool> IsExistVendor(string code)
    {
        return await _client.IsExistVendorAsync(code);
    }
}