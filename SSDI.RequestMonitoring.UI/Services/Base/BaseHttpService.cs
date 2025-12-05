using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

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
                _navigationManager.NavigateTo($"/?returnUrl={encodedReturnUrl}&tokenExpired=true", forceLoad: true);
                return default!;
            }
            return default!;
        }
        catch (HttpRequestException httpEx)
        {
            // ⛔ This catches "Failed to fetch"
            Console.WriteLine($"NETWORK ERROR: {httpEx.Message}");
            return default!;
        }
        catch (TaskCanceledException)
        {
            // ⛔ Timeout
            Console.WriteLine("HTTP REQUEST TIMEOUT");

            return default!;
        }
        catch (Exception ex)
        {
            // 🔥 Unexpected crash protection
            Console.WriteLine($"UNEXPECTED ERROR in SafeApiCall: {ex}");

            return default!;
        }
    }

    protected Response<Guid> ConvertApiExceptions<Guid>(ApiException ex)
    {
        return ex.StatusCode switch
        {
            400 => new Response<Guid> { Message = "Invalid data was submitted", ValidationErrors = ex.Response, Success = false },
            401 => new Response<Guid> { Message = "Unauthorized - please log in again.", Success = false },
            404 => new Response<Guid> { Message = "The record was not found", Success = false },
            500 => new Response<Guid> { Message = "Server error occurred. Try again later.", Success = false },
            _ => new Response<Guid> { Message = "Something went wrong. Please try again later.", Success = false }
        };
    }
}