using System.ComponentModel.DataAnnotations; 

namespace SSDI.RequestMonitoring.UI.Models.Auth;

public class LoginVM
{
    [Required]
    [EmailAddress]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;


}
