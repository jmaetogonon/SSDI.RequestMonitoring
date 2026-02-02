using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Contracts.MasterData;

public interface IBusinessUnitSvc
{
    Task<List<BusinessUnitVM>> GetAllBusinessUnits();

    Task<bool> SyncBusinessUnits();
}