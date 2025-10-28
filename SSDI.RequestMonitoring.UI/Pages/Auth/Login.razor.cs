using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Alerts;
using SSDI.RequestMonitoring.UI.Models.Auth;

namespace SSDI.RequestMonitoring.UI.Pages.Auth;

public partial class Login : ComponentBase
{
    private LoginVM _loginModel = new();
    private bool _isNewRequestModalFormVisible = false;
    private CustomToast _toast = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("initializeTypingAnimation");
        }
    }

    private void ToggleNewRequest()
    {
        _isNewRequestModalFormVisible = !_isNewRequestModalFormVisible;
    }

    private void OnAddRequestModalClose() => _isNewRequestModalFormVisible = false;

    private async Task OnAddRequestModalSave()
    {
        _isNewRequestModalFormVisible = !_isNewRequestModalFormVisible;
        await _toast.ShowToast("Success", "Saved!", "The request has been added successfully.", true);
    }

    private async Task HandleLogin()
    {
        if (await authenticationSvc.AuthenticateAsync(_loginModel.Username, _loginModel.Password))
        {
            var uri = new Uri(navigationManager.Uri);
            var statusFilterString = uri.Query.Split("=%2F").LastOrDefault();
            navigationManager.NavigateTo(string.IsNullOrEmpty(statusFilterString) ? "/dashboard/" : $"/{statusFilterString.Replace("%2F", "/")}");
            return;
        }

        await _toast.ShowToast("Error", "Invalid Credentials!", "Username/password combination unknown.", true);
    }
}