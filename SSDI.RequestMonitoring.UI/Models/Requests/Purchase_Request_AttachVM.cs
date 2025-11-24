namespace SSDI.RequestMonitoring.UI.Models.Requests;

public class Purchase_Request_AttachVM
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
}
