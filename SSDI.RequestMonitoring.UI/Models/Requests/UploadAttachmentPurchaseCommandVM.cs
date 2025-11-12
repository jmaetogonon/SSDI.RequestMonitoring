using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Models.Requests;

public class UploadAttachmentPurchaseCommandVM
{
    public int PurchaseRequestId { get; set; }
    public ICollection<Purchase_Request_AttachVM> Files { get; set; } = [];
}
