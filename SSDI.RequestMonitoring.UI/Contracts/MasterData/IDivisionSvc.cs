using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Contracts.MasterData;

public interface IDivisionSvc
{
    Task<List<DivisionVM>> GetAllDivisions();
    Task<Response<Guid>> BulkUpsertDivisions(List<DivisionVM> divisionList);

}