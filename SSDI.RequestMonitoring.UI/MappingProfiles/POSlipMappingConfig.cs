using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles;

public class POSlipMappingConfig : Profile
{
    public POSlipMappingConfig()
    {

        CreateMap<Request_PO_Slip_Detail, Request_PO_Slip_DetailVM>()
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap()
            .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.DateModified, opt => opt.MapFrom(src => DateTime.Now));

        CreateMap<PurchaseRequestPOByIdDto, Request_PO_SlipVM>()
            .ForMember(q => q.Date_Issued, opt => opt.MapFrom(x => x.Date_Issued.DateTime))
            .ForMember(q => q.SlipApprovalDate, opt => opt.MapFrom(x => x.SlipApprovalDate!.Value.DateTime))
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime));

        CreateMap<PurchaseRequestPODetailsByIdDto, Request_PO_Slip_DetailVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap()
            .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTimeOffset.Now))
            .ForMember(dest => dest.DateModified, opt => opt.MapFrom(src => DateTimeOffset.Now));

        CreateMap<Create_POSlipCommand, Request_PO_SlipVM>().ReverseMap();
        CreateMap<Edit_POSlipCommand, Request_PO_SlipVM>().ReverseMap();

        //===

        CreateMap<JobOrderPOByIdDto, Request_PO_SlipVM>()
            .ForMember(q => q.Date_Issued, opt => opt.MapFrom(x => x.Date_Issued.DateTime))
            .ForMember(q => q.SlipApprovalDate, opt => opt.MapFrom(x => x.SlipApprovalDate!.Value.DateTime))
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime));

        CreateMap<JobOrderPODetailsByIdDto, Request_PO_Slip_DetailVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();
    }
}
