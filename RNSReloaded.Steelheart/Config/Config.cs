using System.ComponentModel;

namespace RNSReloaded.Steelheart.Config;

public class Config : Configurable<Config> {
    [DisplayName("Force Enrage")]
    [DefaultValue(false)]
    public bool ForceEnrage { get; set; } = true;

    [DisplayName("Disable Invuln")]
    [DefaultValue(false)]
    public bool DisableInvuln { get; set; } = true;
}
