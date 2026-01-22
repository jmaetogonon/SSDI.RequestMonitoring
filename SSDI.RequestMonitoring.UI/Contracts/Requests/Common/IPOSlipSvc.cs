using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;
using ApprovalAction = SSDI.RequestMonitoring.UI.Models.Enums.ApprovalAction;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.Common;

public interface IPOSlipSvc
{
    Task<Response<int>> CreatePOSlip(Request_PO_SlipVM slip);

    Task<Response<Guid>> EditPO(Request_PO_SlipVM request);

    Task<Response<Guid>> DeletePO(int id);

    Task<Response<Guid>> ApprovePO(Request_PO_SlipVM slip, ApprovalAction action, int approverId);

    Task<byte[]> GeneratePOPdf(int id);
}
