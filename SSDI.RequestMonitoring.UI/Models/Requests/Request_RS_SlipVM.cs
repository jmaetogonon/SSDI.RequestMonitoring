using System.ComponentModel.DataAnnotations.Schema;

namespace SSDI.RequestMonitoring.UI.Models.Requests;

public class Request_RS_SlipVM
{
    public int Id { get; set; }

    public string SeriesNumber { get; set; } = string.Empty;
    public int? Job_OrderId { get; set; } = null;
    public int? Purchase_RequestId { get; set; } = null;

    public RequisitionSlip_For RequisitionSlip_For { get; set; } = RequisitionSlip_For.CashPayment;
    public string OthersRequisitionSlip_For { get; set; } = string.Empty;
    //public RequisitionSlip_Dept RequisitionSlip_Dept { get; set; } = RequisitionSlip_Dept.Acctg;

    public int RequisitionerId { get; set; }
    public string RequisitionerName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountRequested { get; set; }

    public DateTime? DateOfRequest { get; set; }

    //CASH ADVANCES
    public string CA_Activity { get; set; } = string.Empty;

    public string CA_TDPorCircularNo { get; set; } = string.Empty;
    public string CA_AWPNo { get; set; } = string.Empty;
    public DateTime? CA_LiquidationDueDate { get; set; }

    //PAYMENT
    public int Payment_PayeeId { get; set; }
    public string Payment_PayeeName { get; set; } = string.Empty;

    public string Payment_PaymentDetails { get; set; } = string.Empty;
    public string Payment_References { get; set; } = string.Empty;
    public string Payment_PaymentTerms { get; set; } = string.Empty;

    //MATERIALS & SUPPLIES/REPAIRS & MAINTENANCE
    public string Mat_IntendedFor { get; set; } = string.Empty;

    public string Mat_PreferredSupplier { get; set; } = string.Empty;
    public string Mat_RefNo { get; set; } = string.Empty;
    public string Mat_PaymentTerms { get; set; } = string.Empty;

    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public ApprovalAction Approval { get; set; } = ApprovalAction.Pending;
    public int? SlipApproverId { get; set; }
    public string SlipApproverName { get; set; } = string.Empty;
    public DateTime? SlipApprovalDate { get; set; } = null;

    public int NoOfdaysToLiquidate { get; set; }
}