using SSDI.RequestMonitoring.UI.Models.Common;

namespace SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

public class Job_OrderVM : BaseRequestVM
{
    public ICollection<Job_Order_ApprovalVM> Approvals { get; set; } = [];

    public override ICollection<IApprovalVM> ApprovalsBase => [.. Approvals.Cast<IApprovalVM>()];
}
