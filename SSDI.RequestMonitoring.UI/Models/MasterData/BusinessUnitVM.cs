namespace SSDI.RequestMonitoring.UI.Models.MasterData;

public class BusinessUnitVM
{
    public int Id { get; set; }
    public string BU_Code { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    public string BU_Desc { get; set; } = string.Empty;

    public int StatusId { get; set; } = 1;

    public bool IsISR { get; set; }

    public string GroupNo { get; set; } = string.Empty;
}