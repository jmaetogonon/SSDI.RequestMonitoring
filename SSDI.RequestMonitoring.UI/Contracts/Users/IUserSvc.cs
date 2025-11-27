using SSDI.RequestMonitoring.UI.Models.Users;

namespace SSDI.RequestMonitoring.UI.Contracts.Users;

public interface IUserSvc
{
    Task<List<SupervisorVM>> GetSupervisors();
}