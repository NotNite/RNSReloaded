using System.ComponentModel;

namespace RNSReloaded.BigFish.Config;

public class Config : Configurable<Config> {
    [DisplayName("Fish Scaling")]
    [DefaultValue(1)]
    public float FishScale { get; set; } = 1;
}
