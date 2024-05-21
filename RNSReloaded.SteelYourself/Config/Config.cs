using System.ComponentModel;

namespace RNSReloaded.SteelYourself.Config;

public class Config : Configurable<Config> {
    // ReSharper disable InconsistentNaming
    public enum Encounter {
        enc_bird_archon0,
        enc_bird_sophomore0,
        enc_bird_sophomore1,
        enc_bird_sophomore2,
        enc_bird_student0,
        enc_bird_student1,
        enc_bird_valedictorian0,
        enc_bird_valedictorian1,
        enc_bird_whispering0,
        enc_bird_whispering1,
        enc_dragon_emerald0,
        enc_dragon_emerald1,
        enc_dragon_gold0,
        enc_dragon_gold1,
        enc_dragon_granite0,
        enc_dragon_granite1,
        enc_dragon_granite2,
        enc_dragon_mythril0,
        enc_dragon_mythril1,
        enc_dragon_ruby0,
        enc_frog_idol0,
        enc_frog_idol1,
        enc_frog_painter0,
        enc_frog_seamstress0,
        enc_frog_seamstress1,
        enc_frog_songstress0,
        enc_frog_songstress1,
        enc_frog_tinkerer0,
        enc_frog_tinkerer1,
        enc_frog_tinkerer2,
        enc_mouse_archer0,
        enc_mouse_archer1,
        enc_mouse_cadet0,
        enc_mouse_cadet1,
        enc_mouse_cadet2,
        enc_mouse_oakspear0,
        enc_mouse_oakspear1,
        enc_mouse_paladin0,
        enc_mouse_paladin1,
        enc_mouse_rosemage0,
        enc_queens_axe0,
        enc_queens_harp0,
        enc_queens_knife0,
        enc_queens_spear0,
        enc_queens_staff0,
        enc_rabbit_queen0,
        enc_rabbit_queen1,
        enc_wolf_blackear0,
        enc_wolf_blackear1,
        enc_wolf_blackear2,
        enc_wolf_bluepaw0,
        enc_wolf_bluepaw1,
        enc_wolf_greyeye0,
        enc_wolf_greyeye1,
        enc_wolf_snowfur0,
        enc_wolf_steeltooth0,
        enc_wolf_steeltooth1
    }

    [DisplayName("Encounter")]
    [Description("The encounter to force.")]
    [DefaultValue(Encounter.enc_rabbit_queen0)]
    public Encounter ForcedEncounter { get; set; } = Encounter.enc_rabbit_queen0;
}
