using AutoMapper;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.MappingProfiles.MasterData;

public class MasterDataMappingConfig : Profile
{
    public MasterDataMappingConfig()
    {
        // System Config
        CreateMap<SystemConfigDto, SystemConfigVM>().ReverseMap();

        // Division
        CreateMap<DivisionDto, DivisionVM>().ReverseMap();

        // Department
        CreateMap<DepartmentDto, DepartmentVM>().ReverseMap();

        // Business Unit
        CreateMap<BusinessUnitDto, BusinessUnitVM>().ReverseMap();

        // Vendor
        CreateMap<VendorDto, VendorVM>().ReverseMap();
        CreateMap<VendorVM, AddVendorToServerCommand>();
    }
}