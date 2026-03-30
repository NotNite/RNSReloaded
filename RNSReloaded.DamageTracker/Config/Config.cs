using System.ComponentModel;

namespace RNSReloaded.DamageTracker.Config;

public class Config : Configurable<Config> {
    [DisplayName("Graphical Display")]
    [DefaultValue(true)]
    public bool ImGuiDisplay { get; set; } = true;

    [DisplayName("Buff Display")]
    [DefaultValue(true)]
    public bool ImGuiBuffDisplay { get; set; } = true;

    [DisplayName("Write Logs to File")]
    [DefaultValue(false)]
    public bool WriteLogs { get; set; } = false;
}
