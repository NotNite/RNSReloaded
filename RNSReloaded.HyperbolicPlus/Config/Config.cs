using System.ComponentModel;

namespace RNSReloaded.HyperbolicPlus.Config;

public class Config : Configurable<Config> {
    // ReSharper disable InconsistentNaming
    public enum Pattern {
        // NEST
        // solo
        Twili1_6S,
        Twili1_7S,

        // ARSENAL
        // solo
        Tassha_4S, // 4 cleaves
        Merran1_6S,

        // multi
        Tassha_1M, // untimed
        Tassha_3M,
        Tassha_4M, //  4cleaves
        Tassha_5M,
        Merran2_2M,
        Merran2_3M,
        Merran2_4M,
        Merran2_5M,
        Merran2_6M,
        Merran2_7M,
        Merran2_8M,
        Merran2_9M,

        // LIGHTHOUSE
        // solo
        // Illie2_S,
        // Karsi_7S,
        Ranalie1_2S,
        Ranalie1_3S,
        Ranalie1_4S,
        Ranalie1_5S,
        Ranalie1_6S,
        Ranalie1_7S,
        Ranalie1_8S,
        Ranalie2_4S, // Triple Lasers

        // multi
        Ranalie1_6M, // untimed
        Ranalie1_7M, // untimed
        Ranalie2_3M, // Knockback1
        Ranalie2_4M, // Triple Lasers
        Ranalie1_Mix_S,

        // STREETS
        // MegVaro_2S,
        Matti1_7S,
        Matti1_8S,

        // LAKESIDE
        Blush_6S,
        Blush_7S,

        Blush_2M,
        Blush_3M,
        Blush_4M,
        Blush_5M,
        Blush_6M,
        Blush_7M,

        Avy1_6S,
        Avy2_2S,
        Avy2_3S,
        Avy2_4S,
        Avy2_5S,
        Avy2_6S,
        Avy2_7S,
        Avy2_8S,
        Avy2_9S,
        Avy2_Mix_S,

        Avy1_4M,
        Avy1_6M,

        // KEEP
        // Harp_S,

        // PINNACLE
        Shira2_2S,
        Shira2_3S,
        Shira2_4S,
        Shira2_5S,
        Shira2_6S,
        Shira2_7S,
        Shira2_8S,
        Shira2_9S,
        Shira2_Mix_S,

        Shira2_5M,
        Shira2_6M,

        Bella1_S
    }

    [DisplayName("Battle Pattern")]
    [Description("The pattern to repeat")]
    [DefaultValue(Pattern.Tassha_1M)]
    public Pattern ActivePattern { get; set; } = Pattern.Tassha_1M;

    [DisplayName("Accelerate Speed")]
    [Description("Whether patterns gradually increase speed or not")]
    [DefaultValue(true)]

    public bool AccelerateSpeed { get; set; } = true;


    [DisplayName("No Invulns")]
    [Description("Prevents invulnerability")]
    [DefaultValue(true)]

    public bool PreventInvulns { get; set; } = true;

    [DisplayName("Permadeath")]
    [Description("Prevents revives")]
    [DefaultValue(true)]

    public bool Permadeath { get; set; } = true;
}
