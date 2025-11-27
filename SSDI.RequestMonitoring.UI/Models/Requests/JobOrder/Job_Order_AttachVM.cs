using SSDI.RequestMonitoring.UI.Models.Common;

namespace SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;

public class Job_Order_AttachVM : IAttachmentEntity
{
    public int Id { get; set; }
    public string UniqId { get; set; } = string.Empty;
    public int PurchaseRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime? DateCreated { get; set; }
    public byte[]? ImgData { get; set; }
    public RequestAttachType AttachType { get; set; }
    public int RequisitionId { get; set; }
    public decimal ReceiptAmount { get; set; }
}
