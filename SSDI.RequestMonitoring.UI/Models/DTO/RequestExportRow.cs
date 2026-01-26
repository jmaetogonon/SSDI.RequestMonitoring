namespace SSDI.RequestMonitoring.UI.Models.DTO;

public class RequestExportRow
{
    public string SeriesNo { get; set; } = "";
    public string RequestedBy { get; set; } = "";
    public string NatureOfRequest { get; set; } = "";
    public string DivisionDepartment { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Status { get; set; } = "";
    public string DateRequested { get; set; } = "";
}