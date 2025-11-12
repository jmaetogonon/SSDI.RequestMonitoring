using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.Models.Auth;

namespace SSDI.RequestMonitoring.UI.Pages.Auth;

public partial class Login : ComponentBase
{
    private LoginVM _loginModel = new();
    private bool _isNewRequestModalFormVisible = false;

    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;
    private AlertType AlertType { get; set; } = AlertType.warning;

    protected override void OnInitialized()
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        if (bool.TryParse(query["tokenExpired"], out var expired) && expired)
        {
            IsShowAlert = true; 
            AlertMessage = "Your session has expired. Please log in again to continue.";
        }
    }

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

    private void OnAddRequestModalSave()
    {
        _isNewRequestModalFormVisible = !_isNewRequestModalFormVisible;
        toastSvc.ShowSuccess("The request has been added successfully.");
    }

    private async Task HandleLogin()
    {
        var uri = new Uri(navigationManager.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var returnUrl = query["returnUrl"];
        var tokenExpired = query["tokenExpired"];

        if (await authenticationSvc.AuthenticateAsync(_loginModel.Username, _loginModel.Password))
        {
            string? decodedReturnUrl = null;
            if (!string.IsNullOrEmpty(returnUrl)) decodedReturnUrl = Uri.UnescapeDataString(returnUrl);

            navigationManager.NavigateTo(string.IsNullOrEmpty(decodedReturnUrl) ? "/dashboard" : decodedReturnUrl);
            return;
        }

        IsShowAlert = true;
        AlertMessage = "Username/password combination unknown.";
        AlertType = AlertType.error;
    }
}