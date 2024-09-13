using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using static RNSReloaded.HyperbolicPlus.Config.Config;

namespace RNSReloaded.HyperbolicPlus;


public class BattleData {
    public static void ReadConfig(Config.Config config) {
        name = Enum.GetName(config.ActivePattern) ?? "";
        // DO NOT FORGET ABOUT CROW EXCEPTIONS
        // UNIMPLEMENTED
        pattern = BDLookup.NameMap[name];
        Console.WriteLine(pattern);

        // read pattern data
        BDLookup.PatternData pd = BDLookup.PatternMap[pattern];
        enemy = pd.enemy;
        multi = pd.multi;
        length = pd.length;
        partner = pd.partner;
        mix = pd.mixes;
        Console.WriteLine(enemy);

        // read enemy data
        BDLookup.EnemyData ed = BDLookup.EnemyMap[enemy];
        basic = ed.basic;
        stage = ed.stage;
        zoom = ed.zoom;
        anim = ed.anims;
        Console.WriteLine(zoom);
    }

    public static string name { get; set; } // ex: Menna0_S
    public static string pattern { get; set; } // ex: bp_bird_student0_s
    public static string enemy { get; set; } // ex: bird_student0
    public static string? partner { get; set; } // name of paired battle pattern
    public static bool basic { get; set; } // whether enemy isn't a multiphase fight or not
    public static bool multi { get; set; } // whether the encounter is multi or solo
    public static int length { get; set; } // length of pattern in ms
    public static double zoom { get; set; } // ex: 1.0
    public static int stage { get; set; } // ex: 2
    public static Anims anim { get; set; } // ex: Anims.None
    public static Mixes mix { get; set; } // ex: Mixes.None

    static BattleData() {
        name = "";
        pattern = "";
        enemy = "";
        partner = null;
        basic = false;
        multi = false;
        length = 20000;
        zoom = 1;
        stage = 1;
        anim = Anims.None;
        mix = Mixes.None;
    }

    public static void Default() {
        name = "";
        pattern = "";
        enemy = "";
        partner = null;
        basic = false;
        multi = false;
        length = 20000;
        zoom = 1;
        stage = 1;
        anim = Anims.None;
        mix = Mixes.None;
    }

    public static string GetRealPattern() {
        // to account for the named difficulty cases
        return pattern; // UNIMPLEMENTED
    } // GetEnemy -> "bird_student_0"

    public static int GetTotalLength(List<string> patterns) {
        return patterns.Sum(pattern => BDLookup.PatternMap[pattern].length);
    }

    public static int GetLengthByPattern(string pattern) {
        return BDLookup.PatternMap[pattern].length;
    }

    public static List<string> GetPatternsByMix(Mixes mix) {
        return BDLookup.MixMap[mix];
    }

    public static void PrintData() {
        Console.WriteLine($"PRINTING STORED BATTLEDATA");
        Console.WriteLine($"\tName: {name}");
        Console.WriteLine($"\tPattern: {pattern}");
        Console.WriteLine($"\tEnemy: {enemy}");
        Console.WriteLine($"\tPartner: {pattern}");
        Console.WriteLine($"\tBasic: {basic}");
        Console.WriteLine($"\tMulti: {multi}");
        Console.WriteLine($"\tLength: {length}");
        Console.WriteLine($"\tZoom: {zoom}");
        Console.WriteLine($"\tStage: {stage}");
        Console.WriteLine($"\tAnim: {anim}");
        Console.WriteLine($"\tMix: {mix}");
    }
}
