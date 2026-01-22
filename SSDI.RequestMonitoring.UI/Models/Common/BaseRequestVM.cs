using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.Models.Common;

public abstract class BaseRequestVM : IRequestDetailVM
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string SeriesNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int DivisionId { get; set; }
    public int DepartmentId { get; set; }
    public string Division_Department { get; set; } = string.Empty;

    public string Nature_Of_Request { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;

    public RequestStatus Status { get; set; }
    public RequestPriority Priority { get; set; }
    public string OtherPriority { get; set; } = string.Empty;

    public int? RequestedById { get; set; }
    public string RequestedByName { get; set; } = string.Empty;

    public int? RequestedByDeptHeadId { get; set; }
    public string RequestedByDeptHeadName { get; set; } = string.Empty;

    public DateTime? DateRequested { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public DateTime? DateCompleted { get; set; }
    public bool IsCompleted { get; set; }

    // Reporting
    public bool IsDirectReport { get; set; }

    public string ReportType { get; set; } = string.Empty;
    public int? ReportToDeptSupId { get; set; }
    public int? ReportToDivSupId { get; set; }

    // Pending closure
    public DateTime? PendingClosureDate { get; set; }

    public int? PendingClosureRequestedById { get; set; }

    public int? UserDepartmentHeadId { get; set; }
    public int? UserDivisionHeadId { get; set; }

    public bool IsNoUser { get; set; } = false;

    public ICollection<Request_AttachVM> Attachments { get; set; } = [];
    public ICollection<Request_PO_SlipVM> POSlips { get; set; } = [];
    public ICollection<Request_RS_SlipVM> RequisitionSlips { get; set; } = [];

    public abstract ICollection<IApprovalVM> ApprovalsBase { get; }

    // Shared Stage Computation Logic
    public ApprovalStage? CurrentStage
    {
        get
        {
            if (Status == RequestStatus.Rejected)
            {
                var rejectedStep = ApprovalsBase
                    .Where(a => a.Action == ApprovalAction.Reject)
                    .OrderByDescending(a => a.ActionDate)
                    .FirstOrDefault();

                return rejectedStep?.Stage;
            }

            return Status switch
            {
                RequestStatus.ForEndorsement => ApprovalStage.DepartmentHead,
                RequestStatus.ForAdminVerification => ApprovalStage.DivisionHead,
                RequestStatus.ForCeoApproval => ApprovalStage.Admin,
                RequestStatus.ForRequisition => ApprovalStage.CeoOrAvp,
                _ => null
            };
        }
    }
}