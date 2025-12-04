using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.Contracts.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services.Adapters.Requests.RequisitionSlip;

public class JORequisitionSvcAdapter : IRequisitionSlipSvc
{
    private readonly IJORequisitionSvc _svc;

    public JORequisitionSvcAdapter(IJORequisitionSvc svc) => _svc = svc;

    public async Task<Response<Guid>> ApproveRequisition(ISlipVM slip, Models.Enums.ApprovalAction action, int approverId)
    {
        if (slip is not Job_Order_SlipVM pr) return new Response<Guid>() { Success = false };
        return await _svc.ApproveJORequisition(pr, action, approverId);
    }

    public async Task<Response<int>> CreateRequisition(ISlipVM slip, int headerId)
    {
        if (slip is not Job_Order_SlipVM pr) return await Task.FromResult(new Response<int>() { Success = false, Message="Not Job Order VM" });
        pr.JobOrderId = headerId;
        return await _svc.CreateJORequisition(pr);
    }

    public Task<Response<Guid>> DeleteRequisition(int id)
        => _svc.DeleteJORequisition(id);

    public async Task<Response<Guid>> EditRequisition(ISlipVM request, int headerId)
    {
        if (request is not Job_Order_SlipVM pr) return new Response<Guid>() { Success = false };
        pr.JobOrderId = headerId;
        return await _svc.EditJORequisition(pr);
    }

    public Task<byte[]> GenerateRequisitionPdf(int id)
        => _svc.GenerateJORequisitionPdf(id);
}