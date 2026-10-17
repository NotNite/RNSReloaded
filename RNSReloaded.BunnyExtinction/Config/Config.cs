using System.ComponentModel;

namespace RNSReloaded.BunnyExtinction.Config;

public class Config : Configurable<Config> {
    [DisplayName("Infernal BBQ")]
    [Description("Enables extra challenges. For players seeking extreme, merciless thrills. Or masochists")]
    [DefaultValue(false)]
    public bool InfernalBBQ { get; set; } = true;

    [DisplayName("Max Health 1")]
    [Description("Restricts max health to 1 hp")]
    [DefaultValue(false)]
    public bool LimitHealth { get; set; } = false;

    [DisplayName("Max Speed -2")]
    [Description("Restricts max movement speed to a base of -2")]
    [DefaultValue(false)]
    public bool LimitSpeed { get; set; } = false;

    [DisplayName("Disable Bullet Deletion")]
    [Description("Removes the capability for abilities and loot to delete bullets")]
    [DefaultValue(false)]
    public bool DisableBulletDeletion { get; set; } = false;

    [DisplayName("Permadeath")]
    [Description("Prevents players from reviving in multiplayer")]
    [DefaultValue(false)]
    public bool Permadeath { get; set; } = false;

    [DisplayName("Disable Invulnerability")]
    [Description("Prevents invulnerability from abilities/loot")]
    [DefaultValue(false)]
    public bool DisableInvuln { get; set; } = false;

    [DisplayName("Set Hitbox Size")]
    [Description("Fixes player hitbox to default size")]
    [DefaultValue(false)]
    public bool SetHitbox { get; set; } = false;

    [DisplayName("Steelheart Compatibility")]
    [Description("Reenables Shira's third Steel Yourself when Steelheart is on. Do not use without Steelheart")]
    [DefaultValue(false)]
    public bool Steelheart { get; set; } = false;
}
