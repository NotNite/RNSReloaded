using System.ComponentModel;

namespace RNSReloaded.BunnyExtinction.Config;

public class Config : Configurable<Config> {
    [DisplayName("Infernal BBQ")]
    [Description("Enables extra challenges. For players seeking extreme, merciless thrills. Or masochists")]
    [DefaultValue(false)]
    public bool InfernalBBQ { get; set; } = false;
}
