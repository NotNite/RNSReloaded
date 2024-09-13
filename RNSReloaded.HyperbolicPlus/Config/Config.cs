using System.ComponentModel;

namespace RNSReloaded.HyperbolicPlus.Config;

public class Config : Configurable<Config> {
    // ReSharper disable InconsistentNaming
    public enum Pattern {
        // tutorial
        /*Bella1,
        Rem1,
        Sohko1,
        Sohko1_S,
        PinElta1,
        Maxi1,
        Maxi1_S,*/

        // outskirts
        Bella2_S,
        Bella3_S,
        Rem2_S,
        Rem3_S,
        Sohko2_S,
        Sohko3_S,
        PinElta2_S,
        PinElta3_S,
        Maxi2_S,
        Maxi3_S,

        Bella2_M,
        Bella3_M,
        Rem2_M,
        Rem3_M,
        Sohko2_M,
        Sohko3_M,
        PinElta2_M,
        PinElta3_M,
        Maxi2_M,
        Maxi3_M,

        // nest
        Menna1_S,
        Menna2_S,
        Azel1_S,
        Azel2_S,
        Menna1_M,
        Menna2_M,
        Azel1_M,
        Azel2_M,

        Saya_1S,
        Saya_2S,
        Saya_3S,
        Saya_4S,
        Saya_5S,
        Saya_6S,

        Saya_1M,
        Saya_2M,
        Saya_3M,
        Saya_4M,
        Saya_5M,
        Saya_6M,

        Twili1_1S,
        Twili1_2S,
        Twili1_3S,
        Twili1_4S,
        Twili1_5S,
        Twili1_6S,

        Twili1_1M,
        Twili1_2M,
        Twili1_3M,
        Twili1_4M,
        Twili1_5M,
        Twili1_6M,

        Twili2_1S,
        Twili2_2S,
        Twili2_3S,
        Twili2_4S,
        Twili2_5S,
        Twili2_6S,
        Twili2_7S,
        Twili2_8S,

        Twili2_1M,
        Twili2_2M,
        Twili2_3M,
        Twili2_4M,
        Twili2_5M,
        Twili2_6M,
        Twili2_7M,
        Twili2_8M,

        // arsenal
        Mink1_S,
        Mink2_S,
        RanXin1_S,
        RanXin2_S,

        Mink1_M,
        Mink2_M,
        RanXin1_M,
        RanXin2_M,

        Tassha_1S,
        Tassha_2S,
        Tassha_3S,
        Tassha_4S,
        Tassha_5S,
        Tassha_6S,

        Tassha_1M,
        Tassha_2M,
        Tassha_3M,
        Tassha_4M,
        Tassha_5M,
        Tassha_6M,

        Merran1_1S,
        Merran1_2S,
        Merran1_3S,
        Merran1_4S,
        Merran1_5S,
        Merran1_6S,
        Merran1_7S,

        Merran1_1M,
        Merran1_2M,
        Merran1_3M,
        Merran1_4M,
        Merran1_5M,
        Merran1_6M,
        Merran1_7M,

        Merran2_1S,
        Merran2_2S,
        Merran2_3S,
        Merran2_4S,
        Merran2_5S,
        Merran2_6S,
        Merran2_7S,
        Merran2_8S,

        Merran2_1M,
        Merran2_2M,
        Merran2_3M,
        Merran2_4M,
        Merran2_5M,
        Merran2_6M,
        Merran2_7M,
        Merran2_8M,

        // lighthouse
        PiKuu1_S,
        PiKuu2_S,
        Illie1_S,
        Illie2_S,

        PiKuu1_M,
        PiKuu2_M,
        Illie1_M,
        Illie2_M,

        Karsi_1S,
        Karsi_2S,
        Karsi_3S,
        Karsi_4S,
        Karsi_5S,
        Karsi_6S,

        Karsi_1M,
        Karsi_2M,
        Karsi_3M,
        Karsi_4M,
        Karsi_5M,
        Karsi_6M,

        Ranalie1_1S,
        Ranalie1_2S,
        Ranalie1_3S,
        Ranalie1_4S,
        Ranalie1_5S,
        Ranalie1_6S,
        Ranalie1_7S,

        Ranalie1_1M,
        Ranalie1_2M,
        Ranalie1_3M,
        Ranalie1_4M,
        Ranalie1_5M,
        Ranalie1_6M,
        Ranalie1_7M,

        Ranalie2_1S,
        Ranalie2_2S,
        Ranalie2_3S,
        Ranalie2_4S,
        Ranalie2_5S,
        Ranalie2_6S,
        Ranalie2_7S,
        Ranalie2_8S,

        Ranalie2_1M,
        Ranalie2_2M,
        Ranalie2_3M,
        Ranalie2_4M,
        Ranalie2_5M,
        Ranalie2_6M,
        Ranalie2_7M,
        Ranalie2_8M,

        // streets
        NimiAus1_S,
        NimiAus2_S,
        Orn1_S,
        Orn2_S,

        NimiAus1_M,
        NimiAus2_M,
        Orn1_M,
        Orn2_M,

        MegVaro_1S,
        MegVaro_2S,
        MegVaro_3S,
        MegVaro_4S,
        MegVaro_5S,
        MegVaro_6S,

        MegVaro_1M,
        MegVaro_2M,
        MegVaro_3M,
        MegVaro_4M,
        MegVaro_5M,
        MegVaro_6M,

        Matti1_1S,
        Matti1_2S,
        Matti1_3S,
        Matti1_4S,
        Matti1_5S,
        Matti1_6S,
        Matti1_7S,

        Matti1_1M,
        Matti1_2M,
        Matti1_3M,
        Matti1_4M,
        Matti1_5M,
        Matti1_6M,
        Matti1_7M,

        Matti2_1S,
        Matti2_2S,
        Matti2_3S,
        Matti2_4S,
        Matti2_5S,
        Matti2_6S,
        Matti2_7S,
        Matti2_8S,

        Matti2_1M,
        Matti2_2M,
        Matti2_3M,
        Matti2_4M,
        Matti2_5M,
        Matti2_6M,
        Matti2_7M,
        Matti2_8M,

        // lakeside
        Mav1_S,
        Mav2_S,
        LetteJay1_S,
        LetteJay2_S,

        Mav1_M,
        Mav2_M,
        LetteJay1_M,
        LetteJay2_M,

        Blush_1S,
        Blush_2S,
        Blush_3S,
        Blush_4S,
        Blush_5S,
        Blush_6S,

        Blush_1M,
        Blush_2M,
        Blush_3M,
        Blush_4M,
        Blush_5M,
        Blush_6M,

        Avy1_1S,
        Avy1_2S,
        Avy1_3S,
        Avy1_4S,
        Avy1_5S,
        Avy1_6S,

        Avy1_1M,
        Avy1_2M,
        Avy1_3M,
        Avy1_4M,
        Avy1_5M,
        Avy1_6M,

        Avy2_1S,
        Avy2_2S,
        Avy2_3S,
        Avy2_4S,
        Avy2_5S,
        Avy2_6S,
        Avy2_7S,
        Avy2_8S,

        Avy2_1M,
        Avy2_2M,
        Avy2_3M,
        Avy2_4M,
        Avy2_5M,
        Avy2_6M,
        Avy2_7M,
        Avy2_8M,

        // keep
        Axe_S,
        Harp_S,
        Knives_S,
        Spear_S,
        Staff_S,

        Axe_M,
        Harp_M,
        Knives_M,
        Spear_M,
        Staff_M,

        // pinnacle
        Shira1_1S,
        Shira1_2S,
        Shira1_3S,
        Shira1_4S,
        Shira1_5S,
        Shira1_6S,

        Shira1_1M,
        Shira1_2M,
        Shira1_3M,
        Shira1_4M,
        Shira1_5M,
        Shira1_6M,

        Shira2_1S,
        Shira2_2S,
        Shira2_3S,
        Shira2_4S,
        Shira2_5S,
        Shira2_6S,
        Shira2_7S,
        Shira2_8S,

        Shira2_1M,
        Shira2_2M,
        Shira2_3M,
        Shira2_4M,
        Shira2_5M,
        Shira2_6M,
        Shira2_7M,
        Shira2_8M,

        // mixes
        Ranalie1_Mix_S,
        Avy2_Mix_S,
        Shira2_Mix_S
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
