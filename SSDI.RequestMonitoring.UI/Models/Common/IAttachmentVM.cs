namespace SSDI.RequestMonitoring.UI.Models.Common;

public interface IAttachmentVM
{
    int Id { get; }
    string UniqId { get; }
    string FileName { get; }
    string URL { get; }
    long Size { get; }
    string ContentType { get; }
    DateTime? DateCreated { get; }
    byte[]? ImgData { get; set; }
    RequestAttachType AttachType { get; }
    int RequisitionId { get; }
    decimal ReceiptAmount { get; }
}