using System.ComponentModel;

namespace RNSReloaded.Nightcore.Config;

public class Config : Configurable<Config> {
    // ReSharper disable InconsistentNaming


    [DisplayName("Music speed multiplier")]
    [Description("Music speed = (Gamespeed - 1) * SpeedMultiplier + 1 + Shift. Example/Sane values. between 0 and 1")]
    [DefaultValue(0.5)]


    public double SpeedMultiplier { get; set; } = 0.5;



    [DisplayName("Shift the music speed")]
    [Description("So it can be slower outside battle and normal during. Example/Sane value: -0.2 to -0.5")]
    [DefaultValue(0)]

    public double SpeedShift { get; set; } = 0;
}
