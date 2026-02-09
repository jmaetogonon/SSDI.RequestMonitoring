using Blazored.LocalStorage;

namespace SSDI.RequestMonitoring.UI.Helpers.States;

public class UIStateService(ILocalStorageService localStorage) : IUIStateService
{
    private const string _storageKey = "uiPreferences";
    private readonly ILocalStorageService _localStorage = localStorage;

    public event Action? OnChange;

    private bool _isStriped = false;
    private int _pageSize = 10;
    private bool _isLoaded = false;

    public bool IsStriped
    {
        get => _isStriped;
        set
        {
            if (_isStriped != value)
            {
                _isStriped = value;
                NotifyStateChanged();
                _ = SavePreferencesAsync();
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize != value && IsValidPageSize(value))
            {
                _pageSize = value;
                NotifyStateChanged();
                _ = SavePreferencesAsync();
            }
        }
    }

    public async Task LoadPreferencesAsync()
    {
        if (_isLoaded) return;

        try
        {
            var preferences = await _localStorage.GetItemAsync<UIPreferences>(_storageKey);
            if (preferences != null)
            {
                _isStriped = preferences.IsStriped;
                _pageSize = IsValidPageSize(preferences.PageSize) ? preferences.PageSize : 10;
            }
            _isLoaded = true;
            NotifyStateChanged();
        }
        catch
        {
            // Use default values if loading fails
            _isLoaded = true;
        }
    }

    private async Task SavePreferencesAsync()
    {
        try
        {
            var preferences = new UIPreferences
            {
                IsStriped = _isStriped,
                PageSize = _pageSize
            };
            await _localStorage.SetItemAsync(_storageKey, preferences);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving UI preferences: {ex.Message}");
        }
    }

    private static bool IsValidPageSize(int pageSize)
    {
        // Define valid page sizes
        int[] validPageSizes = [10, 25, 50];
        return validPageSizes.Contains(pageSize);
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

public class UIPreferences
{
    public bool IsStriped { get; set; }
    public int PageSize { get; set; } = 10;
}