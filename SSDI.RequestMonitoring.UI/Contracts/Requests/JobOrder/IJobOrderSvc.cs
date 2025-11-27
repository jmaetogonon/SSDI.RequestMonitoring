using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.JobOrder;

public interface IJobOrderSvc
{
    Task<List<Job_OrderVM>> GetAllJobOrders();

    Task<List<Job_OrderVM>> GetAllJobOrdersByUser(int userId);

    Task<List<Job_OrderVM>> GetAllJobOrdersByAdmin();

    Task<List<Job_OrderVM>> GetAllJobOrderByCeo();

    Task<List<Job_OrderVM>> GetAllJobOrderBySupervisor(int supervisorId, bool includeDepartmentMembers = true, bool includeDivisionMembers = true);

    Task<Job_OrderVM> GetByIdJobOrder(int id);

    Task<Response<Guid>> CreateJobOrder(Job_OrderVM JobOrder);

    Task<Response<Guid>> UpdateJobOrder(int id, Job_OrderVM JobOrder);

    Task<Response<Guid>> DeleteJobOrder(int id);

    Task<Response<Guid>> ApproveJobOrder(ApproveJobOrderCommandVM command);

    Task<byte[]> GenerateJobOrderPdf(int id);

    Task<Response<Guid>> InitiateCloseJobOrder(int id, int initiatorId);

    Task<Response<Guid>> ConfirmCloseJobOrder(int id, int userId);

    Task<Response<Guid>> CancelPendingCloseJobOrder(int id, int userId);
}
