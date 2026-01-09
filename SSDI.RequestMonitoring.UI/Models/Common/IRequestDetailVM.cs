using SSDI.RequestMonitoring.UI.Models.Common;

public interface IRequestDetailVM
{
    int Id { get; }

    string Name { get; }
    string RequestNumber { get; }
    string SeriesNumber { get; }
    int DivisionId { get; }
    int DepartmentId { get; }
    string Division_Department { get; }

    string Nature_Of_Request { get; }
    string Justification { get; }

    RequestStatus Status { get; }
    RequestPriority Priority { get; }
    string OtherPriority { get; }

    int? RequestedById { get; }
    string RequestedByName { get; }

    int? RequestedByDeptHeadId { get; }
    string RequestedByDeptHeadName { get; }

    DateTime? DateRequested { get; }
    DateTime? DateCreated { get; }
    DateTime? DateModified { get; }
    DateTime? DateCompleted { get; }
    bool IsCompleted { get; }

    bool IsDirectReport { get; }
    string ReportType { get; }

    int? ReportToDeptSupId { get; }
    int? ReportToDivSupId { get; }

    DateTime? PendingClosureDate { get; }
    int? PendingClosureRequestedById { get; }

    int? UserDepartmentHeadId { get; }
    int? UserDivisionHeadId { get; }

    public bool IsNoUser { get; }

    // Polymorphic collections
    ICollection<IApprovalVM> ApprovalsBase { get; }

    ICollection<IAttachmentVM> AttachmentsBase { get; }
    ICollection<ISlipVM> SlipsBase { get; }

    // Computed stage (shared logic in BaseRequestVM)
    ApprovalStage? CurrentStage { get; }
}