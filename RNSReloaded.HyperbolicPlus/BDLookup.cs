using RNSReloaded.HyperbolicPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RNSReloaded.HyperbolicPlus.Config.Config;

namespace RNSReloaded.HyperbolicPlus;

public enum Stage {
    NONE = 0,
    OUTSKIRTS = 1,
    NEST = 2,
    ARSENAL = 3,
    LIGHTHOUSE = 4,
    STREETS = 5,
    LAKESIDE = 6,
    KEEP = 7,
    PINNACLE = 8
}

public enum Anims {
    None,
    Tassha,
    Karsi,
    Twili,
    Merran,
    Ranalie,
    Matti,
    Avy,
    Shira
}

public enum Mixes {
    None,
    Nest, Arsenal, Lighthouse, Streets, Lakeside, Keep,
    Saya_S, Saya_M,
    Tassha_S, Tassha_M,
    Karsi_S, Karsi_M,
    MegVaro_S, MegVaro_M,
    Blush_S, Blush_M,
    Twili1_S, Twili2_S,
    Twili1_M, Twili2_M,
    Merran1_S, Merran2_S,
    Merran1_M, Merran2_M,
    Ranalie1_S, Ranalie2_S,
    Ranalie1_M, Ranalie2_M,
    Matti1_S, Matti2_S,
    Matti1_M, Matti2_M,
    Avy1_S, Avy2_S,
    Avy1_M, Avy2_M,
    Shira1_S, Shira2_S,
    Shira1_M, Shira2_M
}

public class BDLookup {
    // there is a lot of data here. Assume data is correct unless a comment specifies
    public struct PatternData {
        public string enemy { get; }
        public bool multi { get; }
        public int length { get; }
        public string? partner { get; }
        public Mixes mixes { get; }

        public PatternData(string enemy, bool multi = false, int length = 20000, string? partner = null, Mixes mix = Mixes.None) {
            this.enemy = enemy;
            this.multi = multi;
            this.length = length;
            this.partner = partner;
            this.mixes = mix;
        }
    }

    public struct EnemyData {
        public bool basic { get; }
        public int stage { get; }
        public double zoom { get; }
        public Anims anims { get; }

        public EnemyData(Stage stage = Stage.OUTSKIRTS, double zoom = 1.0, bool basic = true, Anims anim = Anims.None) {
            this.basic = basic;
            this.stage = (int) stage;
            this.zoom = zoom;
            this.anims = anim;
        }
    }

    public static readonly Dictionary<string, string> NameMap = new() {
        // maps names to patterns
        // names remove 0-indexing, uses character names, and reorders to reflect gameplay order
        // tutorial (UNIMPLEMENTED)
        { "Bella1", "bp_bird_sophomore0" },
        { "Rem1", "bp_wolf_blackear0" },
        { "Sohko1", "bp_dragon_granite0" },
        { "Sohko1_S", "bp_dragon_granite0_s" },
        { "PinElta1", "bp_mouse_cadet0" },
        { "Maxi1", "bp_frog_tinkerer0" },
        { "Maxi1_S", "bp_frog_tinkerer0_s" },

        // outskirts
        { "Bella2_S", "bp_bird_sophomore1_s" },
        { "Bella3_S", "bp_bird_sophomore2_s" },
        { "Rem2_S", "bp_wolf_blackear1_s" },
        { "Rem3_S", "bp_wolf_blackear2_s" },
        { "Sohko2_S", "bp_dragon_granite1_s" },
        { "Sohko3_S", "bp_dragon_granite2_s" },
        { "PinElta2_S", "bp_mouse_cadet1_s" },
        { "PinElta3_S", "bp_mouse_cadet2_s" },
        { "Maxi2_S", "bp_frog_tinkerer1_s" },
        { "Maxi3_S", "bp_frog_tinkerer2_s" },

        { "Bella2M", "bp_bird_sophomore1" },
        { "Bella3M", "bp_bird_sophomore2" },
        { "Rem2M", "bp_wolf_blackear1" },
        { "Rem3M", "bp_wolf_blackear2" },
        { "Sohko2M", "bp_dragon_granite1" },
        { "Sohko3M", "bp_dragon_granite2" },
        { "PinElta2M", "bp_mouse_cadet1" },
        { "PinElta3M", "bp_mouse_cadet2" },
        { "Maxi2M", "bp_frog_tinkerer1" },
        { "Maxi3M", "bp_frog_tinkerer2" },

        // nest
        { "Menna1_S", "bp_bird_student0_s" },
        { "Menna2_S", "bp_bird_student1_s" },
        { "Azel1_S", "bp_bird_whispering0_s" },
        { "Azel2_S", "bp_bird_whispering1_s" },

        { "Menna1_M", "bp_bird_student0" },
        { "Menna2_M", "bp_bird_student1" },
        { "Azel1_M", "bp_bird_whispering0" },
        { "Azel2_M", "bp_bird_whispering1" },
        
        // pt2 saya doesnt exist in the game UNIMPLEMENTED
        // account for in pattern exceptions
        // unsorted UNIMPLEMENTED
        { "Saya_1S", "bp_bird_archon0_pt2_s" },
        { "Saya_2S", "bp_bird_archon0_pt3_s" },
        { "Saya_3S", "bp_bird_archon0_pt4_s" },
        { "Saya_4S", "bp_bird_archon0_pt5_s" },
        { "Saya_5S", "bp_bird_archon0_pt6_s" },
        { "Saya_6S", "bp_bird_archon0_pt7_s" },

        { "Saya_1M", "bp_bird_archon0_pt2" },
        { "Saya_2M", "bp_bird_archon0_pt3" },
        { "Saya_3M", "bp_bird_archon0_pt4" },
        { "Saya_4M", "bp_bird_archon0_pt5" },
        { "Saya_5M", "bp_bird_archon0_pt6" },
        { "Saya_6M", "bp_bird_archon0_pt7" },

        // unsorted UNIMPLEMENTED
        { "Twili1_1S", "bp_bird_valedictorian0_pt2_s" },
        { "Twili1_2S", "bp_bird_valedictorian0_pt3_s" },
        { "Twili1_3S", "bp_bird_valedictorian0_pt4_s" },
        { "Twili1_4S", "bp_bird_valedictorian0_pt5_s" },
        { "Twili1_5S", "bp_bird_valedictorian0_pt6_s" },
        { "Twili1_6S", "bp_bird_valedictorian0_pt7_s" },

        { "Twili1_1M", "bp_bird_valedictorian0_pt2" },
        { "Twili1_2M", "bp_bird_valedictorian0_pt3" },
        { "Twili1_3M", "bp_bird_valedictorian0_pt4" },
        { "Twili1_4M", "bp_bird_valedictorian0_pt5" },
        { "Twili1_5M", "bp_bird_valedictorian0_pt6" },
        { "Twili1_6M", "bp_bird_valedictorian0_pt7" },

        { "Twili2_1S", "bp_bird_valedictorian1_pt2_s" },
        { "Twili2_2S", "bp_bird_valedictorian1_pt3_s" },
        { "Twili2_3S", "bp_bird_valedictorian1_pt4_s" },
        { "Twili2_4S", "bp_bird_valedictorian1_pt5_s" },
        { "Twili2_5S", "bp_bird_valedictorian1_pt6_s" },
        { "Twili2_6S", "bp_bird_valedictorian1_pt7_s" },
        { "Twili2_7S", "bp_bird_valedictorian1_pt8_s" },
        { "Twili2_8S", "bp_bird_valedictorian1_pt9_s" },

        { "Twili2_1M", "bp_bird_valedictorian1_pt2" },
        { "Twili2_2M", "bp_bird_valedictorian1_pt3" },
        { "Twili2_3M", "bp_bird_valedictorian1_pt4" },
        { "Twili2_4M", "bp_bird_valedictorian1_pt5" },
        { "Twili2_5M", "bp_bird_valedictorian1_pt6" },
        { "Twili2_6M", "bp_bird_valedictorian1_pt7" },
        { "Twili2_7M", "bp_bird_valedictorian1_pt8" },
        { "Twili2_8M", "bp_bird_valedictorian1_pt9" },

        // arsenal
        { "Mink1_S", "bp_wolf_greyeye0_s" },
        { "Mink2_S", "bp_wolf_greyeye1_s" },
        { "RanXin1_S", "bp_wolf_bluepaw0_s" },
        { "RanXin2_S", "bp_wolf_bluepaw1_s" },

        { "Mink1_M", "bp_wolf_greyeye0" },
        { "Mink2_M", "bp_wolf_greyeye1" },
        { "RanXin1_M", "bp_wolf_bluepaw0" },
        { "RanXin2_M", "bp_wolf_bluepaw1" },

        // unsorted UNIMPLEMENTED
        { "Tassha_1S", "bp_wolf_snowfur0_pt2_s" },
        { "Tassha_2S", "bp_wolf_snowfur0_pt3_s" },
        { "Tassha_3S", "bp_wolf_snowfur0_pt4_s" },
        { "Tassha_4S", "bp_wolf_snowfur0_pt5_s" },
        { "Tassha_5S", "bp_wolf_snowfur0_pt6_s" },
        { "Tassha_6S", "bp_wolf_snowfur0_pt7_s" },

        { "Tassha_1M", "bp_wolf_snowfur0_pt2" },
        { "Tassha_2M", "bp_wolf_snowfur0_pt3" },
        { "Tassha_3M", "bp_wolf_snowfur0_pt4" },
        { "Tassha_4M", "bp_wolf_snowfur0_pt5" },
        { "Tassha_5M", "bp_wolf_snowfur0_pt6" },
        { "Tassha_6M", "bp_wolf_snowfur0_pt7" },

        // unsorted UNIMPLEMENTED
        { "Merran1_1S", "bp_wolf_steeltooth0_pt2_s" },
        { "Merran1_2S", "bp_wolf_steeltooth0_pt3_s" },
        { "Merran1_3S", "bp_wolf_steeltooth0_pt4_s" },
        { "Merran1_4S", "bp_wolf_steeltooth0_pt5_s" },
        { "Merran1_5S", "bp_wolf_steeltooth0_pt6_s" },
        { "Merran1_6S", "bp_wolf_steeltooth0_pt7_s" },
        { "Merran1_7S", "bp_wolf_steeltooth0_pt8_s" },

        { "Merran1_1M", "bp_wolf_steeltooth0_pt2" },
        { "Merran1_2M", "bp_wolf_steeltooth0_pt3" },
        { "Merran1_3M", "bp_wolf_steeltooth0_pt4" },
        { "Merran1_4M", "bp_wolf_steeltooth0_pt5" },
        { "Merran1_5M", "bp_wolf_steeltooth0_pt6" },
        { "Merran1_6M", "bp_wolf_steeltooth0_pt7" },
        { "Merran1_7M", "bp_wolf_steeltooth0_pt8" },

        { "Merran2_1S", "bp_wolf_steeltooth1_pt2_s" },
        { "Merran2_2S", "bp_wolf_steeltooth1_pt3_s" },
        { "Merran2_3S", "bp_wolf_steeltooth1_pt4_s" },
        { "Merran2_4S", "bp_wolf_steeltooth1_pt5_s" },
        { "Merran2_5S", "bp_wolf_steeltooth1_pt6_s" },
        { "Merran2_6S", "bp_wolf_steeltooth1_pt7_s" },
        { "Merran2_7S", "bp_wolf_steeltooth1_pt8_s" },
        { "Merran2_8S", "bp_wolf_steeltooth1_pt9_s" },

        { "Merran2_1M", "bp_wolf_steeltooth1_pt2" },
        { "Merran2_2M", "bp_wolf_steeltooth1_pt3" },
        { "Merran2_3M", "bp_wolf_steeltooth1_pt4" },
        { "Merran2_4M", "bp_wolf_steeltooth1_pt5" },
        { "Merran2_5M", "bp_wolf_steeltooth1_pt6" },
        { "Merran2_6M", "bp_wolf_steeltooth1_pt7" },
        { "Merran2_7M", "bp_wolf_steeltooth1_pt8" },
        { "Merran2_8M", "bp_wolf_steeltooth1_pt9" },

        // lighthouse
        { "PiKuu1_S", "bp_dragon_gold0_s" },
        { "PiKuu2_S", "bp_dragon_gold1_s" },
        { "Illie1_S", "bp_dragon_emerald0_s" },
        { "Illie2_S", "bp_dragon_emerald1_s" },

        { "PiKuu1_M", "bp_dragon_gold0" },
        { "PiKuu2_M", "bp_dragon_gold1" },
        { "Illie1_M", "bp_dragon_emerald0" },
        { "Illie2_M", "bp_dragon_emerald1" },

        // unsorted UNIMPLEMENTED
        { "Karsi_1S", "bp_dragon_ruby0_pt2_s" },
        { "Karsi_2S", "bp_dragon_ruby0_pt3_s" },
        { "Karsi_3S", "bp_dragon_ruby0_pt4_s" },
        { "Karsi_4S", "bp_dragon_ruby0_pt5_s" },
        { "Karsi_5S", "bp_dragon_ruby0_pt6_s" },
        { "Karsi_6S", "bp_dragon_ruby0_pt7_s" },

        { "Karsi_1M", "bp_dragon_ruby0_pt2" },
        { "Karsi_2M", "bp_dragon_ruby0_pt3" },
        { "Karsi_3M", "bp_dragon_ruby0_pt4" },
        { "Karsi_4M", "bp_dragon_ruby0_pt5" },
        { "Karsi_5M", "bp_dragon_ruby0_pt6" },
        { "Karsi_6M", "bp_dragon_ruby0_pt7" },

        // unsorted UNIMPLEMENTED
        { "Ranalie1_1S", "bp_dragon_mythril0_pt2_s" },
        { "Ranalie1_2S", "bp_dragon_mythril0_pt3_s" },
        { "Ranalie1_3S", "bp_dragon_mythril0_pt4_s" },
        { "Ranalie1_4S", "bp_dragon_mythril0_pt5_s" },
        { "Ranalie1_5S", "bp_dragon_mythril0_pt6_s" },
        { "Ranalie1_6S", "bp_dragon_mythril0_pt7_s" },
        { "Ranalie1_7S", "bp_dragon_mythril0_pt8_s" },

        { "Ranalie1_1M", "bp_dragon_mythril0_pt2" },
        { "Ranalie1_2M", "bp_dragon_mythril0_pt3" },
        { "Ranalie1_3M", "bp_dragon_mythril0_pt4" },
        { "Ranalie1_4M", "bp_dragon_mythril0_pt5" },
        { "Ranalie1_5M", "bp_dragon_mythril0_pt6" },
        { "Ranalie1_6M", "bp_dragon_mythril0_pt7" },
        { "Ranalie1_7M", "bp_dragon_mythril0_pt8" },

        { "Ranalie2_1S", "bp_dragon_mythril1_pt2_s" },
        { "Ranalie2_2S", "bp_dragon_mythril1_pt3_s" },
        { "Ranalie2_3S", "bp_dragon_mythril1_pt4_s" },
        { "Ranalie2_4S", "bp_dragon_mythril1_pt5_s" },
        { "Ranalie2_5S", "bp_dragon_mythril1_pt6_s" },
        { "Ranalie2_6S", "bp_dragon_mythril1_pt7_s" },
        { "Ranalie2_7S", "bp_dragon_mythril1_pt8_s" },
        { "Ranalie2_8S", "bp_dragon_mythril1_pt9_s" },

        { "Ranalie2_1M", "bp_dragon_mythril1_pt2" },
        { "Ranalie2_2M", "bp_dragon_mythril1_pt3" },
        { "Ranalie2_3M", "bp_dragon_mythril1_pt4" },
        { "Ranalie2_4M", "bp_dragon_mythril1_pt5" },
        { "Ranalie2_5M", "bp_dragon_mythril1_pt6" },
        { "Ranalie2_6M", "bp_dragon_mythril1_pt7" },
        { "Ranalie2_7M", "bp_dragon_mythril1_pt8" },
        { "Ranalie2_8M", "bp_dragon_mythril1_pt9" },

        // streets
        { "NimiAus1_S", "bp_mouse_archer0_s" },
        { "NimiAus2_S", "bp_mouse_archer1_s" },
        { "Orn1_S", "bp_mouse_oakspear0_s" },
        { "Orn2_S", "bp_mouse_oakspear1_s" },

        { "NimiAus1_M", "bp_mouse_archer0" },
        { "NimiAus2_M", "bp_mouse_archer1" },
        { "Orn1_M", "bp_mouse_oakspear0" },
        { "Orn2_M", "bp_mouse_oakspear1" },

        // unsorted UNIMPLEMENTED
        { "MegVaro_1S", "bp_mouse_rosemage0_pt2_s" },
        { "MegVaro_2S", "bp_mouse_rosemage0_pt3_s" },
        { "MegVaro_3S", "bp_mouse_rosemage0_pt4_s" },
        { "MegVaro_4S", "bp_mouse_rosemage0_pt5_s" },
        { "MegVaro_5S", "bp_mouse_rosemage0_pt6_s" },
        { "MegVaro_6S", "bp_mouse_rosemage0_pt7_s" },

        { "MegVaro_1M", "bp_mouse_rosemage0_pt2" },
        { "MegVaro_2M", "bp_mouse_rosemage0_pt3" },
        { "MegVaro_3M", "bp_mouse_rosemage0_pt4" },
        { "MegVaro_4M", "bp_mouse_rosemage0_pt5" },
        { "MegVaro_5M", "bp_mouse_rosemage0_pt6" },
        { "MegVaro_6M", "bp_mouse_rosemage0_pt7" },

        // unsorted UNIMPLEMENTED
        { "Matti1_1S", "bp_mouse_paladin0_pt2_s" },
        { "Matti1_2S", "bp_mouse_paladin0_pt3_s" },
        { "Matti1_3S", "bp_mouse_paladin0_pt4_s" },
        { "Matti1_4S", "bp_mouse_paladin0_pt5_s" },
        { "Matti1_5S", "bp_mouse_paladin0_pt6_s" },
        { "Matti1_6S", "bp_mouse_paladin0_pt7_s" },
        { "Matti1_7S", "bp_mouse_paladin0_pt8_s" },

        { "Matti1_1M", "bp_mouse_paladin0_pt2" },
        { "Matti1_2M", "bp_mouse_paladin0_pt3" },
        { "Matti1_3M", "bp_mouse_paladin0_pt4" },
        { "Matti1_4M", "bp_mouse_paladin0_pt5" },
        { "Matti1_5M", "bp_mouse_paladin0_pt6" },
        { "Matti1_6M", "bp_mouse_paladin0_pt7" },
        { "Matti1_7M", "bp_mouse_paladin0_pt8" },

        { "Matti2_1S", "bp_mouse_paladin1_pt2_s" },
        { "Matti2_2S", "bp_mouse_paladin1_pt3_s" },
        { "Matti2_3S", "bp_mouse_paladin1_pt4_s" },
        { "Matti2_4S", "bp_mouse_paladin1_pt5_s" },
        { "Matti2_5S", "bp_mouse_paladin1_pt6_s" },
        { "Matti2_6S", "bp_mouse_paladin1_pt7_s" },
        { "Matti2_7S", "bp_mouse_paladin1_pt8_s" },
        { "Matti2_8S", "bp_mouse_paladin1_pt9_s" },

        { "Matti2_1M", "bp_mouse_paladin1_pt2" },
        { "Matti2_2M", "bp_mouse_paladin1_pt3" },
        { "Matti2_3M", "bp_mouse_paladin1_pt4" },
        { "Matti2_4M", "bp_mouse_paladin1_pt5" },
        { "Matti2_5M", "bp_mouse_paladin1_pt6" },
        { "Matti2_6M", "bp_mouse_paladin1_pt7" },
        { "Matti2_7M", "bp_mouse_paladin1_pt8" },
        { "Matti2_8M", "bp_mouse_paladin1_pt9" },

        // lakeside
        { "Mav1_S", "bp_frog_seamstress0_s" },
        { "Mav2_S", "bp_frog_seamstress1_s" },
        { "LetteJay1_S", "bp_frog_songstress0_s" },
        { "LetteJay2_S", "bp_frog_songstress1_s" },

        { "Mav1_M", "bp_frog_seamstress0" },
        { "Mav2_M", "bp_frog_seamstress1" }, // broken
        { "LetteJay1_M", "bp_frog_songstress0" },
        { "LetteJay2_M", "bp_frog_songstress1" },

        // unsorted UNIMPLEMENTED
        { "Blush_1S", "bp_frog_painter0_pt2_s" },
        { "Blush_2S", "bp_frog_painter0_pt3_s" },
        { "Blush_3S", "bp_frog_painter0_pt4_s" },
        { "Blush_4S", "bp_frog_painter0_pt5_s" },
        { "Blush_5S", "bp_frog_painter0_pt6_s" },
        { "Blush_6S", "bp_frog_painter0_pt7_s" },

        { "Blush_1M", "bp_frog_painter0_pt2" },
        { "Blush_2M", "bp_frog_painter0_pt3" },
        { "Blush_3M", "bp_frog_painter0_pt4" },
        { "Blush_4M", "bp_frog_painter0_pt5" },
        { "Blush_5M", "bp_frog_painter0_pt6" },
        { "Blush_6M", "bp_frog_painter0_pt7" },

        // unsorted UNIMPLEMENTED
        { "Avy1_1S", "bp_frog_idol0_pt2_s" },
        { "Avy1_2S", "bp_frog_idol0_pt3_s" },
        { "Avy1_3S", "bp_frog_idol0_pt4_s" },
        { "Avy1_4S", "bp_frog_idol0_pt5_s" },
        { "Avy1_5S", "bp_frog_idol0_pt6_s" },
        { "Avy1_6S", "bp_frog_idol0_pt7_s" },

        { "Avy1_1M", "bp_frog_idol0_pt2" },
        { "Avy1_2M", "bp_frog_idol0_pt3" },
        { "Avy1_3M", "bp_frog_idol0_pt4" },
        { "Avy1_4M", "bp_frog_idol0_pt5" },
        { "Avy1_5M", "bp_frog_idol0_pt6" },
        { "Avy1_6M", "bp_frog_idol0_pt7" },

        { "Avy2_1S", "bp_frog_idol1_pt2_s" },
        { "Avy2_2S", "bp_frog_idol1_pt3_s" },
        { "Avy2_3S", "bp_frog_idol1_pt4_s" },
        { "Avy2_4S", "bp_frog_idol1_pt5_s" },
        { "Avy2_5S", "bp_frog_idol1_pt6_s" },
        { "Avy2_6S", "bp_frog_idol1_pt7_s" },
        { "Avy2_7S", "bp_frog_idol1_pt8_s" },
        { "Avy2_8S", "bp_frog_idol1_pt9_s" },

        { "Avy2_1M", "bp_frog_idol1_pt2" },
        { "Avy2_2M", "bp_frog_idol1_pt3" },
        { "Avy2_3M", "bp_frog_idol1_pt4" },
        { "Avy2_4M", "bp_frog_idol1_pt5" },
        { "Avy2_5M", "bp_frog_idol1_pt6" },
        { "Avy2_6M", "bp_frog_idol1_pt7" },
        { "Avy2_7M", "bp_frog_idol1_pt8" },
        { "Avy2_8M", "bp_frog_idol1_pt9" },

        // keep
        { "Axe_S", "bp_queens_axe0_s" },
        { "Harp_S", "bp_queens_harp0_s" },
        { "Knives_S", "bp_queens_knife_r0_s" },
        { "Spear_S", "bp_queens_spear0_s" },
        { "Staff_S", "bp_queens_staff0_s" },

        { "Axe_M", "bp_queens_axe0" },
        { "Harp_M", "bp_queens_harp0" },
        { "Knives_M", "bp_queens_knife_r0" },
        { "Spear_M", "bp_queens_spear0" },
        { "Staff_M", "bp_queens_staff0" },

        // pinnacle
        // unsorted UNIMPLEMENTED
        { "Shira1_1S", "bp_rabbit_queen0_pt2_s" },
        { "Shira1_2S", "bp_rabbit_queen0_pt3_s" },
        { "Shira1_3S", "bp_rabbit_queen0_pt4_s" },
        { "Shira1_4S", "bp_rabbit_queen0_pt5_s" },
        { "Shira1_5S", "bp_rabbit_queen0_pt6_s" },
        { "Shira1_6S", "bp_rabbit_queen0_pt7_s" },

        { "Shira1_1M", "bp_rabbit_queen0_pt2" },
        { "Shira1_2M", "bp_rabbit_queen0_pt3" },
        { "Shira1_3M", "bp_rabbit_queen0_pt4" },
        { "Shira1_4M", "bp_rabbit_queen0_pt5" },
        { "Shira1_5M", "bp_rabbit_queen0_pt6" },
        { "Shira1_6M", "bp_rabbit_queen0_pt7" },

        { "Shira2_1S", "bp_rabbit_queen1_pt2_s" },
        { "Shira2_2S", "bp_rabbit_queen1_pt3_s" },
        { "Shira2_3S", "bp_rabbit_queen1_pt4_s" },
        { "Shira2_4S", "bp_rabbit_queen1_pt5_s" },
        { "Shira2_5S", "bp_rabbit_queen1_pt6_s" },
        { "Shira2_6S", "bp_rabbit_queen1_pt7_s" },
        { "Shira2_7S", "bp_rabbit_queen1_pt8_s" },
        { "Shira2_8S", "bp_rabbit_queen1_pt9_s" },

        { "Shira2_1M", "bp_rabbit_queen1_pt2" },
        { "Shira2_2M", "bp_rabbit_queen1_pt3" },
        { "Shira2_3M", "bp_rabbit_queen1_pt4" },
        { "Shira2_4M", "bp_rabbit_queen1_pt5" },
        { "Shira2_5M", "bp_rabbit_queen1_pt6" },
        { "Shira2_6M", "bp_rabbit_queen1_pt7" },
        { "Shira2_7M", "bp_rabbit_queen1_pt8" },
        { "Shira2_8M", "bp_rabbit_queen1_pt9" },

        // mixes
        { "Ranalie1_Mix_S", "mix_dragon_mythril0_s"},
        { "Avy2_Mix_S", "mix_frog_idol1_s"},
        { "Shira2_Mix_S", "mix_rabbit_queen1_s"},
    };

    public static readonly Dictionary<string, PatternData> PatternMap = new() {
        // patterns to enemy, multi, length, partner, mix
        // any patterns without specified times are untimed (UNIMPLEMENTED)
        // tagged data with startup and weirdo aren't currently in use
        // tutorial // (UNIMPLEMENTED)
        { "bp_bird_sophomore0", new PatternData("bird_sophomore0") },
        { "bp_wolf_blackear0", new PatternData("wolf_blackear0") },
        { "bp_dragon_granite0", new PatternData("dragon_granite0") },
        { "bp_dragon_granite0_s", new PatternData("dragon_granite0") },
        { "bp_mouse_cadet0", new PatternData("mouse_cadet0", partner: "bp_mouse_medic0") },
        { "bp_mouse_medic0", new PatternData("mouse_medic0") },
        { "bp_frog_tinkerer0", new PatternData("frog_tinkerer0") },
        { "bp_frog_tinkerer0_s", new PatternData("frog_tinkerer0") },
        
        // outskirts
        { "bp_bird_sophomore1_s", new PatternData("bird_sophomore1") },
        { "bp_bird_sophomore2_s", new PatternData("bird_sophomore2") },
        { "bp_wolf_blackear1_s", new PatternData("wolf_blackear1") },
        { "bp_wolf_blackear2_s", new PatternData("wolf_blackear2") },
        { "bp_dragon_granite1_s", new PatternData("dragon_granite1") },
        { "bp_dragon_granite2_s", new PatternData("dragon_granite2") },
        { "bp_mouse_cadet1_s", new PatternData("mouse_cadet1", partner: "bp_mouse_medic1_s") },
        { "bp_mouse_cadet2_s", new PatternData("mouse_cadet2", partner: "bp_mouse_medic2_s") },
        { "bp_mouse_medic1_s", new PatternData("mouse_medic1") },
        { "bp_mouse_medic2_s", new PatternData("mouse_medic2") },
        { "bp_frog_tinkerer1_s", new PatternData("frog_tinkerer1") },
        { "bp_frog_tinkerer2_s", new PatternData("frog_tinkerer2") },

        { "bp_bird_sophomore1", new PatternData("bird_sophomore1", true) },
        { "bp_bird_sophomore2", new PatternData("bird_sophomore2", true) },
        { "bp_wolf_blackear1", new PatternData("wolf_blackear1", true) },
        { "bp_wolf_blackear2", new PatternData("wolf_blackear2", true) },
        { "bp_dragon_granite1", new PatternData("dragon_granite1", true) },
        { "bp_dragon_granite2", new PatternData("dragon_granite2", true) },
        { "bp_mouse_cadet1", new PatternData("mouse_cadet1", true, partner: "bp_mouse_medic1") },
        { "bp_mouse_cadet2", new PatternData("mouse_cadet2", true, partner: "bp_mouse_medic2") },
        { "bp_mouse_medic1", new PatternData("mouse_medic1", true) },
        { "bp_mouse_medic2", new PatternData("mouse_medic2", true) },
        { "bp_frog_tinkerer1", new PatternData("frog_tinkerer1", true) },
        { "bp_frog_tinkerer2", new PatternData("frog_tinkerer2", true) },
        { "bp_wolf_blackear2_n", new PatternData("wolf_blackear2", true) }, // weirdo

        // nest
        // the entirety of birds is weird, please fix (UNIMPLEMENTED)
        { "bp_bird_student0_s", new PatternData("bird_student0") },
        { "bp_bird_student1_s", new PatternData("bird_student1") },
        { "bp_bird_whispering0_s", new PatternData("bird_whispering0") },
        { "bp_bird_whispering1_s", new PatternData("bird_whispering1") },

        { "bp_bird_student0", new PatternData("bird_student0", true) },
        { "bp_bird_student1", new PatternData("bird_student1", true) },
        { "bp_bird_whispering0", new PatternData("bird_whispering0", true) },
        { "bp_bird_whispering1", new PatternData("bird_whispering1", true) },
        { "bp_bird_student0_n", new PatternData("bird_student0", true) }, // weirdo
        { "bp_bird_student1_l", new PatternData("bird_student1", true) }, // weirdo
        { "bp_bird_whispering0_n", new PatternData("bird_whispering0", true) }, // weirdo
        { "bp_bird_whispering1_n", new PatternData("bird_whispering1", true) }, // weirdo

        { "bp_bird_archon0_s", new PatternData("bird_archon0") }, // startup
        // pt2 doesn't exist on its own
        { "bp_bird_archon0_pt3_s", new PatternData("bird_archon0") },
        { "bp_bird_archon0_pt4_s", new PatternData("bird_archon0") },
        { "bp_bird_archon0_pt5_s", new PatternData("bird_archon0") },
        { "bp_bird_archon0_pt6_s", new PatternData("bird_archon0") },
        { "bp_bird_archon0_pt7_s", new PatternData("bird_archon0") },
        { "bp_bird_archon0_pt2_sh", new PatternData("bird_archon0") }, // weirdo
        { "bp_bird_archon0_pt3_sh", new PatternData("bird_archon0") }, // weirdo
        { "bp_bird_archon0_pt4_sh", new PatternData("bird_archon0") }, // weirdo
        { "bp_bird_archon0_pt5_sh", new PatternData("bird_archon0") }, // weirdo
        { "bp_bird_archon0_pt6_sh", new PatternData("bird_archon0") }, // weirdo
        { "bp_bird_archon0_pt7_sh", new PatternData("bird_archon0") }, // weirdo

        { "bp_bird_archon0", new PatternData("bird_archon0", true) }, // startup
        { "bp_bird_archon0_pt2", new PatternData("bird_archon0", true) },
        { "bp_bird_archon0_pt3", new PatternData("bird_archon0", true) },
        { "bp_bird_archon0_pt4", new PatternData("bird_archon0", true) },
        { "bp_bird_archon0_pt5", new PatternData("bird_archon0", true) },
        { "bp_bird_archon0_pt6", new PatternData("bird_archon0", true) },
        { "bp_bird_archon0_pt7", new PatternData("bird_archon0", true) },
        { "bp_bird_archon0_pt2_h", new PatternData("bird_archon0", true) }, // weirdo
        { "bp_bird_archon0_pt3_h", new PatternData("bird_archon0", true) }, // weirdo
        { "bp_bird_archon0_pt4_h", new PatternData("bird_archon0", true) }, // weirdo
        { "bp_bird_archon0_pt5_h", new PatternData("bird_archon0", true) }, // weirdo
        { "bp_bird_archon0_pt6_h", new PatternData("bird_archon0", true) }, // weirdo
        { "bp_bird_archon0_pt7_h", new PatternData("bird_archon0", true) }, // weirdo

        { "bp_bird_valedictorian0_s", new PatternData("bird_valedictorian0") }, // startup
        { "bp_bird_valedictorian0_pt2_s", new PatternData("bird_valedictorian0") },
        { "bp_bird_valedictorian0_pt3_s", new PatternData("bird_valedictorian0") },
        { "bp_bird_valedictorian0_pt4_s", new PatternData("bird_valedictorian0") },
        { "bp_bird_valedictorian0_pt5_s", new PatternData("bird_valedictorian0") },
        { "bp_bird_valedictorian0_pt6_s", new PatternData("bird_valedictorian0") },
        { "bp_bird_valedictorian0_pt7_s", new PatternData("bird_valedictorian0") },
        { "bp_bird_valedictorian0_pt2_sh", new PatternData("bird_valedictorian0") }, // weirdo
        { "bp_bird_valedictorian0_pt3_sh", new PatternData("bird_valedictorian0") }, // weirdo
        { "bp_bird_valedictorian0_pt4_sh", new PatternData("bird_valedictorian0") }, // weirdo
        { "bp_bird_valedictorian0_pt5_sh", new PatternData("bird_valedictorian0") }, // weirdo
        { "bp_bird_valedictorian0_pt6_sh", new PatternData("bird_valedictorian0") }, // weirdo
        { "bp_bird_valedictorian0_pt7_sh", new PatternData("bird_valedictorian0") }, // weirdo

        { "bp_bird_valedictorian0", new PatternData("bird_valedictorian0", true) }, // startup
        { "bp_bird_valedictorian0_pt2", new PatternData("bird_valedictorian0", true) },
        { "bp_bird_valedictorian0_pt3", new PatternData("bird_valedictorian0", true) },
        { "bp_bird_valedictorian0_pt4", new PatternData("bird_valedictorian0", true) },
        { "bp_bird_valedictorian0_pt5", new PatternData("bird_valedictorian0", true) },
        { "bp_bird_valedictorian0_pt6", new PatternData("bird_valedictorian0", true) },
        { "bp_bird_valedictorian0_pt7", new PatternData("bird_valedictorian0", true) },
        { "bp_bird_valedictorian0_pt2_h", new PatternData("bird_valedictorian0", true) }, // weirdo
        { "bp_bird_valedictorian0_pt3_h", new PatternData("bird_valedictorian0", true) }, // weirdo
        { "bp_bird_valedictorian0_pt4_h", new PatternData("bird_valedictorian0", true) }, // weirdo
        { "bp_bird_valedictorian0_pt5_h", new PatternData("bird_valedictorian0", true) }, // weirdo
        { "bp_bird_valedictorian0_pt6_h", new PatternData("bird_valedictorian0", true) }, // weirdo
        { "bp_bird_valedictorian0_pt7_h", new PatternData("bird_valedictorian0", true) }, // weirdo

        { "bp_bird_valedictorian1_s", new PatternData("bird_valedictorian1") }, // startup
        { "bp_bird_valedictorian1_pt2_s", new PatternData("bird_valedictorian1") },
        { "bp_bird_valedictorian1_pt3_s", new PatternData("bird_valedictorian1") },
        { "bp_bird_valedictorian1_pt4_s", new PatternData("bird_valedictorian1") },
        { "bp_bird_valedictorian1_pt5_s", new PatternData("bird_valedictorian1") },
        { "bp_bird_valedictorian1_pt6_s", new PatternData("bird_valedictorian1") },
        { "bp_bird_valedictorian1_pt7_s", new PatternData("bird_valedictorian1") },
        { "bp_bird_valedictorian1_pt8_s", new PatternData("bird_valedictorian1") },
        { "bp_bird_valedictorian1_pt9_s", new PatternData("bird_valedictorian1") },
        { "bp_bird_valedictorian1_pt2_sh", new PatternData("bird_valedictorian1") }, // weirdo
        { "bp_bird_valedictorian1_pt3_sh", new PatternData("bird_valedictorian1") }, // weirdo
        { "bp_bird_valedictorian1_pt4_sh", new PatternData("bird_valedictorian1") }, // weirdo
        { "bp_bird_valedictorian1_pt5_sh", new PatternData("bird_valedictorian1") }, // weirdo
        { "bp_bird_valedictorian1_pt6_sh", new PatternData("bird_valedictorian1") }, // weirdo
        { "bp_bird_valedictorian1_pt7_sh", new PatternData("bird_valedictorian1") }, // weirdo
        { "bp_bird_valedictorian1_pt7_sl", new PatternData("bird_valedictorian1") }, // weirdo
        { "bp_bird_valedictorian1_pt8_sh", new PatternData("bird_valedictorian1") }, // weirdo
        { "bp_bird_valedictorian1_pt9_sh", new PatternData("bird_valedictorian1") }, // weirdo
        { "bp_bird_valedictorian1_pt9_sl", new PatternData("bird_valedictorian1") }, // weirdo

        { "bp_bird_valedictorian1", new PatternData("bird_valedictorian1", true) }, // startup
        { "bp_bird_valedictorian1_pt2", new PatternData("bird_valedictorian1", true) },
        { "bp_bird_valedictorian1_pt3", new PatternData("bird_valedictorian1", true) },
        { "bp_bird_valedictorian1_pt4", new PatternData("bird_valedictorian1", true) },
        { "bp_bird_valedictorian1_pt5", new PatternData("bird_valedictorian1", true) },
        { "bp_bird_valedictorian1_pt6", new PatternData("bird_valedictorian1", true) },
        { "bp_bird_valedictorian1_pt7", new PatternData("bird_valedictorian1", true) },
        { "bp_bird_valedictorian1_pt8", new PatternData("bird_valedictorian1", true) },
        { "bp_bird_valedictorian1_pt9", new PatternData("bird_valedictorian1", true) },
        { "bp_bird_valedictorian1_pt2_l", new PatternData("bird_valedictorian1", true) }, // weirdo
        { "bp_bird_valedictorian1_pt3_h", new PatternData("bird_valedictorian1", true) }, // weirdo
        { "bp_bird_valedictorian1_pt4_h", new PatternData("bird_valedictorian1", true) }, // weirdo
        { "bp_bird_valedictorian1_pt5_h", new PatternData("bird_valedictorian1", true) }, // weirdo
        { "bp_bird_valedictorian1_pt6_h", new PatternData("bird_valedictorian1", true) }, // weirdo
        { "bp_bird_valedictorian1_pt7_h", new PatternData("bird_valedictorian1", true) }, // weirdo
        { "bp_bird_valedictorian1_pt8_h", new PatternData("bird_valedictorian1", true) }, // weirdo
        { "bp_bird_valedictorian1_pt9_h", new PatternData("bird_valedictorian1", true) }, // weirdo

        // arsenal
        { "bp_wolf_greyeye0_s", new PatternData("wolf_greyeye0") },
        { "bp_wolf_greyeye1_s", new PatternData("wolf_greyeye1") },
        { "bp_wolf_bluepaw0_s", new PatternData("wolf_bluepaw0", partner: "bp_wolf_redclaw0_s") },
        { "bp_wolf_bluepaw1_s", new PatternData("wolf_bluepaw1", partner: "bp_wolf_redclaw1_s") },
        { "bp_wolf_redclaw0_s", new PatternData("wolf_redclaw0") },
        { "bp_wolf_redclaw1_s", new PatternData("wolf_redclaw1") },

        { "bp_wolf_greyeye0", new PatternData("wolf_greyeye0", true) },
        { "bp_wolf_greyeye1", new PatternData("wolf_greyeye1", true) },
        { "bp_wolf_bluepaw0", new PatternData("wolf_bluepaw0", true, partner: "bp_wolf_redclaw0") },
        { "bp_wolf_bluepaw1", new PatternData("wolf_bluepaw1", true, partner: "bp_wolf_redclaw1") },
        { "bp_wolf_redclaw0", new PatternData("wolf_redclaw0", true) },
        { "bp_wolf_redclaw1", new PatternData("wolf_redclaw1", true) },

        { "bp_wolf_snowfur0_s", new PatternData("wolf_snowfur0") }, // startup
        { "bp_wolf_snowfur0_pt2_s", new PatternData("wolf_snowfur0") },
        { "bp_wolf_snowfur0_pt3_s", new PatternData("wolf_snowfur0") },
        { "bp_wolf_snowfur0_pt4_s", new PatternData("wolf_snowfur0") },
        { "bp_wolf_snowfur0_pt5_s", new PatternData("wolf_snowfur0") },
        { "bp_wolf_snowfur0_pt6_s", new PatternData("wolf_snowfur0") },
        { "bp_wolf_snowfur0_pt7_s", new PatternData("wolf_snowfur0") },

        { "bp_wolf_snowfur0", new PatternData("wolf_snowfur0", true) }, // startup
        { "bp_wolf_snowfur0_pt2", new PatternData("wolf_snowfur0", true) },
        { "bp_wolf_snowfur0_pt3", new PatternData("wolf_snowfur0", true, 13000) },
        { "bp_wolf_snowfur0_pt4", new PatternData("wolf_snowfur0", true, 16000) },
        { "bp_wolf_snowfur0_pt5", new PatternData("wolf_snowfur0", true, 20000) },
        { "bp_wolf_snowfur0_pt6", new PatternData("wolf_snowfur0", true) },
        { "bp_wolf_snowfur0_pt7", new PatternData("wolf_snowfur0", true) },

        { "bp_wolf_steeltooth0_s", new PatternData("wolf_steeltooth0") }, // startup
        { "bp_wolf_steeltooth0_pt2_s", new PatternData("wolf_steeltooth0") },
        { "bp_wolf_steeltooth0_pt3_s", new PatternData("wolf_steeltooth0") },
        { "bp_wolf_steeltooth0_pt4_s", new PatternData("wolf_steeltooth0") },
        { "bp_wolf_steeltooth0_pt5_s", new PatternData("wolf_steeltooth0") },
        { "bp_wolf_steeltooth0_pt6_s", new PatternData("wolf_steeltooth0") },
        { "bp_wolf_steeltooth0_pt7_s", new PatternData("wolf_steeltooth0") },
        { "bp_wolf_steeltooth0_pt8_s", new PatternData("wolf_steeltooth0") },

        { "bp_wolf_steeltooth0", new PatternData("wolf_steeltooth0", true) }, // startup
        { "bp_wolf_steeltooth0_pt2", new PatternData("wolf_steeltooth0", true) },
        { "bp_wolf_steeltooth0_pt3", new PatternData("wolf_steeltooth0", true) },
        { "bp_wolf_steeltooth0_pt4", new PatternData("wolf_steeltooth0", true) },
        { "bp_wolf_steeltooth0_pt5", new PatternData("wolf_steeltooth0", true) },
        { "bp_wolf_steeltooth0_pt6", new PatternData("wolf_steeltooth0", true) },
        { "bp_wolf_steeltooth0_pt7", new PatternData("wolf_steeltooth0", true) },
        { "bp_wolf_steeltooth0_pt8", new PatternData("wolf_steeltooth0", true) },

        { "bp_wolf_steeltooth1_s", new PatternData("wolf_steeltooth1") }, // startup
        { "bp_wolf_steeltooth1_pt2_s", new PatternData("wolf_steeltooth1") },
        { "bp_wolf_steeltooth1_pt3_s", new PatternData("wolf_steeltooth1") },
        { "bp_wolf_steeltooth1_pt4_s", new PatternData("wolf_steeltooth1") },
        { "bp_wolf_steeltooth1_pt5_s", new PatternData("wolf_steeltooth1") },
        { "bp_wolf_steeltooth1_pt6_s", new PatternData("wolf_steeltooth1") },
        { "bp_wolf_steeltooth1_pt7_s", new PatternData("wolf_steeltooth1") },
        { "bp_wolf_steeltooth1_pt8_s", new PatternData("wolf_steeltooth1") },
        { "bp_wolf_steeltooth1_pt9_s", new PatternData("wolf_steeltooth1") },

        { "bp_wolf_steeltooth1", new PatternData("wolf_steeltooth1", true) }, // startup
        { "bp_wolf_steeltooth1_pt2", new PatternData("wolf_steeltooth1", true, 21000) },
        { "bp_wolf_steeltooth1_pt3", new PatternData("wolf_steeltooth1", true, 12500) },
        { "bp_wolf_steeltooth1_pt4", new PatternData("wolf_steeltooth1", true, 20000) },
        { "bp_wolf_steeltooth1_pt5", new PatternData("wolf_steeltooth1", true, 12500) },
        { "bp_wolf_steeltooth1_pt6", new PatternData("wolf_steeltooth1", true, 22000) },
        { "bp_wolf_steeltooth1_pt7", new PatternData("wolf_steeltooth1", true, 12500) },
        { "bp_wolf_steeltooth1_pt8", new PatternData("wolf_steeltooth1", true, 21000) },
        { "bp_wolf_steeltooth1_pt9", new PatternData("wolf_steeltooth1", true, 12500) },

        // lighthouse
        { "bp_dragon_gold0_s", new PatternData("dragon_gold0", partner: "bp_dragon_silver0_s") },
        { "bp_dragon_gold1_s", new PatternData("dragon_gold1", partner: "bp_dragon_silver1_s") },
        { "bp_dragon_silver0_s", new PatternData("dragon_silver0") },
        { "bp_dragon_silver1_s", new PatternData("dragon_silver1") },
        { "bp_dragon_emerald0_s", new PatternData("dragon_emerald0") },
        { "bp_dragon_emerald1_s", new PatternData("dragon_emerald1") },

        { "bp_dragon_gold0", new PatternData("dragon_gold0", true, partner: "bp_dragon_silver0") },
        { "bp_dragon_gold1", new PatternData("dragon_gold1", true, partner: "bp_dragon_silver1") },
        { "bp_dragon_silver0", new PatternData("dragon_silver0", true) },
        { "bp_dragon_silver1", new PatternData("dragon_silver1", true) },
        { "bp_dragon_emerald0", new PatternData("dragon_emerald0", true) },
        { "bp_dragon_emerald1", new PatternData("dragon_emerald1", true) },

        { "bp_dragon_ruby0_s", new PatternData("dragon_ruby0") }, // startup
        { "bp_dragon_ruby0_pt2_s", new PatternData("dragon_ruby0") },
        { "bp_dragon_ruby0_pt3_s", new PatternData("dragon_ruby0") },
        { "bp_dragon_ruby0_pt4_s", new PatternData("dragon_ruby0") },
        { "bp_dragon_ruby0_pt5_s", new PatternData("dragon_ruby0") },
        { "bp_dragon_ruby0_pt6_s", new PatternData("dragon_ruby0") },
        { "bp_dragon_ruby0_pt7_s", new PatternData("dragon_ruby0", length: 12500) },

        { "bp_dragon_ruby0", new PatternData("dragon_ruby0", true) }, // startup
        { "bp_dragon_ruby0_pt2", new PatternData("dragon_ruby0", true) },
        { "bp_dragon_ruby0_pt3", new PatternData("dragon_ruby0", true) },
        { "bp_dragon_ruby0_pt4", new PatternData("dragon_ruby0", true) },
        { "bp_dragon_ruby0_pt5", new PatternData("dragon_ruby0", true) },
        { "bp_dragon_ruby0_pt6", new PatternData("dragon_ruby0", true) },
        { "bp_dragon_ruby0_pt7", new PatternData("dragon_ruby0", true) },
        // { "bp_dragon_ruby0_perm", new PatternData() }, // idk what this does either

        { "bp_dragon_mythril0_s", new PatternData("dragon_mythril0") }, // startup
        { "bp_dragon_mythril0_pt2_s", new PatternData("dragon_mythril0", length: 12500) },
        { "bp_dragon_mythril0_pt3_s", new PatternData("dragon_mythril0", length: 10000) },
        { "bp_dragon_mythril0_pt4_s", new PatternData("dragon_mythril0", length: 15000) },
        { "bp_dragon_mythril0_pt5_s", new PatternData("dragon_mythril0", length: 10000) },
        { "bp_dragon_mythril0_pt6_s", new PatternData("dragon_mythril0", length: 13000) },
        { "bp_dragon_mythril0_pt7_s", new PatternData("dragon_mythril0", length: 16000) },
        { "bp_dragon_mythril0_pt8_s", new PatternData("dragon_mythril0", length: 14000) },

        { "bp_dragon_mythril0", new PatternData("dragon_mythril0", true) }, // startup
        { "bp_dragon_mythril0_pt2", new PatternData("dragon_mythril0", true) },
        { "bp_dragon_mythril0_pt3", new PatternData("dragon_mythril0", true) },
        { "bp_dragon_mythril0_pt4", new PatternData("dragon_mythril0", true) },
        { "bp_dragon_mythril0_pt5", new PatternData("dragon_mythril0", true) },
        { "bp_dragon_mythril0_pt6", new PatternData("dragon_mythril0", true) },
        { "bp_dragon_mythril0_pt7", new PatternData("dragon_mythril0", true) },
        { "bp_dragon_mythril0_pt8", new PatternData("dragon_mythril0", true) },

        { "bp_dragon_mythril1_s", new PatternData("dragon_mythril1") }, // startup
        { "bp_dragon_mythril1_pt2_s", new PatternData("dragon_mythril1") },
        { "bp_dragon_mythril1_pt3_s", new PatternData("dragon_mythril1") },
        { "bp_dragon_mythril1_pt4_s", new PatternData("dragon_mythril1", length: 12000) },
        { "bp_dragon_mythril1_pt5_s", new PatternData("dragon_mythril1") },
        { "bp_dragon_mythril1_pt6_s", new PatternData("dragon_mythril1") },
        { "bp_dragon_mythril1_pt7_s", new PatternData("dragon_mythril1") },
        { "bp_dragon_mythril1_pt8_s", new PatternData("dragon_mythril1") },
        { "bp_dragon_mythril1_pt9_s", new PatternData("dragon_mythril1") },

        { "bp_dragon_mythril1", new PatternData("dragon_mythril1", true) }, // startup
        { "bp_dragon_mythril1_pt2", new PatternData("dragon_mythril1", true) },
        { "bp_dragon_mythril1_pt3", new PatternData("dragon_mythril1", true, 22000) },
        { "bp_dragon_mythril1_pt4", new PatternData("dragon_mythril1", true, 15000) },
        { "bp_dragon_mythril1_pt5", new PatternData("dragon_mythril1", true) },
        { "bp_dragon_mythril1_pt6", new PatternData("dragon_mythril1", true, 15000) },
        { "bp_dragon_mythril1_pt7", new PatternData("dragon_mythril1", true, 15000) },
        { "bp_dragon_mythril1_pt8", new PatternData("dragon_mythril1", true) },
        { "bp_dragon_mythril1_pt9", new PatternData("dragon_mythril1", true) },

        // streets
        { "bp_mouse_archer0_s", new PatternData("mouse_archer0", partner: "bp_mouse_axewielder0_s") },
        { "bp_mouse_archer1_s", new PatternData("mouse_archer1", partner: "bp_mouse_axewielder1_s") },
        { "bp_mouse_axewielder0_s", new PatternData("mouse_axewielder0") },
        { "bp_mouse_axewielder1_s", new PatternData("mouse_axewielder1") },
        { "bp_mouse_oakspear0_s", new PatternData("mouse_oakspear0") },
        { "bp_mouse_oakspear1_s", new PatternData("mouse_oakspear1") },

        { "bp_mouse_archer0", new PatternData("mouse_archer0", true, partner: "bp_mouse_axewielder0") },
        { "bp_mouse_archer1", new PatternData("mouse_archer1", true, partner: "bp_mouse_axewielder1") },
        { "bp_mouse_axewielder0", new PatternData("mouse_axewielder0", true) },
        { "bp_mouse_axewielder1", new PatternData("mouse_axewielder1", true) },
        { "bp_mouse_oakspear0", new PatternData("mouse_oakspear0", true) },
        { "bp_mouse_oakspear1", new PatternData("mouse_oakspear1", true) },

        // currently broken
        { "bp_mouse_rosemage0_s", new PatternData("mouse_rosemage0", partner: "bp_mouse_commander0_s") }, // startup
        { "bp_mouse_rosemage0_pt2_s", new PatternData("mouse_rosemage0", partner: "bp_mouse_commander0_pt2_s") },
        { "bp_mouse_rosemage0_pt3_s", new PatternData("mouse_rosemage0", partner: "bp_mouse_commander0_pt3_s") },
        { "bp_mouse_rosemage0_pt4_s", new PatternData("mouse_rosemage0", partner: "bp_mouse_commander0_pt4_s") },
        { "bp_mouse_rosemage0_pt5_s", new PatternData("mouse_rosemage0", partner: "bp_mouse_commander0_pt5_s") },
        { "bp_mouse_rosemage0_pt6_s", new PatternData("mouse_rosemage0", partner: "bp_mouse_commander0_pt6_s") },
        { "bp_mouse_rosemage0_pt7_s", new PatternData("mouse_rosemage0", partner: "bp_mouse_commander0_pt7_s") },
        { "bp_mouse_commander0_s", new PatternData("mouse_commander0") }, // startup
        { "bp_mouse_commander0_pt2_s", new PatternData("mouse_commander0") },
        { "bp_mouse_commander0_pt3_s", new PatternData("mouse_commander0") },
        { "bp_mouse_commander0_pt4_s", new PatternData("mouse_commander0") },
        { "bp_mouse_commander0_pt5_s", new PatternData("mouse_commander0") },
        { "bp_mouse_commander0_pt6_s", new PatternData("mouse_commander0") },
        { "bp_mouse_commander0_pt7_s", new PatternData("mouse_commander0") },

        { "bp_mouse_rosemage0", new PatternData("mouse_rosemage0", true, partner: "bp_mouse_commander0") }, // startup
        { "bp_mouse_rosemage0_pt2", new PatternData("mouse_rosemage0", true, partner: "bp_mouse_commander0_pt2") },
        { "bp_mouse_rosemage0_pt3", new PatternData("mouse_rosemage0", true, partner: "bp_mouse_commander0_pt3") },
        { "bp_mouse_rosemage0_pt4", new PatternData("mouse_rosemage0", true, partner: "bp_mouse_commander0_pt4") },
        { "bp_mouse_rosemage0_pt5", new PatternData("mouse_rosemage0", true, partner: "bp_mouse_commander0_pt5") },
        { "bp_mouse_rosemage0_pt6", new PatternData("mouse_rosemage0", true, partner: "bp_mouse_commander0_pt6") },
        { "bp_mouse_rosemage0_pt7", new PatternData("mouse_rosemage0", true, partner: "bp_mouse_commander0_pt7") },
        { "bp_mouse_commander0", new PatternData("mouse_commander0", true) }, // startup
        { "bp_mouse_commander0_pt2", new PatternData("mouse_commander0", true) },
        { "bp_mouse_commander0_pt3", new PatternData("mouse_commander0", true) },
        { "bp_mouse_commander0_pt4", new PatternData("mouse_commander0", true) },
        { "bp_mouse_commander0_pt5", new PatternData("mouse_commander0", true) },
        { "bp_mouse_commander0_pt6", new PatternData("mouse_commander0", true) },
        { "bp_mouse_commander0_pt7", new PatternData("mouse_commander0", true) },

        { "bp_mouse_paladin0_s", new PatternData("mouse_paladin0") }, // startup
        { "bp_mouse_paladin0_pt2_s", new PatternData("mouse_paladin0") },
        { "bp_mouse_paladin0_pt3_s", new PatternData("mouse_paladin0") },
        { "bp_mouse_paladin0_pt4_s", new PatternData("mouse_paladin0") },
        { "bp_mouse_paladin0_pt5_s", new PatternData("mouse_paladin0") },
        { "bp_mouse_paladin0_pt6_s", new PatternData("mouse_paladin0") },
        { "bp_mouse_paladin0_pt7_s", new PatternData("mouse_paladin0") },
        { "bp_mouse_paladin0_pt8_s", new PatternData("mouse_paladin0", length: 20000) },
        { "bp_mouse_paladin0_pt4_sl", new PatternData("mouse_paladin0") }, // weirdo

        { "bp_mouse_paladin0", new PatternData("mouse_paladin0", true) }, // startup
        { "bp_mouse_paladin0_pt2", new PatternData("mouse_paladin0", true) },
        { "bp_mouse_paladin0_pt3", new PatternData("mouse_paladin0", true) },
        { "bp_mouse_paladin0_pt4", new PatternData("mouse_paladin0", true) },
        { "bp_mouse_paladin0_pt5", new PatternData("mouse_paladin0", true) },
        { "bp_mouse_paladin0_pt6", new PatternData("mouse_paladin0", true) },
        { "bp_mouse_paladin0_pt7", new PatternData("mouse_paladin0", true) },
        { "bp_mouse_paladin0_pt8", new PatternData("mouse_paladin0", true) },

        { "bp_mouse_paladin1_s", new PatternData("mouse_paladin1") }, // startup
        { "bp_mouse_paladin1_pt2_s", new PatternData("mouse_paladin1") },
        { "bp_mouse_paladin1_pt3_s", new PatternData("mouse_paladin1") },
        { "bp_mouse_paladin1_pt4_s", new PatternData("mouse_paladin1") },
        { "bp_mouse_paladin1_pt5_s", new PatternData("mouse_paladin1") },
        { "bp_mouse_paladin1_pt6_s", new PatternData("mouse_paladin1") },
        { "bp_mouse_paladin1_pt7_s", new PatternData("mouse_paladin1") },
        { "bp_mouse_paladin1_pt8_s", new PatternData("mouse_paladin1") },
        { "bp_mouse_paladin1_pt9_s", new PatternData("mouse_paladin1") },

        { "bp_mouse_paladin1", new PatternData("mouse_paladin1", true) }, // startup
        { "bp_mouse_paladin1_pt2", new PatternData("mouse_paladin1", true) },
        { "bp_mouse_paladin1_pt3", new PatternData("mouse_paladin1", true) },
        { "bp_mouse_paladin1_pt4", new PatternData("mouse_paladin1", true) },
        { "bp_mouse_paladin1_pt5", new PatternData("mouse_paladin1", true) },
        { "bp_mouse_paladin1_pt6", new PatternData("mouse_paladin1", true) },
        { "bp_mouse_paladin1_pt7", new PatternData("mouse_paladin1", true) },
        { "bp_mouse_paladin1_pt8", new PatternData("mouse_paladin1", true) },
        { "bp_mouse_paladin1_pt9", new PatternData("mouse_paladin1", true) },
        // { "bp_mouse_paladin1_repeat", new PatternData() }, // idk what this bp does

        // lakeside
        { "bp_frog_seamstress0_s", new PatternData("frog_seamstress0") },
        { "bp_frog_seamstress1_s", new PatternData("frog_seamstress1") },
        { "bp_frog_songstress0_s", new PatternData("frog_songstress0", partner: "bp_frog_musician0_s") },
        { "bp_frog_songstress1_s", new PatternData("frog_songstress1", partner: "bp_frog_musician1_s") },
        { "bp_frog_musician0_s", new PatternData("frog_musician0") },
        { "bp_frog_musician1_s", new PatternData("frog_musician1") },

        { "bp_frog_seamstress0", new PatternData("frog_seamstress0", true) },
        { "bp_frog_seamstress1", new PatternData("frog_seamstress1", true) },
        { "bp_frog_songstress0", new PatternData("frog_songstress0", true, partner: "bp_frog_musician0") },
        { "bp_frog_songstress1", new PatternData("frog_songstress1", true, partner: "bp_frog_musician1") },
        { "bp_frog_musician0", new PatternData("frog_musician0", true) },
        { "bp_frog_musician1", new PatternData("frog_musician1", true) },
        { "bp_frog_seamstress1_h", new PatternData("frog_seamstress1", true) }, // weirdo

        { "bp_frog_painter0_s", new PatternData("frog_painter0") }, // startup
        { "bp_frog_painter0_pt2_s", new PatternData("frog_painter0") },
        { "bp_frog_painter0_pt3_s", new PatternData("frog_painter0") },
        { "bp_frog_painter0_pt4_s", new PatternData("frog_painter0") },
        { "bp_frog_painter0_pt5_s", new PatternData("frog_painter0") },
        { "bp_frog_painter0_pt6_s", new PatternData("frog_painter0", length: 20000) },
        { "bp_frog_painter0_pt7_s", new PatternData("frog_painter0", length: 24000) },
        { "bp_frog_painter0_pt3_sl", new PatternData("frog_painter0") }, // weirdo

        { "bp_frog_painter0", new PatternData("frog_painter0", true) }, // startup
        { "bp_frog_painter0_pt2", new PatternData("frog_painter0", true) },
        { "bp_frog_painter0_pt3", new PatternData("frog_painter0", true) },
        { "bp_frog_painter0_pt4", new PatternData("frog_painter0", true) },
        { "bp_frog_painter0_pt5", new PatternData("frog_painter0", true) },
        { "bp_frog_painter0_pt6", new PatternData("frog_painter0", true) },
        { "bp_frog_painter0_pt7", new PatternData("frog_painter0", true) },
        { "bp_frog_painter0_pt3_l", new PatternData("frog_painter0", true) }, // weirdo

        { "bp_frog_idol0_s", new PatternData("frog_idol0") }, // startup
        { "bp_frog_idol0_pt2_s", new PatternData("frog_idol0") },
        { "bp_frog_idol0_pt3_s", new PatternData("frog_idol0") },
        { "bp_frog_idol0_pt4_s", new PatternData("frog_idol0") },
        { "bp_frog_idol0_pt5_s", new PatternData("frog_idol0") },
        { "bp_frog_idol0_pt6_s", new PatternData("frog_idol0") },
        { "bp_frog_idol0_pt7_s", new PatternData("frog_idol0") },

        { "bp_frog_idol0", new PatternData("frog_idol0", true) }, // startup
        { "bp_frog_idol0_pt2", new PatternData("frog_idol0", true) },
        { "bp_frog_idol0_pt3", new PatternData("frog_idol0", true) },
        { "bp_frog_idol0_pt4", new PatternData("frog_idol0", true) },
        { "bp_frog_idol0_pt5", new PatternData("frog_idol0", true) },
        { "bp_frog_idol0_pt6", new PatternData("frog_idol0", true) },
        { "bp_frog_idol0_pt7", new PatternData("frog_idol0", true) },

        { "bp_frog_idol1_s", new PatternData("frog_idol1") }, // startup
        { "bp_frog_idol1_pt2_s", new PatternData("frog_idol1", length: 18000) },
        { "bp_frog_idol1_pt3_s", new PatternData("frog_idol1", length: 16000) },
        { "bp_frog_idol1_pt4_s", new PatternData("frog_idol1", length: 21000) },
        { "bp_frog_idol1_pt5_s", new PatternData("frog_idol1", length: 18000) },
        { "bp_frog_idol1_pt6_s", new PatternData("frog_idol1", length: 16000) },
        { "bp_frog_idol1_pt7_s", new PatternData("frog_idol1", length: 17000) },
        { "bp_frog_idol1_pt8_s", new PatternData("frog_idol1", length: 17000) },
        { "bp_frog_idol1_pt9_s", new PatternData("frog_idol1", length: 20000) },

        { "bp_frog_idol1", new PatternData("frog_idol1", true) }, // startup
        { "bp_frog_idol1_pt2", new PatternData("frog_idol1", true) },
        { "bp_frog_idol1_pt3", new PatternData("frog_idol1", true) },
        { "bp_frog_idol1_pt4", new PatternData("frog_idol1", true) },
        { "bp_frog_idol1_pt5", new PatternData("frog_idol1", true) },
        { "bp_frog_idol1_pt6", new PatternData("frog_idol1", true) },
        { "bp_frog_idol1_pt7", new PatternData("frog_idol1", true) },
        { "bp_frog_idol1_pt8", new PatternData("frog_idol1", true) },
        { "bp_frog_idol1_pt9", new PatternData("frog_idol1", true) },
        { "bp_frog_idol1_pt7_h", new PatternData("frog_idol1", true) }, // weirdo

        // keep
        { "bp_queens_axe0_s", new PatternData("queens_axe0") },
        { "bp_queens_harp0_s", new PatternData("queens_harp0") },
        { "bp_queens_knife_l0_s", new PatternData("queens_knife_l0", partner: "bp_queens_knife_r0_s") },
        { "bp_queens_knife_r0_s", new PatternData("queens_knife_r0") },
        { "bp_queens_spear0_s", new PatternData("queens_spear0", partner: "bp_queens_shield0_s") },
        { "bp_queens_shield0_s", new PatternData("queens_shield0") },
        { "bp_queens_staff0_s", new PatternData("queens_staff0") },

        { "bp_queens_axe0", new PatternData("queens_axe0", true) },
        { "bp_queens_harp0", new PatternData("queens_harp0", true) },
        { "bp_queens_knife_l0", new PatternData("queens_knife_l0", true, partner: "bp_queens_knife_r0") },
        { "bp_queens_knife_r0", new PatternData("queens_knife_r0", true) },
        { "bp_queens_spear0", new PatternData("queens_spear0", true, partner: "bp_queens_shield0") },
        { "bp_queens_shield0", new PatternData("queens_shield0", true) },
        { "bp_queens_staff0", new PatternData("queens_staff0", true) },

        // pinnacle
        { "bp_rabbit_queen0_s", new PatternData("rabbit_queen0") }, // startup
        { "bp_rabbit_queen0_pt2_s", new PatternData("rabbit_queen0") },
        { "bp_rabbit_queen0_pt3_s", new PatternData("rabbit_queen0") },
        { "bp_rabbit_queen0_pt4_s", new PatternData("rabbit_queen0") },
        { "bp_rabbit_queen0_pt5_s", new PatternData("rabbit_queen0") },
        { "bp_rabbit_queen0_pt6_s", new PatternData("rabbit_queen0") },
        { "bp_rabbit_queen0_pt7_s", new PatternData("rabbit_queen0") },

        { "bp_rabbit_queen0", new PatternData("rabbit_queen0", true) }, // startup
        { "bp_rabbit_queen0_pt2", new PatternData("rabbit_queen0", true) },
        { "bp_rabbit_queen0_pt3", new PatternData("rabbit_queen0", true) },
        { "bp_rabbit_queen0_pt4", new PatternData("rabbit_queen0", true) },
        { "bp_rabbit_queen0_pt5", new PatternData("rabbit_queen0", true) },
        { "bp_rabbit_queen0_pt6", new PatternData("rabbit_queen0", true) },
        { "bp_rabbit_queen0_pt7", new PatternData("rabbit_queen0", true) },

        { "bp_rabbit_queen1_s", new PatternData("rabbit_queen1") }, // startup
        { "bp_rabbit_queen1_pt2_s", new PatternData("rabbit_queen1", length: 20000) },
        { "bp_rabbit_queen1_pt3_s", new PatternData("rabbit_queen1", length: 20000) },
        { "bp_rabbit_queen1_pt4_s", new PatternData("rabbit_queen1", length: 16000) },
        { "bp_rabbit_queen1_pt5_s", new PatternData("rabbit_queen1", length: 19000) },
        { "bp_rabbit_queen1_pt6_s", new PatternData("rabbit_queen1", length: 20000) },
        { "bp_rabbit_queen1_pt7_s", new PatternData("rabbit_queen1", length: 15000) },
        { "bp_rabbit_queen1_pt8_s", new PatternData("rabbit_queen1", length: 20000) },
        { "bp_rabbit_queen1_pt9_s", new PatternData("rabbit_queen1", length: 15000) },

        { "bp_rabbit_queen1", new PatternData("rabbit_queen1", true) }, // startup
        { "bp_rabbit_queen1_pt2", new PatternData("rabbit_queen1", true) },
        { "bp_rabbit_queen1_pt3", new PatternData("rabbit_queen1", true) },
        { "bp_rabbit_queen1_pt4", new PatternData("rabbit_queen1", true) },
        { "bp_rabbit_queen1_pt5", new PatternData("rabbit_queen1", true, 19000) },
        { "bp_rabbit_queen1_pt6", new PatternData("rabbit_queen1", true, 20000) },
        { "bp_rabbit_queen1_pt7", new PatternData("rabbit_queen1", true) },
        { "bp_rabbit_queen1_pt8", new PatternData("rabbit_queen1", true) },
        { "bp_rabbit_queen1_pt9", new PatternData("rabbit_queen1", true) },

        // mixes
        // { "mix_dragon_ruby0_s", new PatternData("dragon_ruby0", mix: Mixes.Karsi) },
        //{ "mix_dragon_mythril0_s", new PatternData("dragon_mythril0", mix: Mixes.Ranalie1_S) },
        // { "mix_dragon_mythril0_s", new PatternData("dragon_mythril0", mix: Mixes.Ranalie1_S) },
        { "mix_dragon_mythril0_s", new PatternData("dragon_mythril0", mix: Mixes.Ranalie1_S) },
        { "mix_frog_idol1_s", new PatternData("frog_idol1", mix: Mixes.Avy2_S) },
        { "mix_rabbit_queen1_s", new PatternData("rabbit_queen1", mix: Mixes.Shira2_S) }
    };

    public static readonly Dictionary<string, EnemyData> EnemyMap = new() {
        // enemy to stage, zoom, basic, anims
        // tutorial (data has not been test. Zoom and basic could be off)
        { "bird_sophomore0", new EnemyData(Stage.OUTSKIRTS) },
        { "wolf_blackear0", new EnemyData(Stage.OUTSKIRTS) },
        { "dragon_granite0", new EnemyData(Stage.OUTSKIRTS) },
        { "mouse_cadet0", new EnemyData(zoom: 0.9) },
        { "frog_tinkerer0", new EnemyData(Stage.OUTSKIRTS) },

        // outskirts
        { "bird_sophomore1", new EnemyData(Stage.OUTSKIRTS) },
        { "bird_sophomore2", new EnemyData(Stage.OUTSKIRTS) },
        { "wolf_blackear1", new EnemyData(Stage.OUTSKIRTS) },
        { "wolf_blackear2", new EnemyData(Stage.OUTSKIRTS) },
        { "dragon_granite1", new EnemyData(Stage.OUTSKIRTS) },
        { "dragon_granite2", new EnemyData(Stage.OUTSKIRTS) },
        { "mouse_cadet1", new EnemyData(zoom: 0.9) },
        { "mouse_cadet2", new EnemyData(zoom: 0.9) },
        { "frog_tinkerer1", new EnemyData(Stage.OUTSKIRTS) },
        { "frog_tinkerer2", new EnemyData(Stage.OUTSKIRTS) },

        // nest
        // azel has a zoom exception, checked on pattern level (UNIMPLEMENTED)
        { "bird_student0", new EnemyData(Stage.NEST, 0.8) },
        { "bird_student1", new EnemyData(Stage.NEST) },
        { "bird_whispering0", new EnemyData(Stage.NEST, 0.9) }, // exception
        { "bird_whispering1", new EnemyData(Stage.NEST, 0.9) },
        { "bird_archon0", new EnemyData(Stage.NEST, 0.9, false) },
        { "bird_valedictorian0", new EnemyData(Stage.NEST, 0.9, false) },
        { "bird_valedictorian1", new EnemyData(Stage.NEST, 0.85, false, Anims.Twili) },

        // arsenal
        { "wolf_greyeye0", new EnemyData(Stage.ARSENAL, 0.8) },
        { "wolf_greyeye1", new EnemyData(Stage.ARSENAL, 0.8) },
        { "wolf_bluepaw0", new EnemyData(Stage.ARSENAL, 0.9) },
        { "wolf_bluepaw1", new EnemyData(Stage.ARSENAL, 0.9) },
        { "wolf_snowfur0", new EnemyData(Stage.ARSENAL, 0.9, false, Anims.Tassha) },
        { "wolf_steeltooth0", new EnemyData(Stage.ARSENAL, 0.9) },
        { "wolf_steeltooth1", new EnemyData(Stage.ARSENAL, 0.75, false, Anims.Merran) },

        // lighthouse
        // Karsi transformation is (UNIMPLEMENTED)
        { "dragon_gold0", new EnemyData(Stage.LIGHTHOUSE) },
        { "dragon_gold1", new EnemyData(Stage.LIGHTHOUSE) },
        { "dragon_emerald0", new EnemyData(Stage.LIGHTHOUSE, 0.95) },
        { "dragon_emerald1", new EnemyData(Stage.LIGHTHOUSE, 0.95) },
        { "dragon_ruby0", new EnemyData(Stage.LIGHTHOUSE, 0.8, false, Anims.Karsi) },
        { "dragon_mythril0", new EnemyData(Stage.LIGHTHOUSE, 0.9, false) },
        { "dragon_mythril1", new EnemyData(Stage.LIGHTHOUSE, 0.7, false, Anims.Ranalie) },

        // streets
        { "mouse_archer0", new EnemyData(Stage.STREETS, 0.8) },
        { "mouse_archer1", new EnemyData(Stage.STREETS, 0.8) },
        { "mouse_oakspear0", new EnemyData(Stage.STREETS, 0.9) },
        { "mouse_oakspear1", new EnemyData(Stage.STREETS, 0.9) },
        { "mouse_rosemage0", new EnemyData(Stage.STREETS, 0.8, false) },
        { "mouse_paladin0", new EnemyData(Stage.STREETS, 0.9, false) },
        { "mouse_paladin1", new EnemyData(Stage.STREETS, 0.8, false, Anims.Matti) },

        // lakeside
        { "frog_seamstress0", new EnemyData(Stage.LAKESIDE, 0.9) },
        { "frog_seamstress1", new EnemyData(Stage.LAKESIDE, 0.9) },
        { "frog_songstress0", new EnemyData(Stage.LAKESIDE, 0.9) },
        { "frog_songstress1", new EnemyData(Stage.LAKESIDE, 0.9) },
        { "frog_painter0", new EnemyData(Stage.LAKESIDE, 0.9, false) },
        { "frog_idol0", new EnemyData(Stage.LAKESIDE, 0.85, false) },
        { "frog_idol1", new EnemyData(Stage.LAKESIDE, 0.85, false, Anims.Avy) },

        // keep
        { "queens_axe0", new EnemyData(Stage.KEEP, 0.9) },
        { "queens_harp0", new EnemyData(Stage.KEEP) },
        { "queens_knife0", new EnemyData(Stage.KEEP, 0.9) },
        { "queens_spear0", new EnemyData(Stage.KEEP, 0.9) },
        { "queens_staff0", new EnemyData(Stage.KEEP) },

        // pinnacle
        { "rabbit_queen0", new EnemyData(Stage.PINNACLE, 0.9, false) },
        { "rabbit_queen1", new EnemyData(Stage.PINNACLE, 0.75, false, Anims.Shira) }
    };

    public static readonly Dictionary<Mixes, List<string>> MixMap = new() {
        { Mixes.None, []},
        { Mixes.Avy2_S, [
            "bp_frog_idol1_pt2_s",
            "bp_frog_idol1_pt3_s",
            "bp_frog_idol1_pt4_s",
            "bp_frog_idol1_pt5_s",
            "bp_frog_idol1_pt6_s",
            "bp_frog_idol1_pt7_s",
            "bp_frog_idol1_pt8_s",
            "bp_frog_idol1_pt9_s"
        ] },
        { Mixes.Ranalie1_S, [
            "bp_dragon_mythril0_pt2_s",
            "bp_dragon_mythril0_pt3_s",
            "bp_dragon_mythril0_pt4_s",
            "bp_dragon_mythril0_pt5_s",
            "bp_dragon_mythril0_pt6_s",
            "bp_dragon_mythril0_pt7_s",
            "bp_dragon_mythril0_pt8_s"
        ] },
        { Mixes.Shira2_S, [
            "bp_rabbit_queen1_pt2_s",
            // "bp_rabbit_queen1_pt3_s", // steel, baited
            "bp_rabbit_queen1_pt4_s",
            "bp_rabbit_queen1_pt5_s", // steel
            "bp_rabbit_queen1_pt6_s",
            "bp_rabbit_queen1_pt7_s", // steel
            "bp_rabbit_queen1_pt8_s",
            // "bp_rabbit_queen1_pt9_s",
        ] }
    };
}
