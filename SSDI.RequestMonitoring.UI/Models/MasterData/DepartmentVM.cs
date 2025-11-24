namespace SSDI.RequestMonitoring.UI.Models.MasterData;

public class DepartmentVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DivisionVM? Division { get; set; }
    public int DivisionId { get; set; }
}