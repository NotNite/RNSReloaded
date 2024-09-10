using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

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
    Twili1_S,Twili2_S,
    Twili1_M,Twili2_M,
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
        public Mixes mixes { get; }

        public PatternData(string enemy, bool multi = false, int length = 20000, Mixes mix = Mixes.None) {
            this.enemy = enemy;
            this.multi = multi;
            this.length = length;
            this.mixes = mix;
        }
    }

    public struct EnemyData {
        public bool basic { get; }
        public int stage { get; }
        public double zoom { get; }
        public Anims anims { get; }

        public EnemyData(Stage stage = Stage.OUTSKIRTS, double zoom = 1, bool basic = true, Anims anim = Anims.None) {
            this.basic = basic;
            this.stage = (int) stage;
            this.zoom = zoom;
            this.anims = anim;
        }
    }

    private static Dictionary<string, string> NameMap = new Dictionary<string, string> {
        // maps names to patterns

    };

    private static Dictionary<string, PatternData> PatternMap = new Dictionary<string, PatternData> {
        // patterns to enemy, multi, length, mix
        // any patterns without specified times are untimed (UNIMPLEMENTED)
        // tutorial // (UNIMPLEMENTED)
        { "bp_bird_sophomore0", new PatternData("bird_sophomore0") },
        { "bp_wolf_blackear0", new PatternData("wolf_blackear0") },
        { "bp_dragon_granite0", new PatternData("dragon_granite0") },
        { "bp_dragon_granite0_s", new PatternData("dragon_granite0") },
        { "bp_mouse_cadet0", new PatternData("mouse_cadet0") },
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
        { "bp_mouse_cadet1_s", new PatternData("mouse_cadet1") },
        { "bp_mouse_cadet2_s", new PatternData("mouse_cadet2") },
        { "bp_mouse_medic1_s", new PatternData("mouse_medic1") },
        { "bp_mouse_medic2_s", new PatternData("mouse_medic2") },
        { "bp_frog_tinkerer1_s", new PatternData("frog_tinkerer1") },
        { "bp_frog_tinkerer2_s", new PatternData("frog_tinkerer2") },

        { "bp_bird_sophomore1", new PatternData("bird_sophomore1") },
        { "bp_bird_sophomore2", new PatternData("bird_sophomore2") },
        { "bp_wolf_blackear1", new PatternData("wolf_blackear1") },
        { "bp_wolf_blackear2", new PatternData("wolf_blackear2") },
        { "bp_dragon_granite1", new PatternData("dragon_granite1") },
        { "bp_dragon_granite2", new PatternData("dragon_granite2") },
        { "bp_mouse_cadet1", new PatternData("mouse_cadet1") },
        { "bp_mouse_cadet2", new PatternData("mouse_cadet2") },
        { "bp_mouse_medic1", new PatternData("mouse_medic1") },
        { "bp_mouse_medic2", new PatternData("mouse_medic2") },
        { "bp_frog_tinkerer1", new PatternData("frog_tinkerer1") },
        { "bp_frog_tinkerer2", new PatternData("frog_tinkerer2") },
        { "bp_wolf_blackear2_n", new PatternData("wolf_blackear2") }, // weirdo

        // nest
        // the entirety of birds is weird, please fix (UNIMPLEMENTED)
        { "bp_bird_student0_s", new PatternData() },
        { "bp_bird_student1_s", new PatternData() },
        { "bp_bird_whispering0_s", new PatternData() },
        { "bp_bird_whispering1_s", new PatternData() },

        { "bp_bird_student0", new PatternData() },
        { "bp_bird_student1", new PatternData() },
        { "bp_bird_whispering0", new PatternData() },
        { "bp_bird_whispering1", new PatternData() },
        { "bp_bird_student0_n", new PatternData() }, // weirdo
        { "bp_bird_student1_l", new PatternData() }, // weirdo
        { "bp_bird_whispering0_n", new PatternData() }, // weirdo
        { "bp_bird_whispering1_n", new PatternData() }, // weirdo

        { "bp_bird_archon0_s", new PatternData() }, // startup
        { "bp_bird_archon0_pt3_s", new PatternData() },
        { "bp_bird_archon0_pt4_s", new PatternData() },
        { "bp_bird_archon0_pt5_s", new PatternData() },
        { "bp_bird_archon0_pt6_s", new PatternData() },
        { "bp_bird_archon0_pt7_s", new PatternData() },
        { "bp_bird_archon0_pt2_sh", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt3_sh", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt4_sh", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt5_sh", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt6_sh", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt7_sh", new PatternData() }, // weirdo

        { "bp_bird_archon0", new PatternData() }, // startup
        { "bp_bird_archon0_pt2", new PatternData() },
        { "bp_bird_archon0_pt3", new PatternData() },
        { "bp_bird_archon0_pt4", new PatternData() },
        { "bp_bird_archon0_pt5", new PatternData() },
        { "bp_bird_archon0_pt6", new PatternData() },
        { "bp_bird_archon0_pt7", new PatternData() },
        { "bp_bird_archon0_pt2_h", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt3_h", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt4_h", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt5_h", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt6_h", new PatternData() }, // weirdo
        { "bp_bird_archon0_pt7_h", new PatternData() }, // weirdo

        { "bp_bird_valedictorian0_s", new PatternData() }, // startup
        { "bp_bird_valedictorian0_pt2_s", new PatternData() },
        { "bp_bird_valedictorian0_pt3_s", new PatternData() },
        { "bp_bird_valedictorian0_pt4_s", new PatternData() },
        { "bp_bird_valedictorian0_pt5_s", new PatternData() },
        { "bp_bird_valedictorian0_pt6_s", new PatternData() },
        { "bp_bird_valedictorian0_pt7_s", new PatternData() },
        { "bp_bird_valedictorian0_pt2_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt3_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt4_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt5_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt6_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt7_sh", new PatternData() }, // weirdo

        { "bp_bird_valedictorian0", new PatternData() }, // startup
        { "bp_bird_valedictorian0_pt2", new PatternData() },
        { "bp_bird_valedictorian0_pt3", new PatternData() },
        { "bp_bird_valedictorian0_pt4", new PatternData() },
        { "bp_bird_valedictorian0_pt5", new PatternData() },
        { "bp_bird_valedictorian0_pt6", new PatternData() },
        { "bp_bird_valedictorian0_pt7", new PatternData() },
        { "bp_bird_valedictorian0_pt2_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt3_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt4_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt5_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt6_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian0_pt7_h", new PatternData() }, // weirdo

        { "bp_bird_valedictorian1_s", new PatternData() }, // startup
        { "bp_bird_valedictorian1_pt2_s", new PatternData() },
        { "bp_bird_valedictorian1_pt3_s", new PatternData() },
        { "bp_bird_valedictorian1_pt4_s", new PatternData() },
        { "bp_bird_valedictorian1_pt5_s", new PatternData() },
        { "bp_bird_valedictorian1_pt6_s", new PatternData() },
        { "bp_bird_valedictorian1_pt7_s", new PatternData() },
        { "bp_bird_valedictorian1_pt8_s", new PatternData() },
        { "bp_bird_valedictorian1_pt9_s", new PatternData() },
        { "bp_bird_valedictorian1_pt2_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt3_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt4_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt5_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt6_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt7_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt7_sl", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt8_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt9_sh", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt9_sl", new PatternData() }, // weirdo

        { "bp_bird_valedictorian1", new PatternData() }, // startup
        { "bp_bird_valedictorian1_pt2", new PatternData() },
        { "bp_bird_valedictorian1_pt3", new PatternData() },
        { "bp_bird_valedictorian1_pt4", new PatternData() },
        { "bp_bird_valedictorian1_pt5", new PatternData() },
        { "bp_bird_valedictorian1_pt6", new PatternData() },
        { "bp_bird_valedictorian1_pt7", new PatternData() },
        { "bp_bird_valedictorian1_pt8", new PatternData() },
        { "bp_bird_valedictorian1_pt9", new PatternData() },
        { "bp_bird_valedictorian1_pt2_l", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt3_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt4_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt5_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt6_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt7_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt8_h", new PatternData() }, // weirdo
        { "bp_bird_valedictorian1_pt9_h", new PatternData() }, // weirdo

        // arsenal
        { "bp_wolf_greyeye0_s", new PatternData() },
        { "bp_wolf_greyeye1_s", new PatternData() },
        { "bp_wolf_bluepaw0_s", new PatternData() },
        { "bp_wolf_bluepaw1_s", new PatternData() },
        { "bp_wolf_redclaw0_s", new PatternData() },
        { "bp_wolf_redclaw1_s", new PatternData() },

        { "bp_wolf_greyeye0", new PatternData() },
        { "bp_wolf_greyeye1", new PatternData() },
        { "bp_wolf_bluepaw0", new PatternData() },
        { "bp_wolf_bluepaw1", new PatternData() },
        { "bp_wolf_redclaw0", new PatternData() },
        { "bp_wolf_redclaw1", new PatternData() },

        { "bp_wolf_snowfur0_s", new PatternData() }, // startup
        { "bp_wolf_snowfur0_pt2_s", new PatternData() },
        { "bp_wolf_snowfur0_pt3_s", new PatternData() },
        { "bp_wolf_snowfur0_pt4_s", new PatternData() },
        { "bp_wolf_snowfur0_pt5_s", new PatternData() },
        { "bp_wolf_snowfur0_pt6_s", new PatternData() },
        { "bp_wolf_snowfur0_pt7_s", new PatternData() },

        { "bp_wolf_snowfur0", new PatternData() }, // startup
        { "bp_wolf_snowfur0_pt2", new PatternData() },
        { "bp_wolf_snowfur0_pt3", new PatternData() },
        { "bp_wolf_snowfur0_pt4", new PatternData() },
        { "bp_wolf_snowfur0_pt5", new PatternData() },
        { "bp_wolf_snowfur0_pt6", new PatternData() },
        { "bp_wolf_snowfur0_pt7", new PatternData() },

        { "bp_wolf_steeltooth0_s", new PatternData() }, // startup
        { "bp_wolf_steeltooth0_pt2_s", new PatternData() },
        { "bp_wolf_steeltooth0_pt3_s", new PatternData() },
        { "bp_wolf_steeltooth0_pt4_s", new PatternData() },
        { "bp_wolf_steeltooth0_pt5_s", new PatternData() },
        { "bp_wolf_steeltooth0_pt6_s", new PatternData() },
        { "bp_wolf_steeltooth0_pt7_s", new PatternData() },
        { "bp_wolf_steeltooth0_pt8_s", new PatternData() },

        { "bp_wolf_steeltooth0", new PatternData() }, // startup
        { "bp_wolf_steeltooth0_pt2", new PatternData() },
        { "bp_wolf_steeltooth0_pt3", new PatternData() },
        { "bp_wolf_steeltooth0_pt4", new PatternData() },
        { "bp_wolf_steeltooth0_pt5", new PatternData() },
        { "bp_wolf_steeltooth0_pt6", new PatternData() },
        { "bp_wolf_steeltooth0_pt7", new PatternData() },
        { "bp_wolf_steeltooth0_pt8", new PatternData() },

        { "bp_wolf_steeltooth1_s", new PatternData() }, // startup
        { "bp_wolf_steeltooth1_pt2_s", new PatternData() },
        { "bp_wolf_steeltooth1_pt3_s", new PatternData() },
        { "bp_wolf_steeltooth1_pt4_s", new PatternData() },
        { "bp_wolf_steeltooth1_pt5_s", new PatternData() },
        { "bp_wolf_steeltooth1_pt6_s", new PatternData() },
        { "bp_wolf_steeltooth1_pt7_s", new PatternData() },
        { "bp_wolf_steeltooth1_pt8_s", new PatternData() },
        { "bp_wolf_steeltooth1_pt9_s", new PatternData() },

        { "bp_wolf_steeltooth1", new PatternData() }, // startup
        { "bp_wolf_steeltooth1_pt2", new PatternData() },
        { "bp_wolf_steeltooth1_pt3", new PatternData() },
        { "bp_wolf_steeltooth1_pt4", new PatternData() },
        { "bp_wolf_steeltooth1_pt5", new PatternData() },
        { "bp_wolf_steeltooth1_pt6", new PatternData() },
        { "bp_wolf_steeltooth1_pt7", new PatternData() },
        { "bp_wolf_steeltooth1_pt8", new PatternData() },
        { "bp_wolf_steeltooth1_pt9", new PatternData() },

        // lighthouse
        { "bp_dragon_gold0_s", new PatternData() },
        { "bp_dragon_gold1_s", new PatternData() },
        { "bp_dragon_silver0_s", new PatternData() },
        { "bp_dragon_silver1_s", new PatternData() },
        { "bp_dragon_emerald0_s", new PatternData() },
        { "bp_dragon_emerald1_s", new PatternData() },

        { "bp_dragon_gold0", new PatternData() },
        { "bp_dragon_gold1", new PatternData() },
        { "bp_dragon_silver0", new PatternData() },
        { "bp_dragon_silver1", new PatternData() },
        { "bp_dragon_emerald0", new PatternData() },
        { "bp_dragon_emerald1", new PatternData() },

        { "bp_dragon_ruby0_s", new PatternData() }, // startup
        { "bp_dragon_ruby0_pt2_s", new PatternData() },
        { "bp_dragon_ruby0_pt3_s", new PatternData() },
        { "bp_dragon_ruby0_pt4_s", new PatternData() },
        { "bp_dragon_ruby0_pt5_s", new PatternData() },
        { "bp_dragon_ruby0_pt6_s", new PatternData() },
        { "bp_dragon_ruby0_pt7_s", new PatternData() },

        { "bp_dragon_ruby0", new PatternData() }, // startup
        { "bp_dragon_ruby0_pt2", new PatternData() },
        { "bp_dragon_ruby0_pt3", new PatternData() },
        { "bp_dragon_ruby0_pt4", new PatternData() },
        { "bp_dragon_ruby0_pt5", new PatternData() },
        { "bp_dragon_ruby0_pt6", new PatternData() },
        { "bp_dragon_ruby0_pt7", new PatternData() },
        // { "bp_dragon_ruby0_perm", new PatternData() }, // idk what this does either

        { "bp_dragon_mythril0_s", new PatternData() }, // startup
        { "bp_dragon_mythril0_pt2_s", new PatternData() },
        { "bp_dragon_mythril0_pt3_s", new PatternData() },
        { "bp_dragon_mythril0_pt4_s", new PatternData() },
        { "bp_dragon_mythril0_pt5_s", new PatternData() },
        { "bp_dragon_mythril0_pt6_s", new PatternData() },
        { "bp_dragon_mythril0_pt7_s", new PatternData() },
        { "bp_dragon_mythril0_pt8_s", new PatternData() },

        { "bp_dragon_mythril0", new PatternData() }, // startup
        { "bp_dragon_mythril0_pt2", new PatternData() },
        { "bp_dragon_mythril0_pt3", new PatternData() },
        { "bp_dragon_mythril0_pt4", new PatternData() },
        { "bp_dragon_mythril0_pt5", new PatternData() },
        { "bp_dragon_mythril0_pt6", new PatternData() },
        { "bp_dragon_mythril0_pt7", new PatternData() },
        { "bp_dragon_mythril0_pt8", new PatternData() },

        { "bp_dragon_mythril1_s", new PatternData() }, // startup
        { "bp_dragon_mythril1_pt2_s", new PatternData() },
        { "bp_dragon_mythril1_pt3_s", new PatternData() },
        { "bp_dragon_mythril1_pt4_s", new PatternData() },
        { "bp_dragon_mythril1_pt5_s", new PatternData() },
        { "bp_dragon_mythril1_pt6_s", new PatternData() },
        { "bp_dragon_mythril1_pt7_s", new PatternData() },
        { "bp_dragon_mythril1_pt8_s", new PatternData() },
        { "bp_dragon_mythril1_pt9_s", new PatternData() },

        { "bp_dragon_mythril1", new PatternData() }, // startup
        { "bp_dragon_mythril1_pt2", new PatternData() },
        { "bp_dragon_mythril1_pt3", new PatternData() },
        { "bp_dragon_mythril1_pt4", new PatternData() },
        { "bp_dragon_mythril1_pt5", new PatternData() },
        { "bp_dragon_mythril1_pt6", new PatternData() },
        { "bp_dragon_mythril1_pt7", new PatternData() },
        { "bp_dragon_mythril1_pt8", new PatternData() },
        { "bp_dragon_mythril1_pt9", new PatternData() },

        // streets
        { "bp_mouse_archer0_s", new PatternData() },
        { "bp_mouse_archer1_s", new PatternData() },
        { "bp_mouse_axewielder0_s", new PatternData() },
        { "bp_mouse_axewielder1_s", new PatternData() },
        { "bp_mouse_oakspear0_s", new PatternData() },
        { "bp_mouse_oakspear1_s", new PatternData() },

        { "bp_mouse_archer0", new PatternData() },
        { "bp_mouse_archer1", new PatternData() },
        { "bp_mouse_axewielder0", new PatternData() },
        { "bp_mouse_axewielder1", new PatternData() },
        { "bp_mouse_oakspear0", new PatternData() },
        { "bp_mouse_oakspear1", new PatternData() },

        { "bp_mouse_rosemage0_s", new PatternData() }, // startup
        { "bp_mouse_rosemage0_pt2_s", new PatternData() },
        { "bp_mouse_rosemage0_pt3_s", new PatternData() },
        { "bp_mouse_rosemage0_pt4_s", new PatternData() },
        { "bp_mouse_rosemage0_pt5_s", new PatternData() },
        { "bp_mouse_rosemage0_pt6_s", new PatternData() },
        { "bp_mouse_rosemage0_pt7_s", new PatternData() },
        { "bp_mouse_commander0_s", new PatternData() }, // startup
        { "bp_mouse_commander0_pt2_s", new PatternData() },
        { "bp_mouse_commander0_pt3_s", new PatternData() },
        { "bp_mouse_commander0_pt4_s", new PatternData() },
        { "bp_mouse_commander0_pt5_s", new PatternData() },
        { "bp_mouse_commander0_pt6_s", new PatternData() },
        { "bp_mouse_commander0_pt7_s", new PatternData() },

        { "bp_mouse_rosemage0", new PatternData() }, // startup
        { "bp_mouse_rosemage0_pt2", new PatternData() },
        { "bp_mouse_rosemage0_pt3", new PatternData() },
        { "bp_mouse_rosemage0_pt4", new PatternData() },
        { "bp_mouse_rosemage0_pt5", new PatternData() },
        { "bp_mouse_rosemage0_pt6", new PatternData() },
        { "bp_mouse_rosemage0_pt7", new PatternData() },
        { "bp_mouse_commander0", new PatternData() }, // startup
        { "bp_mouse_commander0_pt2", new PatternData() },
        { "bp_mouse_commander0_pt3", new PatternData() },
        { "bp_mouse_commander0_pt4", new PatternData() },
        { "bp_mouse_commander0_pt5", new PatternData() },
        { "bp_mouse_commander0_pt6", new PatternData() },
        { "bp_mouse_commander0_pt7", new PatternData() },

        { "bp_mouse_paladin0_s", new PatternData() }, // startup
        { "bp_mouse_paladin0_pt2_s", new PatternData() },
        { "bp_mouse_paladin0_pt3_s", new PatternData() },
        { "bp_mouse_paladin0_pt4_s", new PatternData() },
        { "bp_mouse_paladin0_pt5_s", new PatternData() },
        { "bp_mouse_paladin0_pt6_s", new PatternData() },
        { "bp_mouse_paladin0_pt7_s", new PatternData() },
        { "bp_mouse_paladin0_pt8_s", new PatternData() },
        { "bp_mouse_paladin0_pt4_sl", new PatternData() }, // weirdo

        { "bp_mouse_paladin0", new PatternData() }, // startup
        { "bp_mouse_paladin0_pt2", new PatternData() },
        { "bp_mouse_paladin0_pt3", new PatternData() },
        { "bp_mouse_paladin0_pt4", new PatternData() },
        { "bp_mouse_paladin0_pt5", new PatternData() },
        { "bp_mouse_paladin0_pt6", new PatternData() },
        { "bp_mouse_paladin0_pt7", new PatternData() },
        { "bp_mouse_paladin0_pt8", new PatternData() },

        { "bp_mouse_paladin1_s", new PatternData() }, // startup
        { "bp_mouse_paladin1_pt2_s", new PatternData() },
        { "bp_mouse_paladin1_pt3_s", new PatternData() },
        { "bp_mouse_paladin1_pt4_s", new PatternData() },
        { "bp_mouse_paladin1_pt5_s", new PatternData() },
        { "bp_mouse_paladin1_pt6_s", new PatternData() },
        { "bp_mouse_paladin1_pt7_s", new PatternData() },
        { "bp_mouse_paladin1_pt8_s", new PatternData() },
        { "bp_mouse_paladin1_pt9_s", new PatternData() },

        { "bp_mouse_paladin1", new PatternData() }, // startup
        { "bp_mouse_paladin1_pt2", new PatternData() },
        { "bp_mouse_paladin1_pt3", new PatternData() },
        { "bp_mouse_paladin1_pt4", new PatternData() },
        { "bp_mouse_paladin1_pt5", new PatternData() },
        { "bp_mouse_paladin1_pt6", new PatternData() },
        { "bp_mouse_paladin1_pt7", new PatternData() },
        { "bp_mouse_paladin1_pt8", new PatternData() },
        { "bp_mouse_paladin1_pt9", new PatternData() },
        // { "bp_mouse_paladin1_repeat", new PatternData() }, // idk what this bp does

        // lakeside
        { "bp_frog_seamstress0_s", new PatternData() },
        { "bp_frog_seamstress1_s", new PatternData() },
        { "bp_frog_songstress0_s", new PatternData() },
        { "bp_frog_songstress1_s", new PatternData() },
        { "bp_frog_musician0_s", new PatternData() },
        { "bp_frog_musician1_s", new PatternData() },

        { "bp_frog_seamstress0", new PatternData() },
        { "bp_frog_seamstress1", new PatternData() },
        { "bp_frog_songstress0", new PatternData() },
        { "bp_frog_songstress1", new PatternData() },
        { "bp_frog_musician0", new PatternData() },
        { "bp_frog_musician1", new PatternData() },
        { "bp_frog_seamstress1_h", new PatternData() }, // weirdo

        { "bp_frog_painter0_s", new PatternData() }, // startup
        { "bp_frog_painter0_pt2_s", new PatternData() },
        { "bp_frog_painter0_pt3_s", new PatternData() },
        { "bp_frog_painter0_pt4_s", new PatternData() },
        { "bp_frog_painter0_pt5_s", new PatternData() },
        { "bp_frog_painter0_pt6_s", new PatternData() },
        { "bp_frog_painter0_pt7_s", new PatternData() },
        { "bp_frog_painter0_pt3_sl", new PatternData() }, // weirdo

        { "bp_frog_painter0", new PatternData() }, // startup
        { "bp_frog_painter0_pt2", new PatternData() },
        { "bp_frog_painter0_pt3", new PatternData() },
        { "bp_frog_painter0_pt4", new PatternData() },
        { "bp_frog_painter0_pt5", new PatternData() },
        { "bp_frog_painter0_pt6", new PatternData() },
        { "bp_frog_painter0_pt7", new PatternData() },
        { "bp_frog_painter0_pt3_l", new PatternData() }, // weirdo

        { "bp_frog_idol0_s", new PatternData() }, // startup
        { "bp_frog_idol0_pt2_s", new PatternData() },
        { "bp_frog_idol0_pt3_s", new PatternData() },
        { "bp_frog_idol0_pt4_s", new PatternData() },
        { "bp_frog_idol0_pt5_s", new PatternData() },
        { "bp_frog_idol0_pt6_s", new PatternData() },
        { "bp_frog_idol0_pt7_s", new PatternData() },

        { "bp_frog_idol0", new PatternData() }, // startup
        { "bp_frog_idol0_pt2", new PatternData() },
        { "bp_frog_idol0_pt3", new PatternData() },
        { "bp_frog_idol0_pt4", new PatternData() },
        { "bp_frog_idol0_pt5", new PatternData() },
        { "bp_frog_idol0_pt6", new PatternData() },
        { "bp_frog_idol0_pt7", new PatternData() },

        { "bp_frog_idol1_s", new PatternData() }, // startup
        { "bp_frog_idol1_pt2_s", new PatternData() },
        { "bp_frog_idol1_pt3_s", new PatternData() },
        { "bp_frog_idol1_pt4_s", new PatternData() },
        { "bp_frog_idol1_pt5_s", new PatternData() },
        { "bp_frog_idol1_pt6_s", new PatternData() },
        { "bp_frog_idol1_pt7_s", new PatternData() },
        { "bp_frog_idol1_pt8_s", new PatternData() },
        { "bp_frog_idol1_pt9_s", new PatternData() },

        { "bp_frog_idol1", new PatternData() }, // startup
        { "bp_frog_idol1_pt2", new PatternData() },
        { "bp_frog_idol1_pt3", new PatternData() },
        { "bp_frog_idol1_pt4", new PatternData() },
        { "bp_frog_idol1_pt5", new PatternData() },
        { "bp_frog_idol1_pt6", new PatternData() },
        { "bp_frog_idol1_pt7", new PatternData() },
        { "bp_frog_idol1_pt8", new PatternData() },
        { "bp_frog_idol1_pt9", new PatternData() },
        { "bp_frog_idol1_pt7_h", new PatternData() }, // weirdo

        // keep
        { "bp_queens_axe0_s", new PatternData() },
        { "bp_queens_harp0_s", new PatternData() },
        { "bp_queens_knife_l0_s", new PatternData() },
        { "bp_queens_knife_r0_s", new PatternData() },
        { "bp_queens_shield0_s", new PatternData() },
        { "bp_queens_spear0_s", new PatternData() },
        { "bp_queens_staff0_s", new PatternData() },

        { "bp_queens_axe0", new PatternData() },
        { "bp_queens_harp0", new PatternData() },
        { "bp_queens_knife_l0", new PatternData() },
        { "bp_queens_knife_r0", new PatternData() },
        { "bp_queens_shield0", new PatternData() },
        { "bp_queens_spear0", new PatternData() },
        { "bp_queens_staff0", new PatternData() },

        // pinnacle
        { "bp_rabbit_queen0_s", new PatternData() }, // startup
        { "bp_rabbit_queen0_pt2_s", new PatternData() },
        { "bp_rabbit_queen0_pt3_s", new PatternData() },
        { "bp_rabbit_queen0_pt4_s", new PatternData() },
        { "bp_rabbit_queen0_pt5_s", new PatternData() },
        { "bp_rabbit_queen0_pt6_s", new PatternData() },
        { "bp_rabbit_queen0_pt7_s", new PatternData() },

        { "bp_rabbit_queen0", new PatternData() }, // startup
        { "bp_rabbit_queen0_pt2", new PatternData() },
        { "bp_rabbit_queen0_pt3", new PatternData() },
        { "bp_rabbit_queen0_pt4", new PatternData() },
        { "bp_rabbit_queen0_pt5", new PatternData() },
        { "bp_rabbit_queen0_pt6", new PatternData() },
        { "bp_rabbit_queen0_pt7", new PatternData() },

        { "bp_rabbit_queen1_s", new PatternData() }, // startup
        { "bp_rabbit_queen1_pt2_s", new PatternData() },
        { "bp_rabbit_queen1_pt3_s", new PatternData() },
        { "bp_rabbit_queen1_pt4_s", new PatternData() },
        { "bp_rabbit_queen1_pt5_s", new PatternData() },
        { "bp_rabbit_queen1_pt6_s", new PatternData() },
        { "bp_rabbit_queen1_pt7_s", new PatternData() },
        { "bp_rabbit_queen1_pt8_s", new PatternData() },
        { "bp_rabbit_queen1_pt9_s", new PatternData() },

        { "bp_rabbit_queen1", new PatternData() }, // startup
        { "bp_rabbit_queen1_pt2", new PatternData() },
        { "bp_rabbit_queen1_pt3", new PatternData() },
        { "bp_rabbit_queen1_pt4", new PatternData() },
        { "bp_rabbit_queen1_pt5", new PatternData() },
        { "bp_rabbit_queen1_pt6", new PatternData() },
        { "bp_rabbit_queen1_pt7", new PatternData() },
        { "bp_rabbit_queen1_pt8", new PatternData() },
        { "bp_rabbit_queen1_pt9", new PatternData() },

    };

    private static Dictionary<string, EnemyData> EnemyMap = new Dictionary<string, EnemyData> {
        // enemy to stage, zoom, basic, anims
        // tutorial (data has not been test. Zoom and basic could be off)
        { "bird_sophomore0", new EnemyData() },
        { "wolf_blackear0", new EnemyData() },
        { "dragon_granite0", new EnemyData() },
        { "mouse_cadet0", new EnemyData(zoom: 0.9) },
        { "frog_tinkerer0", new EnemyData() },

        // outskirts
        { "bird_sophomore1", new EnemyData() },
        { "bird_sophomore2", new EnemyData() },
        { "wolf_blackear1", new EnemyData() },
        { "wolf_blackear2", new EnemyData() },
        { "dragon_granite1", new EnemyData() },
        { "dragon_granite2", new EnemyData() },
        { "mouse_cadet1", new EnemyData(zoom: 0.9) },
        { "mouse_cadet2", new EnemyData(zoom: 0.9) },
        { "frog_tinkerer1", new EnemyData() },
        { "frog_tinkerer2", new EnemyData() },

        // nest
        // azel has a zoom exception, checked on pattern level (UNIMPLEMENTED)
        { "bird_student0", new EnemyData(Stage.NEST, 0.8) },
        { "bird_student1", new EnemyData(Stage.NEST) },
        { "bird_whispering0", new EnemyData(Stage.NEST, 0.9) }, // exception
        { "bird_whispering1", new EnemyData(Stage.NEST, 0.9) },
        { "bird_archon0", new EnemyData(Stage.NEST, 0.9, false) },
        { "bird_valedictorian0", new EnemyData(Stage.NEST, 0.9, false, Anims.Twili) },
        { "bird_valedictorian1", new EnemyData(Stage.NEST, 0.85, false, Anims.Twili) },

        // arsenal
        { "wolf_greyeye0", new EnemyData(Stage.ARSENAL, 0.8) },
        { "wolf_greyeye1", new EnemyData(Stage.ARSENAL, 0.8) },
        { "wolf_bluepaw0", new EnemyData(Stage.ARSENAL, 0.9) },
        { "wolf_bluepaw1", new EnemyData(Stage.ARSENAL, 0.9) },
        { "wolf_snowfur0", new EnemyData(Stage.ARSENAL, 0.9, false, Anims.Tassha) },
        { "wolf_steeltooth0", new EnemyData(Stage.ARSENAL, 0.9, false, Anims.Merran) },
        { "wolf_steeltooth1", new EnemyData(Stage.ARSENAL, 0.75, false, Anims.Merran) },

        // lighthouse
        // Karsi transformation is (UNIMPLEMENTED)
        { "dragon_gold0", new EnemyData(Stage.LIGHTHOUSE) },
        { "dragon_gold1", new EnemyData(Stage.LIGHTHOUSE) },
        { "dragon_emerald0", new EnemyData(Stage.LIGHTHOUSE, 0.95) },
        { "dragon_emerald1", new EnemyData(Stage.LIGHTHOUSE, 0.95) },
        { "dragon_ruby0", new EnemyData(Stage.LIGHTHOUSE, 0.8, false, Anims.Karsi) },
        { "dragon_mythril0", new EnemyData(Stage.LIGHTHOUSE, 0.9, false, Anims.Ranalie) },
        { "dragon_mythril1", new EnemyData(Stage.LIGHTHOUSE, 0.7, false, Anims.Ranalie) },

        // streets
        { "mouse_archer0", new EnemyData(Stage.STREETS, 0.8) },
        { "mouse_archer1", new EnemyData(Stage.STREETS, 0.8) },
        { "mouse_oakspear0", new EnemyData(Stage.STREETS, 0.9) },
        { "mouse_oakspear1", new EnemyData(Stage.STREETS, 0.9) },
        { "mouse_rosemage0", new EnemyData(Stage.STREETS, 0.8, false) },
        { "mouse_paladin0", new EnemyData(Stage.STREETS, 0.9, false, Anims.Matti) },
        { "mouse_paladin1", new EnemyData(Stage.STREETS, 0.8, false, Anims.Matti) },

        // lakeside
        { "frog_seamstress0", new EnemyData(Stage.LAKESIDE, 0.9) },
        { "frog_seamstress1", new EnemyData(Stage.LAKESIDE, 0.9) },
        { "frog_songstress0", new EnemyData(Stage.LAKESIDE, 0.9) },
        { "frog_songstress1", new EnemyData(Stage.LAKESIDE, 0.9) },
        { "frog_painter0", new EnemyData(Stage.LAKESIDE, 0.9, false) },
        { "frog_idol0", new EnemyData(Stage.LAKESIDE, 0.85, false, Anims.Avy) },
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
}

public class BattleData2 {

    /*    BattleData.GetTotalLength
        bd.GetLengthByPattern
        GetEnemy -> "bird_student_0"
    */ //UNIMPLEMENTED

    public static void ReadConfig(Config.Config config) {
        name = Enum.GetName(config.ActivePattern) ?? "";
        // DO NOT FORGET ABOUT CROW EXCEPTIONS
        //UNIMPLEMENTED
    }

    public static string name { get; set; } // ex: Menna0_S
    public static string pattern { get; set; } // ex: bp_bird_student0_s
    public static string enemy { get; set; } // ex: bird_student0
    public static bool basic { get; set; } // ex: true
    public static bool multi { get; set; } // ex: true
    public static int length { get; set; } // ex: 15000
    public static double zoom { get; set; } // ex: 1
    public static int stage { get; set; } // ex: 2
    public static Anims anim { get; set; } // ex: Anims.None
    public static Mixes mix { get; set; } // ex: Mixes.None

    static BattleData2() {
        name = "";
        pattern = "";
        enemy = "";
        basic = false;
        multi = false;
        length = 0;
        zoom = 0;
        stage = 0;
        anim = Anims.None;
        mix = Mixes.None;
    }

    public static void Default() {
        name = "";
        pattern = "";
        enemy = "";
        basic = false;
        multi = false;
        length = 0;
        zoom = 0;
        stage = 0;
        anim = Anims.None;
        mix = Mixes.None;

    }
}
