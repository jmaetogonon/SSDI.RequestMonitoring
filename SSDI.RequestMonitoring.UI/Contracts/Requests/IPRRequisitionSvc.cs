using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;
using ApprovalAction = SSDI.RequestMonitoring.UI.Models.Enums.ApprovalAction;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests;

public interface IPRRequisitionSvc
{
    Task<Response<int>> CreatePRRequisition(Purchase_Request_SlipVM slip);

    Task<Response<Guid>> EditPRRequisition(Purchase_Request_SlipVM request);

    Task<Response<Guid>> DeletePRRequisition(int id);

    Task<Response<Guid>> ApprovePRRequisition(Purchase_Request_SlipVM slip, ApprovalAction action, int approverId);

    Task<byte[]> GeneratePRRequisitionPdf(int id);
}