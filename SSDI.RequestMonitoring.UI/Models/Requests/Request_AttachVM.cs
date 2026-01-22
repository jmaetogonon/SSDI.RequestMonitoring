using SSDI.RequestMonitoring.UI.Models.Common;

namespace SSDI.RequestMonitoring.UI.Models.Requests;

public class Request_AttachVM
{
    public int Id { get; set; }
    public string UniqId { get; set; } = string.Empty;
    public int? PurchaseRequestId { get; set; }
    public int? JobOrderId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime? DateCreated { get; set; }
    public byte[]? ImgData { get; set; }
    public RequestAttachType AttachType { get; set; }
    public int RequisitionId { get; set; }
    public int POId { get; set; }
    public decimal ReceiptAmount { get; set; }
    public string ReceiptRemarks { get; set; } = string.Empty;
}