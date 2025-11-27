using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.PurchaseRequests;

public class PRAttachMappingConfig : Profile
{
    public PRAttachMappingConfig()
    {
        CreateMap<PurchaseRequestAttachByIdDto, Purchase_Request_AttachVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated!.Value.DateTime));

        CreateMap<Purchase_Request_Attach, Purchase_Request_AttachVM>().ReverseMap();

        CreateMap<UploadAttachmentPurchaseCommand, UploadAttachmentPurchaseCommandVM>().ReverseMap();

    }
}