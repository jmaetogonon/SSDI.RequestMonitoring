using SSDI.RequestMonitoring.UI.Helpers.States;
using SSDI.RequestMonitoring.UI.Models.Enums;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.Helpers;

public class Utils
{
    private readonly CurrentUser _currentUser;

    public Utils(CurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public static string GenerateGuid() => Guid.NewGuid().ToString();

    public static string FormatDate(DateTime date) => date.ToString("yyyy-MM-dd");

    public bool IsUser() => _currentUser.Role == TokenCons.Role__User;

    public bool IsAdmin() => _currentUser.Role == TokenCons.Role__Admin;

    public bool IsSupervisor() => _currentUser.RoleDesc.Contains(TokenCons.Role__Supervisor, StringComparison.OrdinalIgnoreCase);

    public string GetPriorityDisplay(RequestPriority priority, string otherPriority, bool isShort = false)
    {
        return priority switch
        {
            RequestPriority.Hrs24 => isShort ? "24 hrs" : "Must be done within 24 hours",
            RequestPriority.ThisWeek => isShort ? "This Week" : "Must be done within the week",
            _ => otherPriority
        };
    }

    public string GetRelativeTime(DateTime? date)
    {
        if (date is null) return "Unknown";

        var timeSpan = DateTime.Now - date.Value;

        if (timeSpan.TotalSeconds < 0) return "Future";

        return timeSpan.TotalSeconds switch
        {
            < 60 => "Just now",
            < 3600 => $"{timeSpan.Minutes}m ago",
            < 86400 => $"{timeSpan.Hours}h ago",
            < 604800 => $"{timeSpan.Days}d ago",
            < 2592000 => $"{timeSpan.Days / 7}w ago",
            < 31536000 => $"{timeSpan.Days / 30}mo ago",
            _ => $"{timeSpan.Days / 365}y ago"
        };

        //return timeSpan.TotalDays switch
        //{
        //    >= 365 => $"{(int)(timeSpan.TotalDays / 365)} year{(timeSpan.TotalDays / 365 >= 2 ? "s" : "")} ago",
        //    >= 30 => $"{(int)(timeSpan.TotalDays / 30)} month{(timeSpan.TotalDays / 30 >= 2 ? "s" : "")} ago",
        //    >= 7 => $"{(int)(timeSpan.TotalDays / 7)} week{(timeSpan.TotalDays / 7 >= 2 ? "s" : "")} ago",
        //    >= 1 => $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago",
        //    >= 1 / 24.0 => $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago",
        //    >= 1 / 1440.0 => $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago",
        //    _ => "Just now"
        //};
    }

    public string GetStatusDisplay(RequestStatus item)
    {
        return item switch
        {
            RequestStatus.Draft => TokenCons.Status__Draft,
            RequestStatus.ForEndorsement => TokenCons.Status__ForEndorsement,
            RequestStatus.ForAdminVerification => TokenCons.Status__ForAdminVerification,
            RequestStatus.ForCeoApproval => TokenCons.Status__ForCeoApproval,
            RequestStatus.ForFinanceApproval => TokenCons.Status__ForFinanceApproval,
            RequestStatus.Approved => TokenCons.Status__Approved,
            RequestStatus.Rejected => TokenCons.Status__Rejected,
            RequestStatus.Cancelled => TokenCons.Status__Cancelled,
            _ => "ambot"
        };
    }

    public string GetStatusIcon(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Draft => "bi bi-file-earmark-plus",
            RequestStatus.ForEndorsement => "bi bi-cart",
            RequestStatus.ForAdminVerification => "bi bi-send",
            RequestStatus.ForCeoApproval => "bi bi-patch-check",
            RequestStatus.ForFinanceApproval => "bi bi-check-circle",
            RequestStatus.Approved => "bi bi-archive",
            RequestStatus.Rejected => "bi bi-x-circle",
            RequestStatus.Cancelled => "bi bi-x-octagon",
            _ => "bi bi-circle" // Default icon
        };
    }

    public string GetApprovalStatusText(Purchase_Request_ApprovalVM approval)
    {
        return approval.IsApproved ? (approval.Stage is ApprovalStage.DepartmentHead ? "Submitted" : "Approved") :
               approval.IsRejected ? "Rejected" :
               approval.IsCancelled ? "Cancelled" :
               "Pending";
    }

    public string GetApprovalByText(Purchase_Request_ApprovalVM approval)
    {
        return approval.Stage switch
        {
            ApprovalStage.DepartmentHead => "Requester",
            ApprovalStage.DivisionHead => "Endorser",
            ApprovalStage.Admin => "Verifier",
            ApprovalStage.CeoOrAvp => "Approver",
            ApprovalStage.Finance => "Finance",
            _ => "ambot"
        };
    }

    public string GetApprovalByTimelineText(Purchase_Request_ApprovalVM approval)
    {
        return approval.Stage switch
        {
            ApprovalStage.DepartmentHead => "Submitted by",
            ApprovalStage.DivisionHead => "Endorsed by",
            ApprovalStage.Admin => "Verified by",
            ApprovalStage.CeoOrAvp => "Approved by",
            ApprovalStage.Finance => "Verified by",
            _ => "ambot"
        };
    }

    public string GetApprovalStagesDisplay(ApprovalStage item)
    {
        return item switch
        {
            ApprovalStage.DepartmentHead => "Department Head",
            ApprovalStage.DivisionHead => "Division Head",
            ApprovalStage.Admin => "Admin",
            ApprovalStage.CeoOrAvp => "CEO / AVP",
            ApprovalStage.Finance => "Finance",
            _ => "ambot"
        };
    }
}