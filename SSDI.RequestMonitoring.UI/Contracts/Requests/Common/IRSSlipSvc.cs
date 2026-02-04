using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;
using ApprovalAction = SSDI.RequestMonitoring.UI.Models.Enums.ApprovalAction;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.Common;

public interface IRSSlipSvc
{
    Task<Response<int>> CreateRequisition(Request_RS_SlipVM slip, Models.Enums.RequestType type);

    Task<Response<Guid>> EditRequisition(Request_RS_SlipVM request);

    Task<Response<Guid>> DeleteRequisition(int id);

    Task<Response<Guid>> ApproveRequisition(Request_RS_SlipVM slip, ApprovalAction action, int approverId);

    Task<byte[]> GenerateRequisitionPdf(int id, string businessUnit);
}
