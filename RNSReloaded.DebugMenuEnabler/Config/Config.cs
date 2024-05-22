using System.ComponentModel;

namespace RNSReloaded.DebugMenuEnabler.Config;

public class Config : Configurable<Config> {
    [DisplayName("Show Upgrade Menu")]
    [DefaultValue(false)]
    public bool Upgrade { get; set; } = false;

    [DisplayName("Show Encounters Menu")]
    [DefaultValue(false)]
    public bool Enc { get; set; } = false;

    [DisplayName("Show Dialog Menu")]
    [DefaultValue(false)]
    public bool Dialog { get; set; } = false;

    [DisplayName("Show Item Menu")]
    [DefaultValue(false)]
    public bool Item { get; set; } = false;

    [DisplayName("Show Stats Menu")]
    [DefaultValue(false)]
    public bool Stat { get; set; } = false;
}
