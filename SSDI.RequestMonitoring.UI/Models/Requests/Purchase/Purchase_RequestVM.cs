using SSDI.RequestMonitoring.UI.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace SSDI.RequestMonitoring.UI.Models.Requests.Purchase;

public class Purchase_RequestVM : BaseRequestVM
{
    public ICollection<Purchase_Request_ApprovalVM> Approvals { get; set; } = [];
    public ICollection<Purchase_Request_AttachVM> Attachments { get; set; } = [];
    public ICollection<Purchase_Request_SlipVM> RequisitionSlips { get; set; } = [];

    public override ICollection<IApprovalVM> ApprovalsBase => Approvals.Cast<IApprovalVM>().ToList();
    public override ICollection<IAttachmentVM> AttachmentsBase => Attachments.Cast<IAttachmentVM>().ToList();
    public override ICollection<ISlipVM> SlipsBase => RequisitionSlips.Cast<ISlipVM>().ToList();
}
