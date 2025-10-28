namespace SSDI.RequestMonitoring.UI.Contracts
{
    public interface IAuthenticationSvc
    {
        Task<bool> AuthenticateAsync(string username, string password);
        Task Logout();
    }
}
