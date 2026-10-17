using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Fullmetal.Config;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

/*
    🐀   🐀
🐀           🐀
  "TO ME, MY ALLIES!"
      🎷🐁
🐀           🐀
      🐀
*/

namespace RNSReloaded.Fullmetal;

public unsafe class Mod : IMod {
    private const int SCRIPTCONST = 100000;

    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;
    private Configurator configurator = null!;
    private Config.Config config = null!;
    private Random hallRand = new();

    private string[] route = []; // for background changes
    private string[] hallkeys = [];
    private List<Notch>[] hallNotches = [];
    private int routeType = 0;
    private int hallCount = -1;
    private int notchCount = 0;

    private CInstance* potionSelf;
    private CInstance* potionOther;
    private bool potionStored = false;
    private bool erasingPotions = false;

    private static Dictionary<string, IHook<ScriptDelegate>> ScriptHooks = new();

    public void StartEx(IModLoaderV1 loader, IModConfigV1 modConfig) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>()!;
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        this.logger = loader.GetLogger();

        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.OnReady += this.Ready;
        }

        this.configurator = new Configurator(((IModLoader) loader).GetModConfigDirectory(modConfig.ModId));
        this.config = this.configurator.GetConfiguration<Config.Config>(0);
        this.config.ConfigurationUpdated += this.ConfigurationUpdated;
    }

    private void ConfigurationUpdated(IUpdatableConfigurable newConfig) {
        this.config = (Config.Config) newConfig;
    }

    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out IRNSReloaded? rnsReloaded)
        ) {
            rnsReloaded.LimitOnlinePlay();

            this.InitializeHooks();
        }
    }

    private bool IsReady(
        [MaybeNullWhen(false), NotNullWhen(true)] out IRNSReloaded rnsReloaded,
        [MaybeNullWhen(false), NotNullWhen(true)] out IReloadedHooks hooks,
        [MaybeNullWhen(false), NotNullWhen(true)] out IUtil utils,
        [MaybeNullWhen(false), NotNullWhen(true)] out IBattleScripts scrbp,
        [MaybeNullWhen(false), NotNullWhen(true)] out IBattlePatterns bp
    ) {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out hooks)
        ) {
            utils = rnsReloaded.utils;
            scrbp = rnsReloaded.battleScripts;
            bp = rnsReloaded.battlePatterns;
            return rnsReloaded != null;
        }
        rnsReloaded = null;
        hooks = null;
        utils = null;
        scrbp = null;
        bp = null;
        return false;
    }

    private void CreateAndEnableHook(string scriptName, ScriptDelegate detour, out IHook<ScriptDelegate>? hook) {
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            var script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(scriptName) - SCRIPTCONST);
            hook = hooks.CreateHook(detour, script->Functions->Function);
            hook.Activate();
            hook.Enable();
            return;
        }
        hook = null;
    }

    public void InitializeHooks() {
        var detourMap = new Dictionary<string, ScriptDelegate>{
            { "scr_hallwayprogress_choose_halls", this.ChooseHallsDetour},
            { "scr_hallwayprogress_start_hallway", this.StartHallwayDetour},
            { "scr_hallwayprogress_move_next", this.MoveNextDetour},
            { "scr_rankbar_give_rewards", this.GiveRewardsDetour},
            { "scr_itemsys_erase_potions", this.ErasePotionsDetour},
        };

        string[] hallwaygenScripts = [
            "scr_hallwaygen_outskirts",
            "scr_hallwaygen_outskirts_n",
            "scr_hallwaygen_nest",
            "scr_hallwaygen_arsenal",
            "scr_hallwaygen_lighthouse",
            "scr_hallwaygen_streets",
            "scr_hallwaygen_lakeside",
            "scr_hallwaygen_keep",
            "scr_hallwaygen_pinnacle",
            "scr_hallwaygen_geode",
            "scr_hallwaygen_sanct",
            "scr_hallwaygen_depths",
            "scr_hallwaygen_aurum",
            "scr_hallwaygen_darkhall",
            "scr_hallwaygen_reflection",
            "scr_hallwaygen_chaos_intro",
            "scr_hallwaygen_chaos_mid",
            "scr_hallwaygen_chaos_hallway",
            "scr_hallwaygen_mod",
        ];

        foreach (string script in hallwaygenScripts) {
            detourMap[script] = this.MakeHallwayGenDetour(script);
        }

        foreach (var detourPair in detourMap) {
            this.CreateAndEnableHook(detourPair.Key, detourPair.Value, out var hook);
            if (hook != null) {
                ScriptHooks[detourPair.Key] = hook;
            }
        }
    }

    private int getRandInt() => this.hallRand.Next(0, maxValue: int.MaxValue);

    private static readonly Dictionary<string, string[][]> EncFromKeyMap = new() {
        // first element of value is static enc, not to be randomized
        // subsequent ones are meant to be shuffled
        // BASE GAME AREAS
        {"hw_outskirts", [[],
                ["enc_bird_sophomore1", "enc_bird_sophomore2"],
                ["enc_wolf_blackear1", "enc_wolf_blackear2"],
                ["enc_dragon_granite1", "enc_dragon_granite2"],
                ["enc_mouse_cadet1", "enc_mouse_cadet2"],
                ["enc_frog_tinkerer1", "enc_frog_tinkerer2"],
            ]},
        {"hw_nest", [
                ["enc_bird_archon0", "enc_bird_valedictorian0"],
                ["enc_bird_student0", "enc_bird_student1"],
                ["enc_bird_whispering0", "enc_bird_whispering1"],
            ]},
        {"hw_arsenal", [
                ["enc_wolf_snowfur0", "enc_wolf_steeltooth0"],
                ["enc_wolf_greyeye0", "enc_wolf_greyeye1"],
                ["enc_wolf_bluepaw0", "enc_wolf_bluepaw1"],
            ]},
        {"hw_lighthouse", [
                ["enc_dragon_ruby0", "enc_dragon_mythril0"],
                ["enc_dragon_gold0", "enc_dragon_gold1"],
                ["enc_dragon_emerald0", "enc_dragon_emerald1"],
            ]},
        {"hw_streets", [
                ["enc_mouse_rosemage0", "enc_mouse_paladin0"],
                ["enc_mouse_archer0", "enc_mouse_archer1"],
                ["enc_mouse_oakspear0", "enc_mouse_oakspear1"],
            ]},
        {"hw_lakeside", [
                ["enc_frog_painter0", "enc_frog_idol0"],
                ["enc_frog_seamstress0", "enc_frog_seamstress1"],
                ["enc_frog_songstress0", "enc_frog_songstress1"],
            ]},
        {"hw_keep", [[], [
                "enc_queens_staff0", "enc_queens_knife0",
                "enc_queens_axe0", "enc_queens_spear0",
                "enc_queens_harp0",
            ]]},
        {"hw_pinnacle", [["enc_rabbit_queen0"]]},
        // DLC AREAS
        {"hw_geode", [[],
                ["enc_geode_butterfly0", "enc_geode_butterfly1", "enc_geode_butterfly2"],
                ["enc_geode_moth0", "enc_geode_moth1", "enc_geode_moth2"],
                ["enc_geode_firefly0", "enc_geode_firefly1", "enc_geode_firefly2"],
            ]},
        {"hw_sanct", [
                ["enc_sanct_flower0", "enc_sanct_owl0"],
                ["enc_sanct_capricorn0", "enc_sanct_capricorn1"],
                ["enc_sanct_saph0", "enc_sanct_saph1"],
            ]},
        {"hw_depths", [
                ["enc_depths_angel0", "enc_depths_hound0"],
                ["enc_depths_basilisk0", "enc_depths_basilisk1"],
                ["enc_depths_beast0", "enc_depths_beast1"],
            ]},
        {"hw_aurum", [
                ["enc_aurum_ghost0", "enc_aurum_blackcat0"],
                ["enc_aurum_whitecat0", "enc_aurum_whitecat1"],
                ["enc_aurum_beast0", "enc_aurum_beast1"],
            ]},
        {"hw_darkhall", [[], [
                "enc_darkhall_spelllock0", "enc_darkhall_spelllock1",
                "enc_darkhall_spelllock2", "enc_darkhall_spelllock3",
                "enc_darkhall_spelllock4",
            ]]},
        {"hw_reflection", [["enc_heart_witch0"]]},
    };

    private static readonly string[][] EncChaotic = [
        // outskirts/geode
        [
            "enc_bird_sophomore1", "enc_bird_sophomore2",
            "enc_wolf_blackear1", "enc_wolf_blackear2",
            "enc_dragon_granite1", "enc_dragon_granite2",
            "enc_mouse_cadet1", "enc_mouse_cadet2",
            "enc_frog_tinkerer1", "enc_frog_tinkerer2",
            "enc_geode_butterfly0", "enc_geode_butterfly1", "enc_geode_butterfly2",
            "enc_geode_moth0", "enc_geode_moth1", "enc_geode_moth2",
            "enc_geode_firefly0", "enc_geode_firefly1", "enc_geode_firefly2",
        ],
        // regular encounters
        [
            "enc_bird_student0", "enc_bird_student1",
            "enc_bird_whispering0", "enc_bird_whispering1",
            "enc_wolf_greyeye0", "enc_wolf_greyeye1",
            "enc_wolf_bluepaw0", "enc_wolf_bluepaw1",
            "enc_dragon_gold0", "enc_dragon_gold1",
            "enc_dragon_emerald0", "enc_dragon_emerald1",
            "enc_mouse_archer0", "enc_mouse_archer1",
            "enc_mouse_oakspear0", "enc_mouse_oakspear1",
            "enc_frog_seamstress0", "enc_frog_seamstress1",
            "enc_frog_songstress0", "enc_frog_songstress1",
            "enc_sanct_capricorn0", "enc_sanct_capricorn1",
            "enc_sanct_saph0", "enc_sanct_saph1",
            "enc_depths_basilisk0", "enc_depths_basilisk1",
            "enc_depths_beast0", "enc_depths_beast1",
            "enc_aurum_whitecat0", "enc_aurum_whitecat1",
            "enc_aurum_beast0", "enc_aurum_beast1",
        ],
        // minibosses
        [
            "enc_bird_archon0", "enc_wolf_snowfur0",
            "enc_dragon_ruby0", "enc_mouse_rosemage0",
            "enc_frog_painter0", "enc_sanct_flower0",
            "enc_depths_angel0", "enc_aurum_ghost0",
        ],
        // area bosses
        [
            "enc_bird_valedictorian0", "enc_wolf_steeltooth0",
            "enc_dragon_mythril0", "enc_mouse_paladin0",
            "enc_frog_idol0", "enc_sanct_owl0",
            "enc_depths_hound0", "enc_aurum_blackcat0"
        ],
        // keep/hallway
        [
            "enc_queens_staff0", "enc_queens_knife0",
            "enc_queens_axe0", "enc_queens_spear0",
            "enc_queens_harp0",
            "enc_darkhall_spelllock0", "enc_darkhall_spelllock1",
            "enc_darkhall_spelllock2", "enc_darkhall_spelllock3",
            "enc_darkhall_spelllock4",
        ],
        // final bosses
        ["enc_rabbit_queen0", "enc_heart_witch0"],
    ];

    private string[] getEncFromHallkey(string key) {
        string[][] encList = EncFromKeyMap[key];
        string[] newEnc = encList[0];
        for (uint i = 1; i < encList.Length; i++) {
            this.hallRand.Shuffle(encList[i]);
            newEnc = [.. newEnc, .. encList[i]];
        }
        return newEnc;
    }

    private string[][] getEncChaotic() {
        string[][] encChaotic = EncChaotic;
        foreach (var encs in encChaotic)
            this.hallRand.Shuffle(encs);
        return encChaotic;
    }

    private static class NotchFlag {
        public const int BOSS = 1;           // Regular area boss
        public const int FINAL_BOSS = 2;     // Final boss
        public const int TOXBOX_BOSS = 4;    // Toybox boss
        public const int SHOPKEEP_CAT = 8;   // Asha shop
        public const int SHOPKEEP_WOLF = 16; // Tassha rich wolf!
        public const int SHOPKEEP_BIRD = 32; // Saya shop
        public const int FLOWER_FIGHT = 64;  // Spawns vine deco/background change
        public const int FINALSONG = 128;    // For the credits, play the last CS song
        public const int MUSICFADE = 256;    // For the credits, fade out the music instead after notch
    }

    private static string RouteConfigToHallkey (Config.Config.RouteConfig routeConfig) {
        return routeConfig switch {
            // base
            Config.Config.RouteConfig.ScholarsNest          => "hw_nest",
            Config.Config.RouteConfig.KingsArsenal          => "hw_arsenal",
            Config.Config.RouteConfig.RedDarkhouse          => "hw_lighthouse",
            Config.Config.RouteConfig.ChurchmouseStreets    => "hw_streets",
            Config.Config.RouteConfig.EmeraldLakeside       => "hw_lakeside",
            // dlc
            Config.Config.RouteConfig.SubterraSanctum       => "hw_sanct",
            Config.Config.RouteConfig.DarkhouseDepths       => "hw_depths",
            Config.Config.RouteConfig.AtelierAurum          => "hw_aurum",
            _ => ""
        };
    }

    private List<Notch>[] CreateNotches(Config.Config.RouteConfig route) {
        List<Notch>[] hall = new List<Notch>[7];

        // determine which route type it is (base, dlc, true, chaotic)
        this.routeType = route switch {
            // base game (5 areas)
            Config.Config.RouteConfig.ScholarsNest          or
            Config.Config.RouteConfig.KingsArsenal          or
            Config.Config.RouteConfig.RedDarkhouse          or
            Config.Config.RouteConfig.ChurchmouseStreets    or
            Config.Config.RouteConfig.EmeraldLakeside       or
            Config.Config.RouteConfig.PaleKeep              => 0,
            // dlc (3 areas)
            Config.Config.RouteConfig.SubterraSanctum       or
            Config.Config.RouteConfig.DarkhouseDepths       or
            Config.Config.RouteConfig.AtelierAurum          or
            Config.Config.RouteConfig.LoopingHallway        => 1,
            // random (8 areas)
            Config.Config.RouteConfig.TrueRandom            => 2,
            Config.Config.RouteConfig.ChaoticRandom         => 3,
            _ => -1
        };

        // structure each hall based on route type (since they have different amount of notches)
        string[][] enc = [];
        if (this.routeType == 0) { // BASE GAME / KINGDOM
            // get encounters
            string[] mainAreas = ["hw_nest", "hw_arsenal", "hw_lighthouse", "hw_streets", "hw_lakeside"];
            this.hallRand.Shuffle(mainAreas);
            // choose route
            string start = RouteConfigToHallkey(this.config.Route);
            if (start != "") {
                int index = Array.IndexOf(this.hallkeys, start);
                if (index > 0) {
                    // Swap the first element with the element at the found index
                    string temp = this.hallkeys[0];
                    this.hallkeys[0] = this.hallkeys[index];
                    this.hallkeys[index] = temp;
                }
            }
            // set encounters
            enc = [this.getEncFromHallkey("hw_outskirts")];
            foreach (var area in mainAreas) {
                enc = [.. enc, this.getEncFromHallkey(area)];
            }
            enc = [.. enc, this.getEncFromHallkey("hw_keep")];
            enc = [.. enc, this.getEncFromHallkey("hw_pinnacle")];
            // set hallkeys
            this.route = ["hw_outskirts", .. mainAreas, "hw_keep", "hw_pinnacle"];
            this.hallkeys = ["hw_outskirts", mainAreas[0], mainAreas[2], mainAreas[4], "hw_keep", "hw_pinnacle"];

            // create notches
            hall[0] = [
                new(NotchType.IntroRoom, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][0], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][1], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][5], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][6], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][7], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][8], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][9], this.getRandInt(), 0),
            ];
            hall[1] = [ // first and second area (first hall)
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][0], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Boss, enc[1][1], this.getRandInt(), NotchFlag.BOSS),
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][0], this.getRandInt(), 0),
                new(NotchType.Boss, enc[2][1], this.getRandInt(), NotchFlag.BOSS),
            ];
            hall[2] = [ // third and fourth area (second hall)
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][0], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Boss, enc[3][1], this.getRandInt(), NotchFlag.BOSS),
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][0], this.getRandInt(), 0),
                new(NotchType.Boss, enc[4][1], this.getRandInt(), NotchFlag.BOSS),
            ];
            hall[3] = [ // fifth area (third hall)
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[5][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[5][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[5][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[5][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[5][0], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Boss, enc[5][1], this.getRandInt(), NotchFlag.BOSS),
            ];
            hall[4] = [
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[6][0], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[6][1], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[6][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[6][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[6][4], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
            ];
            hall[5] = [
                new(NotchType.PinnacleCutscene, enc[7][0], this.getRandInt(), 0),
                new(NotchType.FinalBoss, enc[7][0], this.getRandInt(), NotchFlag.BOSS + NotchFlag.FINAL_BOSS),
                new(NotchType.EndRun, "", this.getRandInt(), 0),
            ];

        } else if (this.routeType == 1) { // DLC / UNDERGROUND
            // get encounters
            string[] mainAreas = ["hw_sanct", "hw_depths", "hw_aurum"];
            this.hallRand.Shuffle(mainAreas);
            // choose route
            string start = RouteConfigToHallkey(this.config.Route);
            if (start != "") {
                int index = Array.IndexOf(this.hallkeys, start);
                if (index > 0) {
                    // Swap the first element with the element at the found index
                    string temp = this.hallkeys[0];
                    this.hallkeys[0] = this.hallkeys[index];
                    this.hallkeys[index] = temp;
                }
            }
            // set encounters
            enc = [this.getEncFromHallkey("hw_geode")];
            foreach (var area in mainAreas) {
                enc = [.. enc, this.getEncFromHallkey(area)];
            }
            enc = [.. enc, this.getEncFromHallkey("hw_darkhall")];
            enc = [.. enc, this.getEncFromHallkey("hw_reflection")];
            // set hallkeys
            this.route = ["hw_geode", .. mainAreas, "hw_darkhall", "hw_reflection"];
            this.hallkeys = ["hw_outskirts", .. mainAreas, "hw_darkhall", "hw_reflection"];

            // create notches
            hall[0] = [
                new(NotchType.IntroRoom, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][0], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][1], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][2], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][5], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][6], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][7], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][8], this.getRandInt(), 0),
            ];
            hall[1] = [
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][0], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Boss, enc[1][1], this.getRandInt(), NotchFlag.BOSS),
            ];
            hall[2] = [
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][0], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Boss, enc[2][1], this.getRandInt(), NotchFlag.BOSS),
            ];
            hall[3] = [
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][0], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
                new(NotchType.Boss, enc[3][1], this.getRandInt(), NotchFlag.BOSS),
            ];
            hall[4] = [
                new(NotchType.Shop, "", this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][0], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][1], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][4], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),
            ];
            hall[5] = [
                new(NotchType.PinnacleCutscene, enc[5][0], this.getRandInt(), 0),
                new(NotchType.FinalBoss, enc[5][0], this.getRandInt(), NotchFlag.BOSS + NotchFlag.FINAL_BOSS),
                new(NotchType.EndRun, "", this.getRandInt(), 0),
            ];

        } else if (this.routeType != -1) { // RANDOM
            // randoms have the same amount of notches, so we structure them similarly
            string[] startAreas = ["hw_outskirts", "hw_geode"];
            this.hallRand.Shuffle(startAreas);
            string[] mainAreas = ["hw_nest", "hw_arsenal", "hw_lighthouse", "hw_streets", "hw_lakeside", "hw_sanct", "hw_depths", "hw_aurum"];
            string[] endAreas = ["hw_keep", "hw_darkhall"];
            string[] finalAreas = ["hw_pinnacle", "hw_reflection"];
            string[][] areas = [mainAreas, endAreas, finalAreas];
            foreach (var areaGroup in areas) this.hallRand.Shuffle(areaGroup);

            // set hallkeys
            this.route = [.. startAreas, .. mainAreas, .. endAreas, .. finalAreas];
            this.hallkeys = [startAreas[0], startAreas[1], mainAreas[2], mainAreas[5], mainAreas[7], finalAreas[0]];
            this.hallkeys[1] = this.route[2];

            // their encounters must be fetched independently though
            if (this.routeType == 2) { // TRUE RANDOM
                enc = [[.. this.getEncFromHallkey(startAreas[0]), .. this.getEncFromHallkey(startAreas[1])]]; // since outskirts/geode has varying length, easier to combine them
                foreach (var areaGroup in areas) {
                    foreach (var area in areaGroup) {
                        enc = [.. enc, this.getEncFromHallkey(area)];
                    }
                }
            } else { // CHAOTIC RANDOM
                var chaoticList = this.getEncChaotic();
                enc = [
                    chaoticList[0],
                    [chaoticList[2][0], chaoticList[3][0], .. chaoticList[1].Skip(0).Take(4)],
                    [chaoticList[2][1], chaoticList[3][1], .. chaoticList[1].Skip(4).Take(4)],
                    [chaoticList[2][2], chaoticList[3][2], .. chaoticList[1].Skip(8).Take(4)],
                    [chaoticList[2][3], chaoticList[3][3], .. chaoticList[1].Skip(12).Take(4)],
                    [chaoticList[2][4], chaoticList[3][4], .. chaoticList[1].Skip(16).Take(4)],
                    [chaoticList[2][5], chaoticList[3][5], .. chaoticList[1].Skip(20).Take(4)],
                    [chaoticList[2][6], chaoticList[3][6], .. chaoticList[1].Skip(24).Take(4)],
                    [chaoticList[2][7], chaoticList[3][7], .. chaoticList[1].Skip(28).Take(4)],
                    [.. chaoticList[4].Skip(0).Take(5)],
                    [.. chaoticList[4].Skip(5).Take(5)],
                    [chaoticList[5][0]],
                    [chaoticList[5][1]],
                ];
            }

            hall[0] = [
                new(NotchType.IntroRoom, "", this.getRandInt(), 0), // start
                new(NotchType.Encounter, enc[0][0], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][1], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][2], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),     // chest 1
                new(NotchType.Encounter, enc[0][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][6], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][7], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][8], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][9], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][10], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),     // chest 2
                new(NotchType.Encounter, enc[0][11], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][12], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][13], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][14], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][15], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][16], this.getRandInt(), 0),
            ];
            hall[1] = [
                new(NotchType.Encounter, enc[0][17], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[0][18], this.getRandInt(), 0),
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // main area 1
                new(NotchType.Encounter, enc[1][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[1][0], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),     // chest 3
                new(NotchType.Boss, enc[1][1], this.getRandInt(), NotchFlag.BOSS),
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // main area 2
                new(NotchType.Encounter, enc[2][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[2][0], this.getRandInt(), 0),
                new(NotchType.Boss, enc[2][1], this.getRandInt(), NotchFlag.BOSS),
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // main area 3
                new(NotchType.Encounter, enc[3][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][3], this.getRandInt(), 0),
            ];
            hall[2] = [
                new(NotchType.Encounter, enc[3][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[3][0], this.getRandInt(), 0),
                new(NotchType.Boss, enc[3][1], this.getRandInt(), NotchFlag.BOSS),
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // main area 4
                new(NotchType.Encounter, enc[4][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[4][0], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),     // chest 4
                new(NotchType.Boss, enc[4][1], this.getRandInt(), NotchFlag.BOSS),
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // main area 5
                new(NotchType.Encounter, enc[5][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[5][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[5][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[5][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[5][0], this.getRandInt(), 0),
                new(NotchType.Boss, enc[5][1], this.getRandInt(), NotchFlag.BOSS),
            ];
            hall[3] = [
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // main area 6
                new(NotchType.Encounter, enc[6][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[6][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[6][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[6][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[6][0], this.getRandInt(), 0),
                new(NotchType.Boss, enc[6][1], this.getRandInt(), NotchFlag.BOSS),
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // main area 7
                new(NotchType.Encounter, enc[7][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[7][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[7][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[7][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[7][0], this.getRandInt(), 0),
                new(NotchType.Boss, enc[7][1], this.getRandInt(), NotchFlag.BOSS),
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // main area 8
                new(NotchType.Encounter, enc[8][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[8][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[8][4], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[8][5], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[8][0], this.getRandInt(), 0),
            ];
            hall[4] = [
                new(NotchType.Chest, "", this.getRandInt(), 0),     // chest 5
                new(NotchType.Boss, enc[8][1], this.getRandInt(), NotchFlag.BOSS),
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // end area 1
                new(NotchType.Encounter, enc[9][0], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[9][1], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[9][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[9][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[9][4], this.getRandInt(), 0),
                new(NotchType.Shop, "", this.getRandInt(), NotchFlag.SHOPKEEP_WOLF),      // end area 2
                new(NotchType.Encounter, enc[10][0], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[10][1], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[10][2], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[10][3], this.getRandInt(), 0),
                new(NotchType.Encounter, enc[10][4], this.getRandInt(), 0),
                new(NotchType.Chest, "", this.getRandInt(), 0),     // chest 6
            ];
            hall[5] = [
                new(NotchType.PinnacleCutscene, enc[11][0], this.getRandInt(), 0),
                new(NotchType.FinalBoss, enc[11][0], this.getRandInt(), NotchFlag.BOSS + NotchFlag.FINAL_BOSS),
                new(NotchType.PinnacleCutscene, enc[12][0], this.getRandInt(), 0),
                new(NotchType.FinalBoss, enc[12][0], this.getRandInt(), NotchFlag.BOSS + NotchFlag.FINAL_BOSS),
                new(NotchType.EndRun, "", this.getRandInt(), 0),
            ];
        }
        return hall;
    }

    private RValue* ChooseHallsDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_hallwayprogress_choose_halls"];
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
            // resets tracking variables
            this.potionStored = false;
            this.erasingPotions = false;
            this.hallCount = -1; // used with notchCount for hallway switches and balancing thresholds
            
            // check if Toybox is on
            RValue* hallkey = rnsReloaded.FindValue(self, "hallkey");
            RValue hallkeyLength = rnsReloaded.ArrayGetLength(hallkey) ?? new RValue(0);
            if (utils.RValueToLong(&hallkeyLength) != 1) {
                // randomizes hallkeys based on seed from host
                RValue* mapSeedR = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "mapSeed");
                long mapSeed = utils.RValueToLong(mapSeedR); // mapSeed is a different datatype for host/client
                this.hallRand = new Random((int) mapSeed);

                // determines notches for every area
                this.hallNotches = this.CreateNotches(this.config.Route);

                // sets hallkeys in game
                rnsReloaded.CreateString(rnsReloaded.ArrayGetEntry(hallkey, 0), this.hallkeys[0]);
                rnsReloaded.CreateString(rnsReloaded.ArrayGetEntry(hallkey, 1), this.hallkeys[1]);
                rnsReloaded.CreateString(rnsReloaded.ArrayGetEntry(hallkey, 2), this.hallkeys[2]);
                rnsReloaded.CreateString(rnsReloaded.ArrayGetEntry(hallkey, 3), this.hallkeys[3]);
                rnsReloaded.CreateString(rnsReloaded.ArrayGetEntry(hallkey, 4), this.hallkeys[4]);
                rnsReloaded.CreateString(rnsReloaded.ArrayGetEntry(hallkey, 5), this.hallkeys[5]);
            }
        }
        return returnValue;
    }

    private RValue* StartHallwayDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_hallwayprogress_start_hallway"];
        this.notchCount = 0;
        return hook!.OriginalFunction(self, other, returnValue, argc, argv);
    }

    private RValue* GenericAreaDetour(
        IHook<ScriptDelegate>? hook, Func<int, List<Notch>> createNotches,
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
            this.hallCount += 1;
            utils.setHallway(createNotches(this.hallCount), self, rnsReloaded);
        }
        return returnValue;
    }

    private ScriptDelegate MakeHallwayGenDetour(string hookName) {
        return (CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) => {
            return this.GenericAreaDetour(ScriptHooks[hookName], count => this.hallNotches[count], self, other, returnValue, argc, argv);
        };
    }

    private static int stageFromKey(string key) {
        return key switch {
            "hw_outskirts" => 1,
            "hw_nest" => 2,
            "hw_arsenal" => 3,
            "hw_lighthouse" => 4,
            "hw_streets" => 5,
            "hw_lakeside" => 6,
            "hw_keep" => 7,
            "hw_pinnacle" => 8,
            "hw_geode" => 9,
            "hw_sanct" => 10,
            "hw_depths" => 11,
            "hw_aurum" => 12,
            "hw_darkhall" => 13,
            "hw_reflection" => 14,
            _ => 0, // test hallway
        };
    }

    private bool isNewHallBase(out string currentHallkey) {
        switch ((this.hallCount, this.notchCount)) {
            case (1, 9):
                currentHallkey = this.route[2];
                return true;
            case (2, 9):
                currentHallkey = this.route[4];
                return true;
            default:
                currentHallkey = "";
                return false;
        }
    }

    private static readonly Dictionary<(int hall, int notch), int> NewHallRandomMap =
    new() {
        [(1, 3)] = 2,
        [(1, 11)] = 3,
        [(1, 18)] = 4,
        [(2, 5)] = 5,
        [(2, 13)] = 6,
        [(3, 8)] = 8,
        [(3, 15)] = 9,
        [(4, 3)] = 10,
        [(4, 9)] = 11,
        [(5, 3)] = 13
    };

    private bool isNewHallRandom(out string currentHallkey) {
        var key = (this.hallCount, this.notchCount);
        // check outskirts/geode first
        bool geodeFirst = this.hallkeys[0] == "hw_geode";
        if (key == (0, 11) && geodeFirst) {
            currentHallkey = this.route[1];
            return true;
        }
        if (key == (0, 12) && !geodeFirst) {
            currentHallkey = this.route[1];
            return true;
        }
        // lookup with map
        if (NewHallRandomMap.TryGetValue(key, out var index)) {
            currentHallkey = this.route[index];
            return true;
        }
        currentHallkey = "";
        return false;
    }

    private bool isNewHall(out string currentHallKey) {
        // hall-notch numbers are derived from routes
        // see included routeNotches.txt file for reference
        if (this.routeType == 0) // base
            return this.isNewHallBase(out currentHallKey);
        if (this.routeType == 2 || this.routeType == 3) // random
            return this.isNewHallRandom(out currentHallKey);
        currentHallKey = ""; // dlc never transitions midhall
        return false;
    }

    private int getEnemyLevelBase() {
        return (this.hallCount, this.notchCount) switch {
            (0, 1)  => 2,   // outskirts
            (1, 2)  => 14,  // main 1
            (1, 10) => 21,  // main 2
            (2, 2)  => 28,  // main 3
            (2, 10) => 36,  // main 4
            (3, 2)  => 45,  // main 5
            (4, 2)  => 63,  // end
            (5, 2)  => 68,  // finalboss
            _ => -1,
        };
    }

    private int getEnemyLevelDLC() {
        return (this.hallCount, this.notchCount) switch {
            (0, 1) => 2,    // outskirts
            (0, 5) => 5,
            (0, 9) => 8,
            (1, 2) => 14,   // main 1
            (1, 8) => 19,
            (2, 2) => 28,   // main 2
            (2, 8) => 33,
            (3, 2) => 44,   // main 3
            (3, 8) => 49,
            (4, 2) => 62,   // end
            (5, 2) => 68,   // finalboss
            _ => -1,
        };
    }

    private int getEnemyLevelRandom() {
        return (this.hallCount, this.notchCount) switch {
            (0, 1)  => 1,   // start
            (0, 3)  => 2,
            (0, 5)  => 3,   //  chest correction
            (0, 12) => 9,
            (0, 14) => 10,  //  chest correction
            (1, 1)  => 16,  //  hallchange
            (1, 4)  => 18,  // main 1
            (1, 10) => 22,  //  boss
            (1, 12) => 23,  // main 2
            (1, 17) => 27,  //  boss
            (1, 19) => 28,  // main 3
            (2, 1)  => 30,  //  hallchange
            (2, 4)  => 32,  //  boss
            (2, 6)  => 33,  // main 4
            (2, 12) => 37,  //  boss
            (2, 14) => 38,  // main 5
            (2, 19) => 42,  //  boss
            (3, 2)  => 43,  // main 6
            (3, 7)  => 47,  //  boss
            (3, 9)  => 48,  // main 7
            (3, 14) => 52,  //  boss
            (3, 16) => 53,  // main 8
            (4, 2)  => 57,  //  boss
            (4, 4)  => 58,  // end 1
            (4, 10) => 63,  // end 2
            (5, 2)  => 68,  // final 1
            (5, 4)  => 68,  // final 2
            _ => -1,
        };
    }

    private int getEnemyLevel() {
        return this.routeType switch {
            0 => this.getEnemyLevelBase(),
            1 => this.getEnemyLevelDLC(),
            2 => this.getEnemyLevelRandom(),
            3 => this.getEnemyLevelRandom(),
            _ => -1
        };
    }

    private void updateEnemyLevel() {
        int level = this.getEnemyLevel();
        if (level == -1) return;
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            RValue* enemyLevel = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "enemyLevel");
            *enemyLevel = new RValue(level);
        }
    }

    private RValue* MoveNextDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_hallwayprogress_move_next"];
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
            this.updateEnemyLevel();

            // check and change hallway
            if (this.isNewHall(out var currentHallkey)) {
                int stage = stageFromKey(currentHallkey);
                if (this.hallCount > 0 && ((this.hallCount == 1 && this.notchCount > 9) || (this.hallCount > 1))) {
                    this.erasingPotions = true;
                    rnsReloaded.ExecuteScript("scr_itemsys_erase_potions", this.potionSelf, this.potionOther, []);
                    this.erasingPotions = false;
                }
                rnsReloaded.ExecuteScript("scr_stage_change", self, other, [new(stage), new(3000)]);
                rnsReloaded.ExecuteScript("scr_stage_play_music", self, other, [new(stage)]);
                RValue[] moveNextArgs = [new(1500), new(3000)];
                rnsReloaded.ExecuteScript("scr_players_move_next_position", self, other, moveNextArgs);
            }
            this.notchCount += 1; // counts notch
        }
        return returnValue;
    }

    private double getRewardScaleBase() {
        return this.hallCount switch {
            0 => 3 / 10.0,
            1 or 2 => 2 / 6.0,
            3 => 4 / 6.0,
            4 => 3 / 5.0,
            _ => 1.0
        };
    }

    private double getRewardScaleDLC() {
        return this.hallCount switch {
            0 => 3 / 9.0,
            1 or 2 or 3 => 4 / 6.0,
            4 => 3 / 5.0,
            _ => 1.0
        };
    }
    
    private double getRewardScaleRandom() {
        return this.hallCount switch {
            0 => 3 / 19.0,
            1 or 2 or 3 => 12 / 48.0,
            4 => 3 / 10.0,
            _ => 1.0
        };
    }

    private double getRewardScale() {
        // scales enemy rewards based on how many enemies there are compared to regular hall
        return this.routeType switch {
            0 => this.getRewardScaleBase(),
            1 => this.getRewardScaleDLC(),
            2 => this.getRewardScaleRandom(),
            3 => this.getRewardScaleRandom(),
            _ => -1
        };
    }

    private RValue* GiveRewardsDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_rankbar_give_rewards"];
        // gets scale factor based on hall
        double enemyScale = this.getRewardScale();
        RValue*[] newArgv = new RValue*[3];
        newArgv[0] = argv[0]; // gold value
        newArgv[1] = argv[1]; // exp value

        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            // scales and stores values
            double val1 = utils.RValueToDouble(argv[1]);
            double val2 = utils.RValueToDouble(argv[2]);
            RValue rval1 = new RValue(Math.Ceiling(val1 * enemyScale));
            RValue rval2 = new RValue(Math.Ceiling(val2 * enemyScale));
            newArgv[1] = &rval1;
            newArgv[2] = &rval2;
        }

        // updates reward values
        fixed (RValue** newArgv2 = newArgv) {
            returnValue = hook!.OriginalFunction(self, other, returnValue, argc, newArgv2);
        }
        return returnValue;
    }

    private RValue* ErasePotionsDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv)
        {
        var hook = ScriptHooks["scr_itemsys_erase_potions"];
        // only need to erase potions past outskirts
        if (!this.potionStored) {
            this.potionStored = true;
            this.potionSelf = self;
            this.potionOther = other;
        }
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            if ((this.routeType != 2 && this.routeType != 3) || this.erasingPotions)
            returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
        }
        return returnValue;
    }

    public void Suspend() {}

    public void Resume() {}

    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
