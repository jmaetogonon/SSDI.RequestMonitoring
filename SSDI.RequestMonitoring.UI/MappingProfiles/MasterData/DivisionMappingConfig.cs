using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.MasterData;

public class DivisionMappingConfig : Profile
{
    public DivisionMappingConfig()
    {
        CreateMap<DivisionDto, DivisionVM>().ReverseMap();
    }
}
