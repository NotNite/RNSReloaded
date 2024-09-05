using System.ComponentModel;

namespace RNSReloaded.SoloTogether.Config;

public class Config : Configurable<Config> {
    [DisplayName("Adorable")]
    [Description("Play without a significant challenge. Permadeath is disabled")]
    [DefaultValue(false)]
    public bool Adorable { get; set; } = false;
}
