using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Adapters.Requests.RequisitionSlip;

public class PRRequisitionSvcAdapter : IRequisitionSlipSvc
{
    private readonly IPRRequisitionSvc _svc;

    public PRRequisitionSvcAdapter(IPRRequisitionSvc svc) => _svc = svc;

    public async Task<Response<Guid>> ApproveRequisition(ISlipVM slip, Models.Enums.ApprovalAction action, int approverId)
    {
        if (slip is not Purchase_Request_SlipVM pr) return new Response<Guid>() { Success = false };
        return await _svc.ApprovePRRequisition(pr, action, approverId);
    }

    public async Task<Response<int>> CreateRequisition(ISlipVM slip, int headerId)
    {
        if (slip is not Purchase_Request_SlipVM pr) return await Task.FromResult(new Response<int>() { Success = false, Message = "Not Purchase VM" });
        pr.PurchaseRequestId = headerId;
        return await _svc.CreatePRRequisition(pr);
    }

    public Task<Response<Guid>> DeleteRequisition(int id)
        => _svc.DeletePRRequisition(id);

    public async Task<Response<Guid>> EditRequisition(ISlipVM request, int headerId)
    {
        if (request is not Purchase_Request_SlipVM pr) return new Response<Guid>() { Success = false };
        pr.PurchaseRequestId = headerId;
        return await _svc.EditPRRequisition(pr);
    }

    public Task<byte[]> GenerateRequisitionPdf(int id)
        => _svc.GeneratePRRequisitionPdf(id);
}