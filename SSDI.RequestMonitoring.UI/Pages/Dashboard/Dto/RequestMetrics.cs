namespace SSDI.RequestMonitoring.UI.Pages.Dashboard.Dto;

public class RequestMetrics
{
    public int TotalCount { get; set; }
    public int DraftCount { get; set; }
    public int PendingSubmissionCount { get; set; }
    public int PendingAdminApprovalCount { get; set; }
    public int PendingAdminRequisitionCount { get; set; }
    public int PendingEndorsementCount { get; set; }
    public int PendingCEOApprovalCount { get; set; }
    public int PendingClosureCount { get; set; }
    public int AllPendingCount { get; set; }
    public int CompletedCount { get; set; }
    public int RejectedCount { get; set; }
    public List<ActivityItem> RecentActivity { get; set; } = new();
}