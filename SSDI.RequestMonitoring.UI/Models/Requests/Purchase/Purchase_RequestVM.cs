using System.ComponentModel.DataAnnotations;

namespace SSDI.RequestMonitoring.UI.Models.Requests.Purchase;

public class Purchase_RequestVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DivisionId { get; set; }
    public int DepartmentId { get; set; }
    public string Division_Department { get; set; } = string.Empty;
    public string Nature_Of_Request { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }

    [Required]
    public RequestPriority Priority { get; set; }

    public string OtherPriority { get; set; } = string.Empty;

    public int? RequestedById { get; set; } = null;
    public string RequestedByName { get; set; } = string.Empty;

    public int? RequestedByDeptHeadId { get; set; } = null;
    public string RequestedByDeptHeadName { get; set; } = string.Empty;

    public DateTime? DateRequested { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public DateTime? DateCompleted { get; set; }
    public bool IsCompleted { get; set; }

    public ICollection<Purchase_Request_ApprovalVM> Approvals { get; set; } = [];
    public ICollection<Purchase_Request_AttachVM> Attachments { get; set; } = [];
    public ICollection<Purchase_Request_SlipVM> RequisitionSlips { get; set; } = [];

    public bool IsDirectReport { get; set; }
    public string ReportType { get; set; } = string.Empty;

    public int? ReportToDeptSupId { get; set; } = null;
    public int? ReportToDivSupId { get; set; } = null;

    // for pending close workflow
    public DateTime? PendingClosureDate { get; set; } 
    public int? PendingClosureRequestedById { get; set; } = null;

    // for ui
    public ApprovalStage? CurrentStage
    {
        get
        {
            if (Status == RequestStatus.Rejected)
            {
                // find where rejection happened
                var rejectedStep = Approvals
                    .Where(a => a.Action == ApprovalAction.Reject)
                    .OrderByDescending(a => a.ActionDate)
                    .FirstOrDefault();

                return rejectedStep?.Stage;
            }

            // otherwise, infer current active stage from status
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
