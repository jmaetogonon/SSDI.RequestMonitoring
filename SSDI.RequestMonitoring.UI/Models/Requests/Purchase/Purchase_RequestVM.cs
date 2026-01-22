using SSDI.RequestMonitoring.UI.Models.Common;

namespace SSDI.RequestMonitoring.UI.Models.Requests.Purchase;

public class Purchase_RequestVM : BaseRequestVM
{
    public ICollection<Purchase_Request_ApprovalVM> Approvals { get; set; } = [];

    public override ICollection<IApprovalVM> ApprovalsBase => [.. Approvals.Cast<IApprovalVM>()];
}