using System.ComponentModel;
using Reloaded.Mod.Interfaces;

namespace RNSReloaded.PlayerColorChanger.Config;

public class Config : Configurable<Config> {
    [DisplayName("Player Color")]
    [Description("The hex code of the color you want to use, without a hash.")]
    [DefaultValue("FFFFFF")]
    public string String { get; set; } = "FFFFFF";
}
