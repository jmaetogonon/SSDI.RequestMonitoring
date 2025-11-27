using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.JobOrders;

public class JOAttachMappingConfig : Profile
{
    public JOAttachMappingConfig()
    {
        CreateMap<JobOrderAttachByIdDto, Job_Order_AttachVM>()
            .ForMember(q => q.DateCreated, opt => opt.MapFrom(x => x.DateCreated!.Value.DateTime));

        CreateMap<Job_Order_Attach, Job_Order_AttachVM>().ReverseMap();

        CreateMap<UploadAttachmentJobOrderCommand, UploadAttachmentJobOrderCommandVM>().ReverseMap();
    }
}