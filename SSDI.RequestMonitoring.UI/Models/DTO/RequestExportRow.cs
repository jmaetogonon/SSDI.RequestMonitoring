namespace SSDI.RequestMonitoring.UI.Models.DTO;

public class RequestExportRow
{
    public string SeriesNo { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string NatureOfRequest { get; set; } = string.Empty;
    public string DivisionDepartment { get; set; } = string.Empty;
    public string BusinessUnit { get; set; } = string.Empty; // NEW
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? TotalAmount { get; set; } // NEW (nullable for requests without amount)
    public DateTime? DateRequested { get; set; }
}