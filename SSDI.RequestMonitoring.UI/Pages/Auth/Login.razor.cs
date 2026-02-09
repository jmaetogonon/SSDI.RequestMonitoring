using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.Models.Auth;
using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Pages.Auth;

public partial class Login : ComponentBase
{
    private readonly LoginVM _loginModel = new();
    private bool _isNewRequestModalFormVisible = false;
    private bool _isNewJobOrderModalFormVisible = false;

    private List<DivisionVM> _divisions = [];
    private List<DepartmentVM> _departments = [];

    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;
    private AlertType AlertType { get; set; } = AlertType.warning;

    protected override async Task OnInitializedAsync()
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        if (bool.TryParse(query["tokenExpired"], out var expired) && expired)
        {
            IsShowAlert = true;
            AlertMessage = "Your session has expired. Please log in again to continue.";
        }

        _divisions = await divisionSvc.GetAllDivisions();

        if (_divisions == null)
        {
            toastSvc.ShowWarning("Request timeout. Please try again.");
            return;
        }

        _departments = await departmentSvc.GetAllDepartments();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await jsRuntime.InvokeVoidAsync("initializeTypingAnimation");
        }
    }

    private void ToggleNewPurchaseRequest()
    {
        _isNewRequestModalFormVisible = !_isNewRequestModalFormVisible;
    }

    private void ToggleNewJobOrderRequest()
    {
        _isNewJobOrderModalFormVisible = !_isNewJobOrderModalFormVisible;
    }

    private void OnAddRequestModalClose() => _isNewRequestModalFormVisible = false;

    private void OnAddRequestModalSave()
    {
        toastSvc.ShowSuccess("The request has been added successfully.");
        _isNewRequestModalFormVisible = !_isNewRequestModalFormVisible;
    }

    private void OnAddJobOrderModalClose() => _isNewJobOrderModalFormVisible = false;

    private void OnAddJobOrderModalSave()
    {
        toastSvc.ShowSuccess("The request has been added successfully.");
        _isNewJobOrderModalFormVisible = !_isNewJobOrderModalFormVisible;
    }

    private async Task HandleLogin()
    {
        var uri = new Uri(navigationManager.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var returnUrl = query["returnUrl"];

        var result = await authenticationSvc.AuthenticateAsync(_loginModel.Username, _loginModel.Password);
        if (result == "success")
        {
            string? decodedReturnUrl = null;
            if (!string.IsNullOrEmpty(returnUrl)) decodedReturnUrl = Uri.UnescapeDataString(returnUrl);

            navigationManager.NavigateTo(string.IsNullOrEmpty(decodedReturnUrl) ? "/dashboard" : decodedReturnUrl);
            return;
        }

        if (result == "NoAccess")
        {
            IsShowAlert = true;
            AlertMessage = "Access not configured. Please contact your administrator.";
            AlertType = AlertType.error;
            return;
        }

        if (result == "DeniedAccess")
        {
            IsShowAlert = true;
            AlertMessage = "You don't have permission to access this application.";
            AlertType = AlertType.error;
            return;
        }

        IsShowAlert = true;
        AlertMessage = "Invalid username or password.";
        AlertType = AlertType.error;
    }
}