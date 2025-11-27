using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests.Purchase;

public class PRRequisitionSvc : BaseHttpService, IPRRequisitionSvc
{
    private readonly IMapper _mapper;

    public PRRequisitionSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<Response<int>> CreatePRRequisition(Purchase_Request_SlipVM slip)
    {
        try
        {
            var createCommand = _mapper.Map<CreatePR_RequisitionCommand>(slip);
            var newId = await _client.CreatePRRequisitionAsync(createCommand);
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

    public async Task<Response<Guid>> EditPRRequisition(Purchase_Request_SlipVM request)
    {
        try
        {
            var updateRequestCommand = _mapper.Map<EditPR_RequisitionCommand>(request);
            await _client.EditPRRequisitionAsync(updateRequestCommand);
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

    public async Task<Response<Guid>> ApprovePRRequisition(Purchase_Request_SlipVM slip, Models.Enums.ApprovalAction action, int approverId)
    {
        try
        {
            var request = new ApprovePR_RequisitionCommand
            {
                SlipId = slip.Id,
                ApproverId = approverId,
                Action = (Base.ApprovalAction)action
            };

            var updateRequestCommand = _mapper.Map<ApprovePR_RequisitionCommand>(request);
            await _client.ApprovePRRequisitionAsync(updateRequestCommand);
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

    public async Task<Response<Guid>> DeletePRRequisition(int id)
    {
        try
        {
            await _client.DeletePRRequisitionAsync(id);
            return new Response<Guid>() { Success = true };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }

    public async Task<byte[]> GeneratePRRequisitionPdf(int id)
    {
        try
        {
            var requests = await _client.GeneratePdfPRRequisitionAsync(id);
            return requests;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"PDF Generation failed: {ex.Message}");
            return null!;
        }
    }
}
