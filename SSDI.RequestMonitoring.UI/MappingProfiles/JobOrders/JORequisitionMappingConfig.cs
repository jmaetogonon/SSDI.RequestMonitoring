using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.JobOrders;

public class JORequisitionMappingConfig : Profile
{
    public JORequisitionMappingConfig()
    {
        CreateMap<Job_Order_Slip, Job_Order_SlipVM>().ReverseMap();

        CreateMap<CreateJO_RequisitionCommand, Job_Order_SlipVM>().ReverseMap();

        CreateMap<JobOrderRequisitionByIdDto, Job_Order_SlipVM>()
            .ForMember(q => q.DateOfRequest, opt => opt.MapFrom(x => x.DateOfRequest!.Value.DateTime))
            .ForMember(q => q.CA_LiquidationDueDate, opt => opt.MapFrom(x => x.CA_LiquidationDueDate!.Value.DateTime))
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated!.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ForMember(q => q.SlipApprovalDate, opt => opt.MapFrom(x => x.SlipApprovalDate!.Value.DateTime));

        CreateMap<EditJO_RequisitionCommand, Job_Order_SlipVM>().ReverseMap();
    }
}