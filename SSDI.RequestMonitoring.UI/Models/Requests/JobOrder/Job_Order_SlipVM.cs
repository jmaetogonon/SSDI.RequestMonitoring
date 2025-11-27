using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

public class Job_Order_SlipVM
{
    public int Id { get; set; }
    public Purchase_RequestVM? PurchaseRequest { get; set; }
    public int PurchaseRequestId { get; set; }

    public RequisitionSlip_For RequisitionSlip_For { get; set; } = RequisitionSlip_For.CashPayment;
    public string OthersRequisitionSlip_For { get; set; } = string.Empty;
    public RequisitionSlip_Dept RequisitionSlip_Dept { get; set; } = RequisitionSlip_Dept.Acctg;

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
    public string Payment_Payee { get; set; } = string.Empty;

    public string Payment_PaymentDetails { get; set; } = string.Empty;
    public string Payment_References { get; set; } = string.Empty;
    public string Payment_PaymentTerms { get; set; } = string.Empty;

    //MATERIALS & SUPPLIES/REPAIRS & MAINTENANCE
    public string Mat_IntendedFor { get; set; } = string.Empty;

    public string Mat_PreferredSupplier { get; set; } = string.Empty;
    public string Mat_RefNo { get; set; } = string.Empty;
    public string Mat_PaymentTerms { get; set; } = string.Empty;

    //OTHERS
    public string Others_IntentedFor { get; set; } = string.Empty;

    public string Others_References { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public ApprovalAction Approval { get; set; } = ApprovalAction.Pending;
    public string SlipApproverName { get; set; } = string.Empty;
    public DateTime? SlipApprovalDate { get; set; } = null;
}