using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests.Purchase;

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

    public async Task<Response<Guid>> DeletePurchaseRequest(int id)
    {
        try
        {
            await _client.DeletePurchaseRequestAsync(id);
            return new Response<Guid>() { Success = true };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }

    public async Task<List<Purchase_RequestVM>> GetAllPurchaseRequests()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllPurchaseRequestAsync();
            return _mapper.Map<List<Purchase_RequestVM>>(requests);
        });
    }

    public async Task<List<Purchase_RequestVM>> GetAllPurchaseRequestsByUser(int userId)
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllPurchaseReqByUserAsync(userId);
            return _mapper.Map<List<Purchase_RequestVM>>(requests);
        });
    }

    public async Task<List<Purchase_RequestVM>> GetAllPurchaseRequestsByAdmin()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllPurchaseReqByAdminAsync();
            return _mapper.Map<List<Purchase_RequestVM>>(requests);
        });
    }

    public async Task<List<Purchase_RequestVM>> GetAllPurchaseReqByCeo()
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllPurchaseReqByCeoAsync();
            return _mapper.Map<List<Purchase_RequestVM>>(requests);
        });
    }

    public async Task<List<Purchase_RequestVM>> GetAllPurchaseReqBySupervisor(int supervisorId, bool includeDepartmentMembers = true, bool includeDivisionMembers = true)
    {
        return await SafeApiCall(async () =>
        {
            var requests = await _client.GetAllPurchaseReqBySupervisorsAsync(supervisorId, includeDepartmentMembers, includeDivisionMembers);
            return _mapper.Map<List<Purchase_RequestVM>>(requests);
        });
    }

    public async Task<Purchase_RequestVM> GetByIdPurchaseRequest(int id)
    {
        var request = await _client.GetByIdPurchaseRequestAsync(id);
        return _mapper.Map<Purchase_RequestVM>(request);
    }

    public async Task<Response<Guid>> UpdatePurchaseRequest(int id, Purchase_RequestVM PurchaseRequest)
    {
        try
        {
            var updateRequestCommand = _mapper.Map<UpdatePurchaseRequestCommand>(PurchaseRequest);
            await _client.UpdatePurchaseRequestAsync(updateRequestCommand);
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

    public async Task<Response<Guid>> ApprovePurchaseRequest(ApprovePurchaseRequestCommandVM command)
    {
        try
        {
            var approveCommand = _mapper.Map<ApprovePurchaseRequestCommand>(command);
            await _client.ApprovePurchaseRequestAsync(approveCommand);

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

    public async Task<byte[]> GeneratePurchaseRequestPdf(int id, bool isWithAttach)
    {
        try
        {
            var requests = await _client.GeneratePdfAsync(id, isWithAttach);
            return requests;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"PDF Generation failed: {ex.Message}");
            return null!;
        }
    }

    public async Task<Response<Guid>> InitiateClosePurchaseRequest(int id, int initiatorId)
    {
        try
        {
            InitiateClosePurchaseRequestCommand command = new()
            {
                RequestId = id,
                InitiatorUserId = initiatorId
            };

            await _client.InitiateClosePurchaseRequestAsync(command);
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

    public async Task<Response<Guid>> ConfirmClosePurchaseRequest(int id, int userId)
    {
        try
        {
            ConfirmClosePurchaseRequestCommand command = new()
            {
                RequestId = id,
                UserId = userId
            };

            await _client.ConfirmClosePurchaseRequestAsync(command);
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

    public async Task<Response<Guid>> CancelPendingClosePurchaseRequest(int id, int userId)
    {
        try
        {
            CancelPendingClosePurchaseRequestCommand command = new()
            {
                RequestId = id,
                UserId = userId
            };
            await _client.CancelPendingClosePurchaseRequestAsync(command);

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