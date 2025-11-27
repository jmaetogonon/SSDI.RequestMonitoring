namespace SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

public class UploadAttachmentJobOrderCommandVM
{
    public int PurchaseRequestId { get; set; }
    public RequestAttachType Type { get; set; }
    public ICollection<Job_Order_AttachVM> Files { get; set; } = [];
    public int RequisitionId { get; set; }
    public decimal ReceiptAmount { get; set; }
}
