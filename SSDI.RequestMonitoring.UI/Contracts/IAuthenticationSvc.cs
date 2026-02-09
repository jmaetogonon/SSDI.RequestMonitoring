namespace SSDI.RequestMonitoring.UI.Contracts;

public interface IAuthenticationSvc
{
    Task<string> AuthenticateAsync(string username, string password);

    Task Logout();
}