namespace SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

public class ApproveJobOrderCommandVM
{
    public int RequestId { get; set; }
    public ApprovalStage Stage { get; set; }
    public int ApproverId { get; set; }
    public ApprovalAction Action{ get; set; }
    public string? Remarks { get; set; }
}
