using SSDI.RequestMonitoring.UI.Models.Common;

namespace SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

public class Job_OrderVM : BaseRequestVM
{
    public ICollection<Job_Order_ApprovalVM> Approvals { get; set; } = [];
    public ICollection<Job_Order_AttachVM> Attachments { get; set; } = [];
    public ICollection<Job_Order_SlipVM> RequisitionSlips { get; set; } = [];

    public override ICollection<IApprovalVM> ApprovalsBase => Approvals.Cast<IApprovalVM>().ToList();
    public override ICollection<IAttachmentVM> AttachmentsBase => Attachments.Cast<IAttachmentVM>().ToList();
    public override ICollection<ISlipVM> SlipsBase => RequisitionSlips.Cast<ISlipVM>().ToList();
}
