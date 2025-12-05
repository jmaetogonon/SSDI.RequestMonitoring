using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests.JobOrder;

public class JOAttachSvc : BaseHttpService, IJOAttachSvc
{
    private readonly IMapper _mapper;

    public JOAttachSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<byte[]?> GetAttachByte(int attachmentId)
    {
        try
        {
            var fileResponse = await _client.GetAttachmentJobOrderAsync(attachmentId);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Response<Guid>> UploadAttachPurchase(UploadAttachmentJobOrderCommandVM command)
    {
        try
        {
            var uploadAttCommand = _mapper.Map<UploadAttachmentJobOrderCommand>(command);
            await _client.UploadAttachmentJobOrderAsync(uploadAttCommand);
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

    public async Task<Response<Guid>> DeleteAttachRequest(int id)
    {
        try
        {
            await _client.DeleteAttachmentJobOrderAsync(id);
            return new Response<Guid>() { Success = true };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<Guid>(ex);
        }
    }

    public async Task<byte[]?> DownloadAllAttachZip(int requestId)
    {
        try
        {
            var fileResponse = await _client.DownloadAllAttachZipJobOrderAsync(requestId);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> DownloadAllSlipAttachZip(int requestId)
    {
        try
        {
            var fileResponse = await _client.DownloadAllSlipAttachJobOrderAsync(requestId);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }
}