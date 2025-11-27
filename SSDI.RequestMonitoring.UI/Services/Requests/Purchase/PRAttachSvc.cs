using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Requests.Purchase;

public class PRAttachSvc : BaseHttpService, IPRAttachSvc
{
    private readonly IMapper _mapper;

    public PRAttachSvc(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager, IMapper mapper) : base(client, localStorage, navigationManager)
    {
        _mapper = mapper;
    }

    public async Task<byte[]?> GetAttachByte(int attachmentId)
    {
        try
        {
            var fileResponse = await _client.GetAttachmentAsync(attachmentId);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Response<Guid>> UploadAttachPurchase(UploadAttachmentPurchaseCommandVM command)
    {
        try
        {
            var uploadAttCommand = _mapper.Map<UploadAttachmentPurchaseCommand>(command);
            await _client.UploadAttachmentPurchaseAsync(uploadAttCommand);
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
            await _client.DeleteAttachmentRequestAsync(id);
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
            var fileResponse = await _client.DownloadAllAttachZipAsync(requestId);
            return fileResponse;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }
}
