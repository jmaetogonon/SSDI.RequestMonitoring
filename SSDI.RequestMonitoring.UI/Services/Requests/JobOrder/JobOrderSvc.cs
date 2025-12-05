using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests.JobOrder;

public class JobOrderSvc : BaseHttpService, IJobOrderSvc
{
    private readonly IMapper _mapper;

    public JobOrderSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<Response<Guid>> CreateJobOrder(Job_OrderVM JobOrder)
    {
        try
        {
            var createRequestCommand = _mapper.Map<CreateJobOrderCommand>(JobOrder);
            await _client.CreateJobOrderAsync(createRequestCommand);
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

    public async Task<Response<Guid>> DeleteJobOrder(int id)
    {
        try
        {
            await _client.DeleteJobOrderAsync(id);
            return new Response<Guid>() { Success = true };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }

    public async Task<List<Job_OrderVM>> GetAllJobOrders()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllJobOrderAsync();
            return _mapper.Map<List<Job_OrderVM>>(requests);
        });
    }

    public async Task<List<Job_OrderVM>> GetAllJobOrdersByUser(int userId)
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllJobOrderByUserAsync(userId);
            return _mapper.Map<List<Job_OrderVM>>(requests);
        });
    }

    public async Task<List<Job_OrderVM>> GetAllJobOrdersByAdmin()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllJobOrderByAdminAsync();
            return _mapper.Map<List<Job_OrderVM>>(requests);
        });
    }

    public async Task<List<Job_OrderVM>> GetAllJobOrderByCeo()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllJobOrderByCeoAsync();
            return _mapper.Map<List<Job_OrderVM>>(requests);
        });
    }

    public async Task<List<Job_OrderVM>> GetAllJobOrderBySupervisor(int supervisorId, bool includeDepartmentMembers = true, bool includeDivisionMembers = true)
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllJobOrderBySupervisorsAsync(supervisorId, includeDepartmentMembers, includeDivisionMembers);
            return _mapper.Map<List<Job_OrderVM>>(requests);
        });
    }

    public async Task<Job_OrderVM> GetByIdJobOrder(int id)
    {
        var request = await _client.GetByIdJobOrderAsync(id);
        return _mapper.Map<Job_OrderVM>(request);
    }

    public async Task<Response<Guid>> UpdateJobOrder(int id, Job_OrderVM JobOrder)
    {
        try
        {
            var updateRequestCommand = _mapper.Map<UpdateJobOrderCommand>(JobOrder);
            await _client.UpdateJobOrderAsync(updateRequestCommand);
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

    public async Task<Response<Guid>> ApproveJobOrder(ApproveJobOrderCommandVM command)
    {
        try
        {
            var approveCommand = _mapper.Map<ApproveJobOrderCommand>(command);
            await _client.ApproveJobOrderAsync(approveCommand);

            return new Response<Guid>
            {
                Success = true,
                Message = command.Action == Models.Enums.ApprovalAction.Approve
                    ? "Purchase request approved successfully."
                    : "Purchase request rejected successfully."
            };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }

    public async Task<byte[]> GenerateJobOrderPdf(int id)
    {
        try
        {
            var requests = await _client.GeneratePdfJobOrderAsync(id);
            return requests;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"PDF Generation failed: {ex.Message}");
            return null!;
        }
    }

    public async Task<Response<Guid>> InitiateCloseJobOrder(int id, int initiatorId)
    {
        try
        {
            InitiateCloseJobOrderCommand command = new()
            {
                RequestId = id,
                InitiatorUserId = initiatorId
            };

            await _client.InitiateCloseJobOrderAsync(command);
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

    public async Task<Response<Guid>> ConfirmCloseJobOrder(int id, int userId)
    {
        try
        {
            ConfirmCloseJobOrderCommand command = new()
            {
                RequestId = id,
                UserId = userId
            };

            await _client.ConfirmCloseJobOrderAsync(command);
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

    public async Task<Response<Guid>> CancelPendingCloseJobOrder(int id, int userId)
    {
        try
        {
            CancelPendingCloseJobOrderCommand command = new()
            {
                RequestId = id,
                UserId = userId
            };
            await _client.CancelPendingCloseJobOrderAsync(command);

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