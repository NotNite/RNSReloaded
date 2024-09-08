using System;
using System.Collections.Generic;
using System.Linq;

namespace RNSReloaded.HyperbolicPlus;

public class BattleData {
    private class EnemyStage {
        /*  1 Outskirts
            2 Nest
            3 Arsenal
            4 Lighthouse
            5 Streets
            6 Lakeside
            7 Keep
            8 Pinnacle */

        private static Dictionary<string, EnemyStage> StageDict = new Dictionary<string, EnemyStage> {
            // nest
            { "bird_valedictorian0", new EnemyStage(2, [0.9, 0.85]) },
            { "bird_valedictorian1", new EnemyStage(2, [0.9, 0.85]) },

            // arsenal
            { "wolf_snowfur0", new EnemyStage(3, [0.9, 0.9]) },
            { "wolf_steeltooth0", new EnemyStage(3, [0.9, 0.9]) },
            { "wolf_steeltooth1", new EnemyStage(3, [0.75, 0.75]) },
            // lighthouse
            { "dragon_emerald1", new EnemyStage(4, [0.9, 0.9]) }, // unzoomed
            { "dragon_ruby0", new EnemyStage(4, [0.8, 0.8]) },
            
            { "dragon_mythril0", new EnemyStage(4, [0.9, 0.9]) },
            { "dragon_mythril1", new EnemyStage(4, [0.7, 0.7])},
            // streets
            { "mouse_rosemage0", new EnemyStage(5, [1, 1])}, // unzoomed
            { "mouse_paladin0", new EnemyStage(5, [0.9, 0.9])},
            // lakeside
            { "frog_painter0", new EnemyStage(6, [0.9, 0.9]) }, // unzoomed
            { "frog_idol1", new EnemyStage(6, [0.85, 0.85]) },
            // keep
            { "queens_harp0", new EnemyStage(7, [1, 1])}, // unzoomed
            // pinnacle
            { "rabbit_queen1", new EnemyStage(8, [0.75, 0.75])}, // solo zoom

            { "bird_sophomore1", new EnemyStage(1, [1, 1])}
        };

        private int stage { get; set; }
        private double[] zoom { get; set; }

        public EnemyStage(int stage, double[] zoom) {
            this.stage = stage;
            this.zoom = zoom;
        }

        public static int GetStage(string name) { return EnemyStage.StageDict[name].stage; }
        public static double GetZoom(string name, bool multi) { return EnemyStage.StageDict[name].zoom[multi ? 1 : 0]; }
    }

    private static Dictionary<string, int> LengthDict = new Dictionary<string, int> {
        // NEST
        // solo
        { "bp_bird_valedictorian0_pt6_s", 20000 },
        { "bp_bird_valedictorian0_pt7_s", 20000 },

        // ARSENAL
        // solo
        { "bp_wolf_snowfur0_pt4_s", 16000 },
        { "bp_wolf_steeltooth0_pt6_s", 16000 }, // untimed
        // multi
        { "bp_wolf_snowfur0_pt3", 13000 },
        { "bp_wolf_snowfur0_pt4", 16000 },
        { "bp_wolf_snowfur0_pt5", 20000 },

        { "bp_wolf_steeltooth1_pt2", 21000 },
        { "bp_wolf_steeltooth1_pt3", 12500 },
        { "bp_wolf_steeltooth1_pt4", 20000 },
        { "bp_wolf_steeltooth1_pt5", 12500 },
        { "bp_wolf_steeltooth1_pt6", 22000 },
        { "bp_wolf_steeltooth1_pt7", 12500 },
        { "bp_wolf_steeltooth1_pt8", 21000 },
        { "bp_wolf_steeltooth1_pt9", 12500 },
        
        // LIGHTHOUSE
        // solo
        { "bp_dragon_emerald1_s", 12500 },
        { "bp_dragon_ruby0_pt7_s", 12500 },

        { "bp_dragon_mythril0_pt2_s", 12500 },
        { "bp_dragon_mythril0_pt3_s", 10000 },
        { "bp_dragon_mythril0_pt4_s", 15000 },
        { "bp_dragon_mythril0_pt5_s", 10000 },
        { "bp_dragon_mythril0_pt6_s", 13000 },
        { "bp_dragon_mythril0_pt7_s", 16000 },
        { "bp_dragon_mythril0_pt8_s", 14000 },
        { "bp_dragon_mythril1_pt4_s", 12000},
        // multi
        { "bp_dragon_mythril0_pt6", 15000 },
        { "bp_dragon_mythril0_pt7", 15000},
        { "bp_dragon_mythril1_pt3", 22000},
        { "bp_dragon_mythril1_pt4", 15000},
        { "bp_mouse_rosemage0_pt2_s", 15000},

        { "bp_mouse_paladin0_pt7_s", 20000}, //untimed
        { "bp_mouse_paladin0_pt8_s", 20000},
        
        // LAKESIDE
        { "bp_frog_painter0_pt6_s", 20000},
        { "bp_frog_painter0_pt7_s", 24000},

        // solo
        { "bp_frog_idol1_pt2_s", 18000},
        { "bp_frog_idol1_pt3_s", 16000},
        { "bp_frog_idol1_pt4_s", 21000},
        { "bp_frog_idol1_pt5_s", 18000},
        { "bp_frog_idol1_pt6_s", 16000},
        { "bp_frog_idol1_pt7_s", 17000},
        { "bp_frog_idol1_pt8_s", 17000},
        { "bp_frog_idol1_pt9_s", 20000},


        // multi
        { "bp_frog_painter0_pt2", 15000}, // untimed
        { "bp_frog_painter0_pt3", 15000}, // untimed
        { "bp_frog_painter0_pt4", 15000}, // untimed
        { "bp_frog_painter0_pt5", 15000}, // untimed
        { "bp_frog_painter0_pt6", 15000}, // untimed
        { "bp_frog_painter0_pt7", 24000}, // untimed //

        // KEEP
        { "bp_queens_harp0_s", 15000},

        // PINNACLE
        { "bp_rabbit_queen1_pt2_s", 20000}, 
        { "bp_rabbit_queen1_pt3_s", 20000}, // baited attack. exclude from pvp
        { "bp_rabbit_queen1_pt4_s", 16000}, 
        { "bp_rabbit_queen1_pt5_s", 19000}, // steel 2
        { "bp_rabbit_queen1_pt6_s", 20000}, 
        { "bp_rabbit_queen1_pt7_s", 15000}, // steel 3
        { "bp_rabbit_queen1_pt8_s", 20000}, 
        { "bp_rabbit_queen1_pt9_s", 15000}, // just an enrage, exclude

        { "bp_rabbit_queen1_pt5", 19000}, // steel 2
        { "bp_rabbit_queen1_pt6", 20000},

        { "bp_bird_sophomore1_s", 20000}
    };

    private static Dictionary<string, BattleData> DataDict = new Dictionary<string, BattleData> {
        { "Twili1_6S", new BattleData("bird_valedictorian", "bp_bird_valedictorian0_pt6_s") },
        { "Twili1_7S", new BattleData("bird_valedictorian", "bp_bird_valedictorian0_pt7_s") },
        
        // ARSENAL
        // solo
        { "Tassha_4S", new BattleData("wolf_snowfur0", "bp_wolf_snowfur0_pt4_s", false, Mixes.None, Anims.Tassha) },
        { "Merran1_6S", new BattleData("wolf_steeltooth0", "bp_wolf_steeltooth0_pt6_s", false, Mixes.None, Anims.Tassha) },

        // multi
        { "Tassha_3M", new BattleData("wolf_snowfur0", "bp_wolf_snowfur0_pt3", true, Mixes.None, Anims.Tassha) },
        { "Tassha_4M", new BattleData("wolf_snowfur0", "bp_wolf_snowfur0_pt4", true, Mixes.None, Anims.Tassha) },
        { "Tassha_5M", new BattleData("wolf_snowfur0", "bp_wolf_snowfur0_pt5", true, Mixes.None, Anims.Tassha) },

        { "Merran2_2M", new BattleData("wolf_steeltooth1", "bp_wolf_steeltooth1_pt2", true, Mixes.None, Anims.Merran) }, // timeslows may not work properly
        { "Merran2_3M", new BattleData("wolf_steeltooth1", "bp_wolf_steeltooth1_pt3", true,  Mixes.None, Anims.Merran) },
        { "Merran2_4M", new BattleData("wolf_steeltooth1", "bp_wolf_steeltooth1_pt4", true,  Mixes.None, Anims.Merran) },
        { "Merran2_5M", new BattleData("wolf_steeltooth1", "bp_wolf_steeltooth1_pt5", true,  Mixes.None, Anims.Merran) },
        { "Merran2_6M", new BattleData("wolf_steeltooth1", "bp_wolf_steeltooth1_pt6", true,  Mixes.None, Anims.Merran) },
        { "Merran2_7M", new BattleData("wolf_steeltooth1", "bp_wolf_steeltooth1_pt7", true,  Mixes.None, Anims.Merran) },
        { "Merran2_8M", new BattleData("wolf_steeltooth1", "bp_wolf_steeltooth1_pt8", true,  Mixes.None, Anims.Merran) },
        { "Merran2_9M", new BattleData("wolf_steeltooth1", "bp_wolf_steeltooth1_pt9", true,  Mixes.None, Anims.Merran) },

        // LIGHTHOUSE
        // solo
        { "Illie2_S", new BattleData("dragon_emerald1", "bp_dragon_emerald1_s") },
        { "Karsi_7S", new BattleData("dragon_ruby0", "bp_dragon_ruby0_pt7_s") },

        { "Ranalie1_2S", new BattleData("dragon_mythril0", "bp_dragon_mythril0_pt2_s") }, // expanding lasers
        { "Ranalie1_3S", new BattleData("dragon_mythril0", "bp_dragon_mythril0_pt3_s") }, // safe knockback
        { "Ranalie1_4S", new BattleData("dragon_mythril0", "bp_dragon_mythril0_pt4_s") }, // death
        { "Ranalie1_5S", new BattleData("dragon_mythril0", "bp_dragon_mythril0_pt5_s") }, 
        { "Ranalie1_6S", new BattleData("dragon_mythril0", "bp_dragon_mythril0_pt6_s") }, // Y lasers
        { "Ranalie1_7S", new BattleData("dragon_mythril0", "bp_dragon_mythril0_pt7_s") }, // flaremill
        { "Ranalie1_8S", new BattleData("dragon_mythril0", "bp_dragon_mythril0_pt8_s") }, // expanding rotation
        
        { "Ranalie2_4S", new BattleData("dragon_mythril1", "bp_dragon_mythril1_pt4_s", false, Mixes.None, Anims.Ranalie) },

        // multi
        { "Ranalie1_6M", new BattleData("dragon_mythril1", "bp_dragon_mythril0_pt6", true) },
        { "Ranalie1_7M", new BattleData("dragon_mythril1", "bp_dragon_mythril0_pt7", true) },
        { "Ranalie2_3M", new BattleData("dragon_mythril1", "bp_dragon_mythril1_pt3", true,  Mixes.None, Anims.Ranalie) },
        { "Ranalie2_4M", new BattleData("dragon_mythril1", "bp_dragon_mythril1_pt4", true,  Mixes.None, Anims.Ranalie) },

        // STREETS
        { "Matti1_7S", new BattleData("mouse_paladin0", "bp_mouse_paladin0_pt7_s") },
        { "Matti1_8S", new BattleData("mouse_paladin0", "bp_mouse_paladin0_pt8_s") },

        // LAKESIDE
        { "Blush_6S", new BattleData("frog_painter0", "bp_frog_painter0_pt6_s") },
        { "Blush_7S", new BattleData("frog_painter0", "bp_frog_painter0_pt7_s") },
        
        { "Blush_2M", new BattleData("frog_painter0", "bp_frog_painter0_pt2", true) },
        { "Blush_3M", new BattleData("frog_painter0", "bp_frog_painter0_pt3", true) },
        { "Blush_4M", new BattleData("frog_painter0", "bp_frog_painter0_pt4", true) },
        { "Blush_5M", new BattleData("frog_painter0", "bp_frog_painter0_pt5", true) },
        { "Blush_6M", new BattleData("frog_painter0", "bp_frog_painter0_pt6", true) },
        { "Blush_7M", new BattleData("frog_painter0", "bp_frog_painter0_pt7", true) },
        
        { "Avy2_2S", new BattleData("frog_idol1", "bp_frog_idol1_pt2_s", false,  Mixes.None, Anims.Avy) },
        { "Avy2_3S", new BattleData("frog_idol1", "bp_frog_idol1_pt3_s", false,  Mixes.None, Anims.Avy) },
        { "Avy2_4S", new BattleData("frog_idol1", "bp_frog_idol1_pt4_s", false,  Mixes.None, Anims.Avy) },
        { "Avy2_5S", new BattleData("frog_idol1", "bp_frog_idol1_pt5_s", false,  Mixes.None, Anims.Avy) },
        { "Avy2_6S", new BattleData("frog_idol1", "bp_frog_idol1_pt6_s", false,  Mixes.None, Anims.Avy) },
        { "Avy2_7S", new BattleData("frog_idol1", "bp_frog_idol1_pt7_s", false,  Mixes.None, Anims.Avy) },
        { "Avy2_8S", new BattleData("frog_idol1", "bp_frog_idol1_pt8_s", false,  Mixes.None, Anims.Avy) },
        { "Avy2_9S", new BattleData("frog_idol1", "bp_frog_idol1_pt9_s", false,  Mixes.None, Anims.Avy) },

        // PINNACLE
        { "Shira2_2S", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt2_s", false, Mixes.None, Anims.Shira) },
        { "Shira2_3S", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt3_s", false,  Mixes.None, Anims.Shira) },
        { "Shira2_4S", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt4_s", false,  Mixes.None, Anims.Shira) },
        { "Shira2_5S", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt5_s", false,  Mixes.None, Anims.Shira) },
        { "Shira2_6S", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt6_s", false,  Mixes.None, Anims.Shira) },
        { "Shira2_7S", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt7_s", false,  Mixes.None, Anims.Shira) },
        { "Shira2_8S", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt8_s", false,  Mixes.None, Anims.Shira) },
        { "Shira2_9S", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt9_s", false,  Mixes.None, Anims.Shira) },

        { "Shira2_5M", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt5", true, Mixes.None, Anims.Shira) },
        { "Shira2_6M", new BattleData("rabbit_queen1", "bp_rabbit_queen1_pt6", true, Mixes.None, Anims.Shira) },

        // EXTRA
        { "Bella1_S", new BattleData("bird_sophomore1", "bp_bird_sophomore1_s") },
        { "MegVaro_2S", new BattleData("mouse_rosemage0", "bp_mouse_rosemage0_pt2_s") }, // broken
        { "Harp_S", new BattleData("queens_harp0", "bp_queens_harp0_s") }, // broken

        // MIXES
        // currently, baited attacks are left out
        { "Ranalie1_Mix_S", new BattleData("dragon_mythril0", "", false, Mixes.Ranalie1_S, Anims.None) },
        { "Avy2_Mix_S", new BattleData("frog_idol1", "", false, Mixes.Avy2_S, Anims.Avy) },
        { "Shira2_Mix_S", new BattleData("rabbit_queen1", "", false, Mixes.Shira2_S, Anims.Shira) }
    };

    public string enemy { get; set; }
    public string pattern { get; set; }
    public bool multi { get; set; }
    public Mixes mix { get; set; }
    public Anims anim { get; set; }

    public BattleData(string enemy, string pattern, bool multi=false, Mixes mix=Mixes.None, Anims anim=Anims.None) {
        this.enemy = enemy;
        this.pattern = pattern;
        this.mix = mix;
        this.anim = anim;
    }

    public BattleData(string enemy, string pattern) {
        this.enemy = enemy;
        this.pattern = pattern;
        this.mix = Mixes.None;
        this.anim = Anims.None;
    }

    public static string GetEnemy(string name) {
        if (BattleData.DataDict.TryGetValue(name, out var data)) {
            return data.enemy;
        }
        Console.WriteLine("Error in getting enemy of " + name);
        return "";
    }

    public static string GetPattern(string name) {
        if (BattleData.DataDict.TryGetValue(name, out var data)) {
            return data.pattern;
        }
        Console.WriteLine("Error in getting pattern of " + name);
        return "";
    }

    public static bool GetMulti(string name) {
        if (BattleData.DataDict.TryGetValue(name, out var data)) {
            return data.multi;
        }
        Console.WriteLine("Error in getting multi of " + name);
        return false;
    }

    public static int GetStage(string name) {
        var enemy = GetEnemy(name);
        if (enemy != null) {
            return BattleData.EnemyStage.GetStage(enemy);
        }
        return 0;
    }

    public static double GetZoom(string name) {
        // zoom is dependent on solo/multi
        var enemy = GetEnemy(name);
        var multi = GetMulti(name);
        if (enemy != null) {
            return BattleData.EnemyStage.GetZoom(enemy, multi);
        }
        return 2;
    }

    public static int GetLength(string name) {
        var pattern = GetPattern(name);
        if (pattern != null && BattleData.LengthDict.TryGetValue(pattern, out var length)) {
            return length;
        }
        Console.WriteLine("Error in getting length of " + name);
        return -1;
    }

    public static int GetLengthByPattern(string pattern) {
        if (pattern != null && BattleData.LengthDict.TryGetValue(pattern, out var length)) {
            return length;
        }
        Console.WriteLine("Error in getting length by pattern of " + pattern);
        return -1;
    }

    public static Mixes GetMix(string name) {
        if (BattleData.DataDict.TryGetValue(name, out var data)) {
            return data.mix;
        }
        Console.WriteLine("Error in getting mix of " + name);
        return Mixes.None;
    }

    public static Anims GetAnim(string name) {
        if (BattleData.DataDict.TryGetValue(name, out var data)) {
            return data.anim;
        }
        Console.WriteLine("Error in getting anim of " + name);
        return Anims.None;
    }

    public static int GetTotalLength(List<string> patterns) {
        return patterns.Sum(pattern => BattleData.GetLengthByPattern(pattern));
    }
}
