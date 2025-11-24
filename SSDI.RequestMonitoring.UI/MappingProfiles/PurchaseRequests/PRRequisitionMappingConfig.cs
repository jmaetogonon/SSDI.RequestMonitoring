using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.PurchaseRequests;

public class PRRequisitionMappingConfig : Profile
{
    public PRRequisitionMappingConfig()
    {
        CreateMap<Purchase_Request_Slip, Purchase_Request_SlipVM>().ReverseMap();

        CreateMap<CreatePR_RequisitionCommand, Purchase_Request_SlipVM>().ReverseMap();

        CreateMap<PurchaseRequestRequisitionByIdDto, Purchase_Request_SlipVM>()
            .ForMember(q => q.DateOfRequest, opt => opt.MapFrom(x => x.DateOfRequest!.Value.DateTime))
            .ForMember(q => q.CA_LiquidationDueDate, opt => opt.MapFrom(x => x.CA_LiquidationDueDate!.Value.DateTime))
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated!.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ForMember(q => q.SlipApprovalDate, opt => opt.MapFrom(x => x.SlipApprovalDate!.Value.DateTime));

        CreateMap<EditPR_RequisitionCommand, Purchase_Request_SlipVM>().ReverseMap();
    }
}