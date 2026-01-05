namespace SSDI.RequestMonitoring.UI.Pages.Dashboard.Dto;

public class ActivityItem
{
    public int Id { get; set; }
    public string RequestType { get; set; } = ""; // "purchase" or "joborder"
    public string Title { get; set; } = "";
    public RequestStatus Status { get; set; }
    public RequestPriority Priority { get; set; }
    public DateTime Date { get; set; }
    public DateTime? PendingSince { get; set; }
}