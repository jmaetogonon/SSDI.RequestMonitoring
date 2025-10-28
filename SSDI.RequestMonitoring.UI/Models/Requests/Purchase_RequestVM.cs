using System.ComponentModel.DataAnnotations;

namespace SSDI.RequestMonitoring.UI.Models.Requests;

public class Purchase_RequestVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Division_Department { get; set; } = string.Empty;
    public string Nature_Of_Request { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    [Required]
    public string Priority { get; set; } = string.Empty;
    public string OtherPriority { get; set; } = string.Empty;
    public DateTime? DateRequested { get; set; }
    public DateTime? DateAdminNotified { get; set; }
}
