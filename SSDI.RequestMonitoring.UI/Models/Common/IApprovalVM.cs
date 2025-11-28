namespace SSDI.RequestMonitoring.UI.Models.Common
{
    public interface IApprovalVM
    {
        int Id { get; }
        ApprovalStage Stage { get; }
        ApprovalAction? Action { get; }
        int ApproverId { get; }
        string ApproverName { get; }
        string Remarks { get; }
        bool IsApproved { get; }
        bool IsRejected { get; }
        bool IsCancelled { get; }
        bool IsPending { get; }
        DateTime? ActionDate { get; }
    }
}