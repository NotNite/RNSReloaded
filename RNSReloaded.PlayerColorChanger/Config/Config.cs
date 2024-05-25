using System.ComponentModel;

namespace RNSReloaded.PlayerColorChanger.Config;

public class Config : Configurable<Config> {
    [DisplayName("Player Color")]
    [Description("The hex code of the primary color you want to use, without a hash. (Ring, health bar, etc.)")]
    [DefaultValue("FFFFFF")]
    public string String { get; set; } = "FFFFFF";

    [DisplayName("Dark Player Color")]
    [Description("The hex code of the dark color you want to use, without a hash. (Unknown use)")]
    [DefaultValue("FFFFFF")]
    public string DarkString { get; set; } = "FFFFFF";

    [DisplayName("Base Player Color")]
    [Description("The hex code of the base color you want to use, without a hash. (Cursor color, etc.)")]
    [DefaultValue("FFFFFF")]
    public string BaseString { get; set; } = "FFFFFF";

    [DisplayName("Saturated Player Color")]
    [Description("The hex code of the saturated color you want to use, without a hash. (Unknown use)")]
    [DefaultValue("FFFFFF")]
    public string SatString { get; set; } = "FFFFFF";
}
