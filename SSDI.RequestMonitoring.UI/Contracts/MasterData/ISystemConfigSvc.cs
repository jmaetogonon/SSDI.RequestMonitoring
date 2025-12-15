using SSDI.RequestMonitoring.UI.Models.MasterData;

namespace SSDI.RequestMonitoring.UI.Contracts.MasterData;

public interface ISystemConfigSvc
{
    Task<SystemConfigVM> GetSystemConfigAsync();
}
