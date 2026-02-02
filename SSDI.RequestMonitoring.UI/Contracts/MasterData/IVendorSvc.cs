using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Contracts.MasterData;

public interface IVendorSvc
{
    Task<List<VendorVM>> GetAllVendors();

    Task<bool> SyncVendors();

    Task<bool> IsExistVendor(string code);

    Task<bool> AddVendorToServer(VendorVM vendor);
}