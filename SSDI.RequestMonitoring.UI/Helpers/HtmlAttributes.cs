namespace SSDI.RequestMonitoring.UI.Helpers;

public static class HtmlAttributes
{
    // Cached dictionaries for the entire app
    private static readonly IReadOnlyDictionary<string, object> _disabled =
        new Dictionary<string, object> { ["disabled"] = "disabled" };

    private static readonly IReadOnlyDictionary<string, object> _readOnly =
        new Dictionary<string, object> { ["readonly"] = "readonly" };

    private static readonly IReadOnlyDictionary<string, object> _hidden =
        new Dictionary<string, object> { ["hidden"] = "hidden" };

    private static readonly IReadOnlyDictionary<string, object> _disabledAndHidden =
        new Dictionary<string, object>
        {
            ["disabled"] = "disabled",
            ["hidden"] = "hidden"
        };

    private static readonly IReadOnlyDictionary<string, object> _disabledOrReadOnly =
        new Dictionary<string, object>
        {
            ["disabled"] = "disabled",
            ["readonly"] = "readonly"
        };

    /// <summary>
    /// Returns a disabled attribute if condition is true, otherwise null.
    /// </summary>
    public static IReadOnlyDictionary<string, object>? DisabledIf(bool condition)
        => condition ? _disabled : null;

    /// <summary>
    /// Returns a readonly attribute if condition is true, otherwise null.
    /// </summary>
    public static IReadOnlyDictionary<string, object>? ReadOnlyIf(bool condition)
        => condition ? _readOnly : null;

    /// <summary>
    /// Returns a hidden attribute if condition is true, otherwise null.
    /// </summary>
    public static IReadOnlyDictionary<string, object>? HiddenIf(bool condition)
        => condition ? _hidden : null;

    /// <summary>
    /// Returns both disabled and hidden attributes if condition is true, otherwise null.
    /// </summary>
    public static IReadOnlyDictionary<string, object>? DisabledAndHiddenIf(bool condition)
        => condition ? _disabledAndHidden : null;

    /// <summary>
    /// Returns both disabled and readonly attributes if condition is true, otherwise null.
    /// </summary>
    public static IReadOnlyDictionary<string, object>? DisabledOrReadOnlyIf(bool condition)
        => condition ? _disabledOrReadOnly : null;
}
