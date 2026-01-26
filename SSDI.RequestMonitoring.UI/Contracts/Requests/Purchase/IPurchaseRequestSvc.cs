using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Contracts.Requests.Purchase;

public interface IPurchaseRequestSvc
{
    Task<List<Purchase_RequestVM>> GetAllPurchaseRequests();

    Task<List<Purchase_RequestVM>> GetAllPurchaseRequestsByUser(int userId);

    Task<List<Purchase_RequestVM>> GetAllPurchaseRequestsByAdmin();

    Task<List<Purchase_RequestVM>> GetAllPurchaseReqByCeo();

    Task<List<Purchase_RequestVM>> GetAllPurchaseReqBySupervisor(int supervisorId, bool includeDepartmentMembers = true, bool includeDivisionMembers = true);

    Task<Purchase_RequestVM> GetByIdPurchaseRequest(int id);

    Task<Response<Guid>> CreatePurchaseRequest(Purchase_RequestVM PurchaseRequest);

    Task<Response<Guid>> UpdatePurchaseRequest(int id, Purchase_RequestVM PurchaseRequest);

    Task<Response<Guid>> DeletePurchaseRequest(int id);

    Task<Response<Guid>> ApprovePurchaseRequest(ApprovePurchaseRequestCommandVM command);

    Task<byte[]> GeneratePurchaseRequestPdf(int id, bool isWithAttach);

    Task<Response<Guid>> InitiateClosePurchaseRequest(int id, int initiatorId);

    Task<Response<Guid>> ConfirmClosePurchaseRequest(int id, int userId);

    Task<Response<Guid>> CancelPendingClosePurchaseRequest(int id, int userId);
}