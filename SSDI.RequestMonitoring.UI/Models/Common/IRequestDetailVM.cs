namespace SSDI.RequestMonitoring.UI.Models.Common;

public interface IRequestDetailVM
{
    int Id { get; }
    string Name { get; }
    string Division_Department { get; }
    string Nature_Of_Request { get; }
    string Justification { get; }

    RequestPriority Priority { get; }
    string OtherPriority { get; }

    RequestStatus Status { get; }

    string? RequestedByDeptHeadName { get; }
    DateTime? DateRequested { get; }
    DateTime? DateCreated { get; }

    ICollection<IApprovalEntity> Approvals { get; }
}
