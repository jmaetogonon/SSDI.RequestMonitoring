namespace SSDI.RequestMonitoring.UI.Models.Users;

public class UserVM
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = "p@ssw0rd";
    public string Role { get; set; } = string.Empty;
    public string RoleDesc { get; set; } = string.Empty;
    public string IsHasAppAccess { get; set; } = string.Empty;
    public int? DepartmentHeadId { get; set; }
    public string DepartmentHeadName { get; set; } = string.Empty;
    public int? DivisionHeadId { get; set; }
    public string DivisionHeadName { get; set; } = string.Empty;
}
