using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;

namespace SSDI.RequestMonitoring.UI.Services.Base;

public class BaseHttpService
{
    protected IClient _client;
    protected readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;

    public BaseHttpService(IClient client, ILocalStorageService localStorage, NavigationManager navigationManager)
    {
        _client = client;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
    }

    protected async Task<T> SafeApiCall<T>(Func<Task<T>> apiCall)
    {
        try
        {
            return await apiCall();
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
            {
                await _localStorage.RemoveItemAsync("rmtoken"); // clear invalid token
                var currentUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
                var encodedReturnUrl = Uri.EscapeDataString("/" + currentUrl);
                _navigationManager.NavigateTo($"/login?returnUrl={encodedReturnUrl}&tokenExpired=true", forceLoad: true);
                return default!;
            }
            return default!;
        }
    }

    protected Response<Guid> ConvertApiExceptions<Guid>(ApiException ex)
    {
        if (ex.StatusCode == 400)
        {
            return new Response<Guid>() { Message = "Invalid data was submitted", ValidationErrors = ex.Response, Success = false };
        }
        else if (ex.StatusCode == 401)
        {
            return new Response<Guid>() { Message = "Unauthorized - please log in again.", Success = false };
        }
        else if (ex.StatusCode == 404)
        {
            return new Response<Guid>() { Message = "The record was not found", Success = false };
        }
        else if (ex.StatusCode == 500)
        {
            return new Response<Guid>() { Message = "An unexpected error occurred on the server. Please try again later.", Success = false };
        }
        else
        {
            return new Response<Guid>() { Message = "Something went wrong, please try again later.", Success = false };
        }
    }

    protected async Task AddBearerToken()
    {
        if (await _localStorage.ContainKeyAsync("token"))
            _client.HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _localStorage.GetItemAsync<string>("token"));
    }
}