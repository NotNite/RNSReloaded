using System.ComponentModel;

namespace RNSReloaded.DoubleTime.Config;

public class Config : Configurable<Config> {
    // ReSharper disable InconsistentNaming


    [DisplayName("Speed")]
    [Description("Speed multipier")]
    [DefaultValue(1.5)]
    public double SpeedMultiplier { get; set; } = 1.5;
}
