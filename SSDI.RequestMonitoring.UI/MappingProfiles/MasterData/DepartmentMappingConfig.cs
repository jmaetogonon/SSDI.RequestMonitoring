using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.MasterData;

public class DepartmentMappingConfig : Profile
{
    public DepartmentMappingConfig()
    {
        CreateMap<SystemConfigDto, SystemConfigVM>().ReverseMap();
        CreateMap<DepartmentDto, DepartmentVM>().ReverseMap();
    }
}