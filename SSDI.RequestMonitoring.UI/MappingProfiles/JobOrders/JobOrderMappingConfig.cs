using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.JobOrders;

public class JobOrderMappingConfig : Profile
{
    public JobOrderMappingConfig()
    {
        CreateMap<JobOrderDto, Job_OrderVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<JobOrderByUserDto, Job_OrderVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<JobOrderByIdDto, Job_OrderVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ForMember(q => q.PendingClosureDate, opt => opt.MapFrom(x => x.PendingClosureDate!.Value.DateTime))
            .ForMember(q => q.DateCompleted, opt => opt.MapFrom(x => x.DateCompleted!.Value.DateTime))
            .ReverseMap();

        CreateMap<JobOrderBySupervisorDto, Job_OrderVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<JobOrderByAdminDto, Job_OrderVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<JobOrderByCEODto, Job_OrderVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated.DateTime))
            .ForMember(q => q.DateRequested, opt => opt.MapFrom(x => x.DateRequested!.Value.DateTime))
            .ForMember(q => q.DateModified, opt => opt.MapFrom(x => x.DateModified!.Value.DateTime))
            .ReverseMap();

        CreateMap<JobOrderApprovalByIdDto, Job_Order_ApprovalVM>()
            .ForMember(q => q.ActionDate, opt => opt.MapFrom(x => x.ActionDate!.Value.DateTime));

        CreateMap<CreateJobOrderCommand, Job_OrderVM>().ReverseMap();

        CreateMap<UpdateJobOrderCommand, Job_OrderVM>().ReverseMap();

        CreateMap<ApproveJobOrderCommand, ApproveJobOrderCommandVM>().ReverseMap();
    }
}