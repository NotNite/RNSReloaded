using System.ComponentModel;

namespace RNSReloaded.RabbitSeed.Config;

public class Config : Configurable<Config> {
    [DisplayName("Seed")]
    [Description("The seed that should be used for generating runs")]
    [DefaultValue(0)]
    public long mapSeed { get; set; } = 0;

    [DisplayName("Turn on set seed")]
    [Description("Limits online play if turned on")]
    [DefaultValue(true)]
    public bool shouldSetSeed { get; set; } = true;

    [DisplayName("Send chat message with seed")]
    [Description("Sends a chat message on the first fight with the current map seed")]
    [DefaultValue(true)]
    public bool shouldChatSeed { get; set; } = true;
}
