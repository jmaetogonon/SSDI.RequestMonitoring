using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;
using ApprovalAction = SSDI.RequestMonitoring.UI.Models.Enums.ApprovalAction;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.JobOrder;

public interface IJORequisitionSvc
{
    Task<Response<int>> CreateJORequisition(Job_Order_SlipVM slip);

    Task<Response<Guid>> EditJORequisition(Job_Order_SlipVM request);

    Task<Response<Guid>> DeleteJORequisition(int id);

    Task<Response<Guid>> ApproveJORequisition(Job_Order_SlipVM slip, ApprovalAction action, int approverId);

    Task<byte[]> GenerateJORequisitionPdf(int id);
}