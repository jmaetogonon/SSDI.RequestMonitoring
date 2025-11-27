namespace SSDI.RequestMonitoring.UI.Models.Common
{
    public interface IApprovalEntity
    {
        ApprovalStage Stage { get; }
        bool IsApproved { get; }
        bool IsRejected { get; }
        bool IsCancelled { get; }
        bool IsPending { get; }
    }
}