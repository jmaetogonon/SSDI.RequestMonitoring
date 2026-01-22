using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests;

public class POSlipSvc : BaseHttpService, IPOSlipSvc
{
    private readonly IMapper _mapper;

    public POSlipSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<Response<int>> CreatePOSlip(Request_PO_SlipVM slip)
    {
        try
        {
            var createCommand = _mapper.Map<Create_POSlipCommand>(slip);
            var newId = await _client.CreatePOSlipAsync(createCommand);
            return new Response<int>()
            {
                Data = newId,
                Success = true,
            };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<int>(ex);
        }
    }


    public async Task<Response<Guid>> EditPO(Request_PO_SlipVM request)
    {
        try
        {
            var updateRequestCommand = _mapper.Map<Edit_POSlipCommand>(request);
            await _client.EditPOSlipAsync(updateRequestCommand);
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

    public async Task<Response<Guid>> ApprovePO(Request_PO_SlipVM slip, Models.Enums.ApprovalAction action, int approverId)
    {
        try
        {
            var request = new Approve_POSlipCommand
            {
                SlipId = slip.Id,
                ApproverId = approverId,
                Action = (Base.ApprovalAction)action
            };

            await _client.ApprovePOSlipAsync(request);
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

    public async Task<Response<Guid>> DeletePO(int id)
    {
        try
        {
            await _client.DeletePOSlipAsync(id);
            return new Response<Guid>() { Success = true };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }

    public async Task<byte[]> GeneratePOPdf(int id)
    {
        try
        {
            var requests = await _client.GeneratePdfPOSlipAsync(id);
            return requests;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"PDF Generation failed: {ex.Message}");
            return null!;
        }
    }
}