using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests;

public class RSSlipSvc : BaseHttpService, IRSSlipSvc
{
    private readonly IMapper _mapper;

    public RSSlipSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<Response<int>> CreateRequisition(Request_RS_SlipVM slip)
    {
        try
        {
            var createCommand = _mapper.Map<Create_RSSlipCommand>(slip);
            var newId = await _client.CreateRSSlipAsync(createCommand);
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

    public async Task<Response<Guid>> EditRequisition(Request_RS_SlipVM request)
    {
        try
        {
            var updateRequestCommand = _mapper.Map<Edit_RSSlipCommand>(request);
            await _client.EditRSSlipAsync(updateRequestCommand);
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

    public async Task<Response<Guid>> ApproveRequisition(Request_RS_SlipVM slip, Models.Enums.ApprovalAction action, int approverId)
    {
        try
        {
            var request = new Approve_RSSlipCommand
            {
                SlipId = slip.Id,
                ApproverId = approverId,
                Action = (Base.ApprovalAction)action
            };

            await _client.ApproveRSSlipAsync(request);
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

    public async Task<Response<Guid>> DeleteRequisition(int id)
    {
        try
        {
            await _client.DeleteRSSlipAsync(id);
            return new Response<Guid>() { Success = true };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }

    public async Task<byte[]> GenerateRequisitionPdf(int id)
    {
        try
        {
            var requests = await _client.GeneratePdfRSSlipAsync(id);
            return requests;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"PDF Generation failed: {ex.Message}");
            return null!;
        }
    }
}
