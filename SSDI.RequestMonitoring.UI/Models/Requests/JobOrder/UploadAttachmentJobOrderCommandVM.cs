namespace SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

public class UploadAttachmentJobOrderCommandVM
{
    public int JobOrderId { get; set; }
    public RequestAttachType Type { get; set; }
    public ICollection<Job_Order_AttachVM> Files { get; set; } = [];
    public int RequisitionId { get; set; }
    public decimal ReceiptAmount { get; set; }
    public string ReceiptRemarks { get; set; } = string.Empty;
}
