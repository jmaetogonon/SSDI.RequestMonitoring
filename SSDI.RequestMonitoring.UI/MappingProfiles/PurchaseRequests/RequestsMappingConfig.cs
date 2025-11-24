using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.PurchaseRequests;

public class RequestsMappingConfig : Profile
{
    public RequestsMappingConfig()
    {
        CreateMap<PurchaseRequestDto, Purchase_RequestVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<PurchaseReqByUserDto, Purchase_RequestVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<PurchaseRequestByIdDto, Purchase_RequestVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<PurchaseReqBySupervisorDto, Purchase_RequestVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<PurchaseReqByAdminDto, Purchase_RequestVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<PurchaseReqByCEODto, Purchase_RequestVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<PurchaseRequestApprovalByIdDto, Purchase_Request_ApprovalVM>()
            .ForMember(q => q.ActionDate, opt => opt.MapFrom(x => x.ActionDate!.Value.DateTime));

        CreateMap<CreatePurchaseRequestCommand, Purchase_RequestVM>().ReverseMap();

        CreateMap<UpdatePurchaseRequestCommand, Purchase_RequestVM>().ReverseMap();

        CreateMap<ApprovePurchaseRequestCommand, ApprovePurchaseRequestCommandVM>().ReverseMap();
    }
}