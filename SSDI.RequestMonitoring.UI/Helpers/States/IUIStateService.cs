namespace SSDI.RequestMonitoring.UI.Helpers.States;

public interface IUIStateService
{
    event Action OnChange;

    bool IsStriped { get; set; }
    int PageSize { get; set; }

    Task LoadPreferencesAsync();
}
