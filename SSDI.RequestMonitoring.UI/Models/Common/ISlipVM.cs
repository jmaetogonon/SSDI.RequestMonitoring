namespace SSDI.RequestMonitoring.UI.Models.Common;
public interface ISlipVM
{
    int Id { get; set; }
    RequisitionSlip_For RequisitionSlip_For { get; set; }
    string OthersRequisitionSlip_For { get; set; }
    RequisitionSlip_Dept RequisitionSlip_Dept { get; set; }

    int RequisitionerId { get; set; }
    string RequisitionerName { get; set; }

    decimal AmountRequested { get; set; }
    DateTime? DateOfRequest { get; set; }

    string CA_Activity { get; set; }
    string CA_TDPorCircularNo { get; set; }
    string CA_AWPNo { get; set; }
    DateTime? CA_LiquidationDueDate { get; set; }

    string Payment_Payee { get; set; }
    string Payment_PaymentDetails { get; set; }
    string Payment_References { get; set; }
    string Payment_PaymentTerms { get; set; }

    string Mat_IntendedFor { get; set; }
    string Mat_PreferredSupplier { get; set; }
    string Mat_RefNo { get; set; }
    string Mat_PaymentTerms { get; set; }

    string Others_IntentedFor { get; set; }
    string Others_References { get; set; }

    DateTime DateCreated { get; set; }
    DateTime? DateModified { get; set; }
    ApprovalAction Approval { get; set; }
    string SlipApproverName { get; set; }
    DateTime? SlipApprovalDate { get; set; }
}
