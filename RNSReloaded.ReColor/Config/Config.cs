using System.ComponentModel;

namespace RNSReloaded.ReColor.Config;

public class Config : Configurable<Config> {
    // ReSharper disable InconsistentNaming

    [DisplayName("Purple Recolor")]
    [Description("Hex Value for purple rings")]
    [DefaultValue(0xFF00FF)]
    public int PurpleColor { get; set; } = 0xFF00FF;

    [DisplayName("Blue Recolor")]
    [Description("Hex Value for blue rings")]
    [DefaultValue(0xFF0000)]
    public int BlueColor { get; set; } = 0xFF0000;

    [DisplayName("Red Recolor")]
    [Description("Hex Value for red rings")]
    [DefaultValue(0x0000FF)]
    public int RedColor { get; set; } = 0x0000FF;

    [DisplayName("Yellow Recolor")]
    [Description("Hex Value for yellow rings")]
    [DefaultValue(0x00FFFF)]
    public int YellowColor { get; set; } = 0x00FFFF;

    [DisplayName("Green Recolor")]
    [Description("Hex Value for red rings")]
    [DefaultValue(0x00FF00)]
    public int GreenColor { get; set; } = 0x00FF00;
    
}
