using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests;

public class AttachSvc : BaseHttpService, IAttachSvc
{
    private readonly IMapper _mapper;

    public AttachSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<byte[]?> GetAttachByte(int attachmentId)
    {
        try
        {
            var fileResponse = await _client.GetAttachmentByteAsync(attachmentId);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Response<Guid>> UploadAsync(int headerId, bool isPR, IEnumerable<Request_AttachVM> files, Models.Enums.RequestAttachType attachType, int requisitionId = 0, decimal receiptAmount = 0m, string receiptremarks = "", int? poID = null)
    {
        try
        {
            var command = new UploadAttachCommand
            {
                PurchaseRequestId = isPR ? headerId : null,
                JobOrderId = isPR ? null : headerId,
                Files = _mapper.Map<ICollection<Request_Attach>>(files),
                Type = (Base.RequestAttachType)attachType,
                RequisitionId = requisitionId,
                ReceiptAmount = (double)receiptAmount,
                ReceiptRemarks = receiptremarks,
                PoId = poID ?? 0
            };

            await _client.UploadAttachAsync(command);
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

    public async Task<Response<Guid>> DeleteAsync(int id)
    {
        try
        {
            await _client.DeleteAttachAsync(id);
            return new Response<Guid>() { Success = true };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }

    public async Task<byte[]?> DownloadAllReqZipAsync(int requestId, bool isPR)
    {
        try
        {
            var fileResponse = await _client.GetAllReqAttachZipByteAsync(requestId, isPR);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> DownloadAllRSZipAsync(int requestId, bool isPR)
    {
        try
        {
            var fileResponse = await _client.GetAllRSAttachZipByteAsync(requestId, isPR);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> DownloadAllPOZipAsync(int requestId, bool isPR)
    {
        try
        {
            var fileResponse = await _client.GetAllPOAttachZipByteAsync(requestId, isPR);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }
}