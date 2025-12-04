using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Services.Base;
using ApprovalAction = SSDI.RequestMonitoring.UI.Models.Enums.ApprovalAction;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.Common;

public interface IRequisitionSlipSvc
{
    Task<Response<int>> CreateRequisition(ISlipVM slip, int headerId);

    Task<Response<Guid>> EditRequisition(ISlipVM request, int headerId);

    Task<Response<Guid>> DeleteRequisition(int id);

    Task<Response<Guid>> ApproveRequisition(ISlipVM slip, ApprovalAction action, int approverId);

    Task<byte[]> GenerateRequisitionPdf(int id);
}
