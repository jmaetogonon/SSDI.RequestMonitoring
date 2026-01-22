using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles;

public class RSSlipMappingConfig : Profile
{
    public RSSlipMappingConfig()
    {
        CreateMap<PurchaseRequestRequisitionByIdDto, Request_RS_SlipVM>()
            .ForMember(q => q.DateOfRequest, opt => opt.MapFrom(x => x.DateOfRequest!.Value.DateTime))
            .ForMember(q => q.CA_LiquidationDueDate, opt => opt.MapFrom(x => x.CA_LiquidationDueDate!.Value.DateTime))
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated!.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ForMember(q => q.SlipApprovalDate, opt => opt.MapFrom(x => x.SlipApprovalDate!.Value.DateTime));

        CreateMap<Create_RSSlipCommand, Request_RS_SlipVM>().ReverseMap();
        CreateMap<Edit_RSSlipCommand, Request_RS_SlipVM>().ReverseMap();

        //===

        CreateMap<JobOrderRequisitionByIdDto, Request_RS_SlipVM>()
            .ForMember(q => q.DateOfRequest, opt => opt.MapFrom(x => x.DateOfRequest!.Value.DateTime))
            .ForMember(q => q.CA_LiquidationDueDate, opt => opt.MapFrom(x => x.CA_LiquidationDueDate!.Value.DateTime))
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated!.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ForMember(q => q.SlipApprovalDate, opt => opt.MapFrom(x => x.SlipApprovalDate!.Value.DateTime));
    }
}