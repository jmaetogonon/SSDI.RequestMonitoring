using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.MasterData;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.MasterData;

public class DepartmentSvc : BaseHttpService, IDepartmentSvc
{
    private readonly IMapper _mapper;

    public DepartmentSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<List<DepartmentVM>> GetAllDepartments()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllDepartmentsAsync();
            return _mapper.Map<List<DepartmentVM>>(requests);
        });
    }
}