using SSDI.RequestMonitoring.UI.Models.Requests;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests;

public interface IPurchaseRequestSvc
{
    Task<List<Purchase_RequestVM>> GetAllPurchaseRequests();
    Task<Purchase_RequestVM> GetByIdPurchaseRequest(int id);
    Task<Response<Guid>> CreatePurchaseRequest(Purchase_RequestVM PurchaseRequest);
    Task<Response<Guid>> UpdatePurchaseRequest(int id, Purchase_RequestVM PurchaseRequest);
    Task<Response<Guid>> DeletePurchaseRequest(int id);
}
