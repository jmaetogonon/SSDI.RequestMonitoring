using System.ComponentModel.DataAnnotations;

namespace SSDI.RequestMonitoring.UI.Models.DTO;

public class UserSettingsModel
{
    [Display(Name = "Language")]
    public string Language { get; set; } = "en";

    [Display(Name = "Time Zone")]
    public string TimeZone { get; set; } = "UTC";

    [Display(Name = "Enable Sound Alerts")]
    public bool EnableSoundAlerts { get; set; } = true;

    [Display(Name = "Auto-logout Time")]
    public string AutoLogoutTime { get; set; } = "30";

    // E-Signature settings (if needed)
    public ESignatureSettings ESignature { get; set; } = new();

    // Method to reset to defaults
    public void ResetToDefaults()
    {
        Language = "en";
        TimeZone = "UTC";
        EnableSoundAlerts = true;
        AutoLogoutTime = "30";
        ESignature = new ESignatureSettings();
    }
}

public class ESignatureSettings
{
    // Add e-signature specific properties here
    public string SignaturePath { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
}