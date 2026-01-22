using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles;

public class AttachMappingConfig : Profile
{
    public AttachMappingConfig()
    {
        CreateMap<JobOrderAttachByIdDto, Request_AttachVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated!.Value.DateTime));

        CreateMap<PurchaseRequestAttachByIdDto, Request_AttachVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated!.Value.DateTime));

        CreateMap<Request_Attach, Request_AttachVM>().ReverseMap();
    }
}