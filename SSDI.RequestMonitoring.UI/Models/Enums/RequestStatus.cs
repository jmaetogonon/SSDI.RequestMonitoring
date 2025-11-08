namespace SSDI.RequestMonitoring.UI.Models.Enums;

public enum RequestStatus
{
    Draft,
    ForEndorsement,
    ForAdminVerification,
    ForCeoApproval,
    ForFinanceApproval,
    Approved,
    Rejected,
    Cancelled
}
