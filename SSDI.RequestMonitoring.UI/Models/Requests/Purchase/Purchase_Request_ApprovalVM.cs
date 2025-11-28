using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Enums;

namespace SSDI.RequestMonitoring.UI.Models.Requests.Purchase;

public class Purchase_Request_ApprovalVM : IApprovalVM
{
    public int Id { get; set; }
    public int PurchaseRequestId { get; set; }

    public ApprovalStage Stage { get; set; } // Department, Division, Admin, Finance, CEO
    public ApprovalAction? Action { get; set; } // Approve, Reject, or null if pending
    public int ApproverId { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public DateTime? ActionDate { get; set; }

    // Helper properties for UI
    public bool IsApproved => Action == ApprovalAction.Approve;

    public bool IsRejected => Action == ApprovalAction.Reject;
    public bool IsCancelled => Action == ApprovalAction.Cancel;
    public bool IsPending => Action == null;
}
