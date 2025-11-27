namespace SSDI.RequestMonitoring.UI.Models.Enums;

public enum RequestStatus
{
    Draft,
    ForEndorsement,
    ForAdminVerification,
    ForCeoApproval,
    ForRequisition,
    Approved,
    Rejected,
    Cancelled,
    PendingRequesterClosure,
    Closed
}
