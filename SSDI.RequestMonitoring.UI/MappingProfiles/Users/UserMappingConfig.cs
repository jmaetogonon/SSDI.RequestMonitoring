using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.Users;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.Users;

public class UserMappingConfig : Profile
{
    public UserMappingConfig()
    {
        CreateMap<SupervisorDto, SupervisorVM>().ReverseMap();

        CreateMap<UserFile, UserFileVM>().ReverseMap();
        CreateMap<UserDto, UserVM>().ReverseMap();
    }
}