using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Contracts.MasterData;

public interface IDepartmentSvc
{
    Task<List<DepartmentVM>> GetAllDepartments(); 
    Task<Response<Guid>> BulkUpsertDepartments(List<DepartmentVM> departmentList);

}