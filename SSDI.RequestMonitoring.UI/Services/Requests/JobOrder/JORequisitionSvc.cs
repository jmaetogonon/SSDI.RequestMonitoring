using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests.JobOrder;

public class JORequisitionSvc : BaseHttpService, IJORequisitionSvc
{
    private readonly IMapper _mapper;

    public JORequisitionSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<Response<int>> CreateJORequisition(Job_Order_SlipVM slip)
    {
        try
        {
            var createCommand = _mapper.Map<CreateJO_RequisitionCommand>(slip);
            var newId = await _client.CreateJORequisitionAsync(createCommand);
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

    public async Task<Response<Guid>> EditJORequisition(Job_Order_SlipVM request)
    {
        try
        {
            var updateRequestCommand = _mapper.Map<EditJO_RequisitionCommand>(request);
            await _client.EditJORequisitionAsync(updateRequestCommand);
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

    public async Task<Response<Guid>> ApproveJORequisition(Job_Order_SlipVM slip, Models.Enums.ApprovalAction action, int approverId)
    {
        try
        {
            var request = new ApproveJO_RequisitionCommand
            {
                SlipId = slip.Id,
                ApproverId = approverId,
                Action = (Base.ApprovalAction)action
            };

            var updateRequestCommand = _mapper.Map<ApproveJO_RequisitionCommand>(request);
            await _client.ApproveJORequisitionAsync(updateRequestCommand);
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

    public async Task<Response<Guid>> DeleteJORequisition(int id)
    {
        try
        {
            await _client.DeleteJORequisitionAsync(id);
            return new Response<Guid>() { Success = true };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }

    public async Task<byte[]> GenerateJORequisitionPdf(int id)
    {
        try
        {
            var requests = await _client.GeneratePdfJORequisitionAsync(id);
            return requests;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"PDF Generation failed: {ex.Message}");
            return null!;
        }
    }
}