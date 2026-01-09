using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SSDI.RequestMonitoring.UI.Contracts;
using SSDI.RequestMonitoring.UI.Providers;
using SSDI.RequestMonitoring.UI.Services.Base;

namespace SSDI.RequestMonitoring.UI.Services
{
    public class AuthenticationSvc : BaseHttpService, IAuthenticationSvc
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthenticationSvc(IClient client, ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager) : base(client, localStorage, navigationManager)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            try
            {
                AuthRequest authenticationRequest = new() { Username = username, Password = password };
                var authenticationResponse = await _client.LoginAsync(authenticationRequest);
                if (authenticationResponse.Token != string.Empty)
                {
                    await _localStorage.SetItemAsync("rmtoken", authenticationResponse.Token);

                    // Set claims in Blazor and login state
                    await((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedIn();
                    return "success";
                }
                return "error";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("No Access"))
                {
                    return "NoAccess";
                }
                if (ex.Message.Contains("Access Denied"))
                {
                    return "DeniedAccess";
                }

                return ex.Message;
            }
        }

        public async Task Logout()
        {
            await((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedOut();
        }
    }
}
