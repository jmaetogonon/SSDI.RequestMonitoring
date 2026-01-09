using SSDI.RequestMonitoring.UI.Helpers.States;
using SSDI.RequestMonitoring.UI.Models.Common;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;

namespace SSDI.RequestMonitoring.UI.Helpers;

public class Utils
{
    private readonly CurrentUser _currentUser;

    public Utils(CurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public string GenerateGuid() => Guid.NewGuid().ToString();

    public string GenerateUniqId() => DateTime.Now.ToString("yyyyMMddhhmmssfff");

    public string FormatDate(DateTime date) => date.ToString("yyyy-MM-dd");
    
    public string GetLastModifiedDisplay(DateTime? dateModified)
    {
        if (dateModified is null) return "Never modified";

        var now = DateTime.Now;
        var timeSpan = now - dateModified.Value;

        return timeSpan.TotalDays switch
        {
            < 1 when timeSpan.TotalHours < 1 => $"Updated {timeSpan.Minutes}m ago",
            < 1 => $"Updated {timeSpan.Hours}h ago",
            < 7 => $"Updated {timeSpan.Days}d ago",
            < 30 => $"Updated {timeSpan.Days / 7}w ago",
            _ => $"Updated {dateModified.Value:MMM dd, yyyy}"
        };
    }

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

    public string GetRequestTypeName(RequestType type) =>
    type switch
    {
        RequestType.Purchase => "Purchase",
        RequestType.JobOrder => "Job Orders",
        _ => "All"
    };

    public string FirstCharToUpper(string input) =>
        string.IsNullOrEmpty(input) ? input : char.ToUpper(input[0]) + input.Substring(1);

    public string GetStatusDisplay(RequestStatus item)
    {
        return item switch
        {
            RequestStatus.Draft => TokenCons.Status__Draft,
            RequestStatus.ForEndorsement => TokenCons.Status__ForEndorsement,
            RequestStatus.ForAdminVerification => TokenCons.Status__ForAdminVerification,
            RequestStatus.ForCeoApproval => TokenCons.Status__ForCeoApproval,
            RequestStatus.ForRequisition => TokenCons.Status__ForRequisition,
            RequestStatus.Rejected => TokenCons.Status__Rejected,
            RequestStatus.Cancelled => TokenCons.Status__Cancelled,
            RequestStatus.PendingRequesterClosure => TokenCons.Status__PendingClose,
            RequestStatus.Closed => TokenCons.Status__Closed,
            _ => "ambot"
        };
    }

    public string GetStatusIcon(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Draft => "bi bi-file-earmark-text",           // Document being written
            RequestStatus.ForEndorsement => "bi bi-send-check",         // Sent for check/approval
            RequestStatus.ForAdminVerification => "bi bi-clipboard-check", // Checklist verification
            RequestStatus.ForCeoApproval => "bi bi-award",              // High-level approval
            RequestStatus.ForRequisition => "bi bi-cart-check",         // Shopping cart approved
            RequestStatus.Approved => "bi bi-check-circle-fill",        // Solid approval
            RequestStatus.Rejected => "bi bi-x-circle-fill",            // Solid rejection
            RequestStatus.Cancelled => "bi bi-slash-circle",            // Clearly cancelled
            RequestStatus.PendingRequesterClosure => "bi bi-clock",     // Time/awaiting action
            RequestStatus.Closed => "bi bi-archive-fill",               // Solid archived
            _ => "bi bi-question-circle"                                // Unknown state
        };
    }

    public string GetStatusColor(RequestStatus status) =>
        status switch
        {
            RequestStatus.Draft => "#9ca3af",
            RequestStatus.ForEndorsement => "#60a5fa",
            RequestStatus.ForAdminVerification => "#818cf8",
            RequestStatus.ForCeoApproval => "#a78bfa",
            RequestStatus.ForRequisition => "#34d399",
            RequestStatus.Rejected => "#f87171",
            RequestStatus.PendingRequesterClosure => "#fbbf24",
            RequestStatus.Closed => "#6b7280",
            RequestStatus.Cancelled => "#94a3b8",
            _ => "#d1d5db" // default gray
        };

    public string GetApprovalStatusText(IApprovalVM approval)
    {
        return approval.IsApproved ? (approval.Stage is ApprovalStage.DepartmentHead ? "Submitted" : "Approved") :
               approval.IsRejected ? "Rejected" :
               approval.IsCancelled ? "Cancelled" :
               "Pending";
    }

    public string GetApprovalByText(IApprovalVM approval)
    {
        return approval.Stage switch
        {
            ApprovalStage.DepartmentHead => "Requester",
            ApprovalStage.DivisionHead => "Endorser",
            ApprovalStage.Admin => "Verifier",
            ApprovalStage.CeoOrAvp => "Approver",
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
            _ => "ambot"
        };
    }

    public string FormatPendingAge(DateTime since)
    {
        var span = DateTime.Now - since;

        if (span.TotalMinutes < 60)
            return $"{span.Minutes}m";

        if (span.TotalHours < 24)
            return $"{span.Hours}h {span.Minutes}m";

        return $"{span.Days}d {span.Hours}h";
    }
}