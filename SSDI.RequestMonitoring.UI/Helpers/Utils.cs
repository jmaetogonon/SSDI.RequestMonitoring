namespace SSDI.RequestMonitoring.UI.Helpers;

public static class Utils
{
    public static string GenerateGuid() => Guid.NewGuid().ToString();

    public static string FormatDate(DateTime date) => date.ToString("yyyy-MM-dd");
    public static void WriteLine(string text) => Console.WriteLine(text);

    public static async Task DelayAsync(int milliseconds) => await Task.Delay(milliseconds);
}