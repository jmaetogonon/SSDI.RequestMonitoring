using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.MasterData;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.MasterData;

public class DivisionSvc : BaseHttpService, IDivisionSvc
{
    private readonly IMapper _mapper;

    public DivisionSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<List<DivisionVM>> GetAllDivisions()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllDivisionsAsync();
            return _mapper.Map<List<DivisionVM>>(requests);
        });
    }

    public async Task<Response<Guid>> BulkUpsertDivisions(List<DivisionVM> divisionList)
    {
        try
        {
            var comm = _mapper.Map<List<DivisionDto>>(divisionList);
            await _client.UpsertDivisionsAsync(comm);
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