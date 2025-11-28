using SSDI.RequestMonitoring.UI.Models.Common;

namespace SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

public class Job_Order_ApprovalVM : IApprovalVM
{
    public int Id { get; set; }
    public int JobOrderId { get; set; }
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