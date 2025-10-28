using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests;

public class PurchaseRequestSvc : BaseHttpService, IPurchaseRequestSvc
{
    private readonly IMapper _mapper;

    public PurchaseRequestSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<Response<Guid>> CreatePurchaseRequest(Purchase_RequestVM PurchaseRequest)
    {
        try
        {
            var createRequestCommand = _mapper.Map<CreatePurchaseRequestCommand>(PurchaseRequest);
            await _client.CreatePurchaseRequestAsync(createRequestCommand);
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

    public Task<Response<Guid>> DeletePurchaseRequest(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Purchase_RequestVM>> GetAllPurchaseRequests()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllPurchaseRequestAsync();
            return _mapper.Map<List<Purchase_RequestVM>>(requests);
        });
    }

    public async Task<Purchase_RequestVM> GetByIdPurchaseRequest(int id)
    {
        var request = await _client.GetByIdPurchaseRequestAsync(id);
        return _mapper.Map<Purchase_RequestVM>(request);
    }

    public Task<Response<Guid>> UpdatePurchaseRequest(int id, Purchase_RequestVM PurchaseRequest)
    {
        throw new NotImplementedException();
    }
}