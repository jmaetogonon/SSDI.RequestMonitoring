using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles;

public class RequestsMappingConfig : Profile
{
    public RequestsMappingConfig()
    {
        CreateMap<PurchaseRequestDto, Purchase_RequestVM>()
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateAdminNotified, opt => opt.MapFrom(x => x.DateAdminNotified!.Value.DateTime))
            .ReverseMap();
        CreateMap<CreatePurchaseRequestCommand, Purchase_RequestVM>().ReverseMap();
    }
}
