using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SSDI.RequestMonitoring.UI.Helpers.States;

public class CurrentUser : IDisposable
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private bool _initialized;

    public event Action? OnUserChanged;

    public int UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public string RoleDesc { get; private set; } = string.Empty;
    public int DepartmentHeadId { get; private set; }
    public int DivisionHeadId { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public bool IsUser { get; private set; }
    public bool IsAdmin { get; private set; }
    public bool IsSupervisor { get; private set; }
    public bool IsCEO { get; private set; }

    public CurrentUser(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;

        // 🔥 Automatically reload user when auth state changes
        _authenticationStateProvider.AuthenticationStateChanged += AuthStateChangedHandler;
    }

    private async void AuthStateChangedHandler(Task<AuthenticationState> task)
    {
        await InitializeAsync();
        OnUserChanged?.Invoke(); // notify subscribers (like Navbar) to re-render
    }

    public async Task InitializeAsync()
    {
        if (_initialized) _initialized = false; // allow reload every time state changes

        var authState = await _authenticationStateProvider!.GetAuthenticationStateAsync();
        var user = authState?.User;

        if (user?.Identity is { IsAuthenticated: true })
        {
            var idString = user.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
            var deptHead = user.Claims.FirstOrDefault(x => x.Type == "departmentHeadId")?.Value;
            var divHead = user.Claims.FirstOrDefault(x => x.Type == "divisionHeadId")?.Value;

            UserId = int.TryParse(idString, out var parsedId) ? parsedId : 0;
            Username = user.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)!.Value;
            FullName = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)!.Value;
            Role = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)!.Value;
            RoleDesc = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
            DepartmentHeadId = int.TryParse(deptHead, out var parseddeptHeadId) ? parseddeptHeadId : 0;
            DivisionHeadId = int.TryParse(divHead, out var parseddivHeadId) ? parseddivHeadId : 0;
            IsAuthenticated = true;

            IsUser = Role == TokenCons.Role__User;
            IsAdmin = Role == TokenCons.Role__Admin;
            IsCEO = Username == "b.palang" || Username == "c.gagui";
            IsSupervisor = (RoleDesc.Contains("Supervisor", StringComparison.OrdinalIgnoreCase) ||
                       RoleDesc.Contains("Admin", StringComparison.OrdinalIgnoreCase) ||
                       RoleDesc.Contains("Lead", StringComparison.OrdinalIgnoreCase) ||
                       RoleDesc.Contains("Head", StringComparison.OrdinalIgnoreCase) ||
                       RoleDesc.Contains("Manager", StringComparison.OrdinalIgnoreCase)) && RoleDesc != "System Administrator";
        }
        else
        {
            Username = string.Empty;
            FullName = string.Empty;
            UserId = 0;
            IsAuthenticated = false;
        }
    }

    public void Dispose()
    {
        _authenticationStateProvider.AuthenticationStateChanged -= AuthStateChangedHandler;
    }
}