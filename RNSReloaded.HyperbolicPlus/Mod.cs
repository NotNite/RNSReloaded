using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using RNSReloaded.HyperbolicPlus.Config;
using System.Diagnostics.CodeAnalysis;

using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading;
using System;
using System.Security.Cryptography;

namespace RNSReloaded.HyperbolicPlus;

/*
    🐀   🐀
🐀           🐀
  "TO ME, MY ALLIES!"
      🎷🐁
🐀           🐀
      🐀
*/

public unsafe class Mod : IMod {
    private const int SCRIPTCONST = 100000;
    public const int ANIM_TIME = 2000;

    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Configurator configurator = null!;
    private Config.Config config = null!;
    private Random rng = new Random();

    private string battleName = "";
    private int atkNo = 0;
    private double damageMult = 0.0;
    private double gameSpeed = 1.0;
    private bool enFlag = false; // prevent infinite loops
    private bool isTakingDamage = false; // for invuln control

    private static Dictionary<string, IHook<ScriptDelegate>> ScriptHooks = [];
    private static readonly string[] PREVENTINVULNSCRIPTS = [
        "scr_pattern_deal_damage_ally",
        "scr_player_invuln",
        "scr_hbsflag_check"
    ];
    private static readonly string[] PERMADEATHSCRIPTS = [
        "scr_kotracker_can_revive"
    ];

    private IHook<ScriptDelegate>? enemyHookS;
    private IHook<ScriptDelegate>? enemyHookM;

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
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
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
            CScript* script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(scriptName) - SCRIPTCONST);
            hook = hooks.CreateHook(detour, script->Functions->Function);
            hook.Activate();
            hook.Enable();
            return;
        }
        hook = null;
    }

    public void InitializeHooks() {
        var detourMap = new Dictionary<string, ScriptDelegate> {
            { "scr_charselect2_start_run", this.StartRunDetour},
            // damage mult
            { "scr_pattern_deal_damage_enemy_subtract",
                (CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) => {
                    var hook = ScriptHooks["scr_pattern_deal_damage_enemy_subtract"];
                    argv[2]->Real *= this.damageMult;
                    return hook.OriginalFunction(self, other, returnValue, argc, argv);
                }
            },
            // encounters
            { "scrdt_encounter", this.EncounterDetour},
            // enrage control
            { "bpsw_enrage_time", this.EnrageTimeDetour},
            // invuln control
            { "bp_rabbit_queen1_steel_activate", this.SteelActivateDetour}, // always active
            { "scr_pattern_deal_damage_ally", this.PlayerDmgDetour},
            { "scr_player_invuln", this.InvulnDetour},
            { "scr_hbsflag_check", this.AddHbsFlagCheckDetour},
            // permadeath
            { "scr_kotracker_can_revive", this.ReviveDetour}
        };

        foreach (var detourPair in detourMap) {
            this.CreateAndEnableHook(detourPair.Key, detourPair.Value, out var hook);
            if (hook != null) {
                ScriptHooks[detourPair.Key] = hook;
            }
        }

        this.ConfigSetupHooks();
    }

    private void ConfigSetupHooks() {
        // function to enable/disable certain hooks depending on config
        if (this.config.PreventInvulns) {
            foreach (string script in PREVENTINVULNSCRIPTS) {
                ScriptHooks[script].Enable();
            }
        } else {
            foreach (string script in PREVENTINVULNSCRIPTS) {
                ScriptHooks[script].Disable();
            }
        }
        if (this.config.Permadeath) {
            foreach (string script in PERMADEATHSCRIPTS) {
                ScriptHooks[script].Enable();
            }
        } else {
            foreach (string script in PERMADEATHSCRIPTS) {
                ScriptHooks[script].Disable();
            }
        }
    }

    void execute_pattern(CInstance* self, CInstance* other, string pattern, RValue[] args) {
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            rnsReloaded.ExecuteScript("bpatt_var", self, other, args);
            var x = rnsReloaded.ScriptFindId(pattern);
            args = [new RValue(x)];
            rnsReloaded.ExecuteScript("bpatt_add", self, other, args);
            rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
        }
    }

    private void RunAnimation(Anims id, CInstance* self, CInstance* other) {
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            RValue animName;
            RValue[] argv;
            switch (id) {
                case Anims.None:
                    break;
                case Anims.Tassha:
                    this.execute_pattern(self, other, "bp_wolf_disappear", []);
                    break;
                //scrbp_transform_animation(anim_bird_valedictorian_big, 300, 2, 0.50) -> undefined
                //scrbp_transform_animation(anim_mouse_paladin_big, 300, 2, 0.50)->undefined
                case Anims.Merran:
                    //scrbp_move_character_absolute(1660, 540, 1500, 1) -> undefined
                    //scrbp_move_character(700, 0, 1500, 1)->undefined
                    rnsReloaded.ExecuteScript("scrbp_move_character_absolute", self, other, [new RValue(1600), new RValue(540), new RValue(1500), new RValue(1)]);
                    animName = new RValue(0);
                    rnsReloaded.CreateString(&animName, "anim_wolf_steeltooth_big");
                    argv = new RValue[] { animName, new RValue(380), new RValue(2), new RValue(0.70) };
                    rnsReloaded.ExecuteScript("scrbp_transform_animation", self, other, argv);
                    //scrbp_transform_animation(anim_wolf_steeltooth_big, 380, 2, 0.70)->undefined
                    break;
                case Anims.Ranalie:
                    //scrbp_move_character(700, 0, 1500, 1
                    rnsReloaded.ExecuteScript("scrbp_move_character", self, other, [new RValue(700), new RValue(0), new RValue(1500), new RValue(1)]);
                    animName = new RValue(0);
                    rnsReloaded.CreateString(&animName, "anim_dragon_mythril_big");
                    argv = new RValue[] { animName, new RValue(700), new RValue(4), new RValue(1) };
                    rnsReloaded.ExecuteScript("scrbp_transform_animation", self, other, argv);
                    //(anim_dragon_mythril_big, 700, 4, 1)
                    break;
                case Anims.Avy:
                    //move_character_absolute(960, 540, 1500, 0)
                    rnsReloaded.ExecuteScript("scrbp_move_character_absolute", self, other, [new RValue(960), new RValue(540), new RValue(1500), new RValue(0)]);
                    animName = new RValue(0);
                    rnsReloaded.CreateString(&animName, "anim_frog_idol_big");
                    argv = new RValue[] { animName, new RValue(300), new RValue(2), new RValue(0.50) };
                    rnsReloaded.ExecuteScript("scrbp_transform_animation", self, other, argv);
                    //ml_Script_scrbp_transform_animation(anim_frog_idol_big, 300, 2, 0.50) -> undefined
                    break;
                case Anims.Shira:
                    //move_character(0, 0, 1500, 1) ->
                    //gml_Script_scrbp_move_character_absolute(960, 540, 1500, 1) -> undefined
                    rnsReloaded.ExecuteScript("scrbp_move_character_absolute", self, other, [new RValue(960), new RValue(540), new RValue(1500), new RValue(1)]);
                    animName = new RValue(0);
                    rnsReloaded.CreateString(&animName, "anim_rabbit_queen_big");
                    argv = new RValue[] { animName, new RValue(500), new RValue(2.40), new RValue(0.60) };
                    rnsReloaded.ExecuteScript("scrbp_transform_animation", self, other, argv);
                    //scrbp_transform_animation(anim_rabbit_queen_big, 500, 2.40, 0.60) -> undefined
                    break;
            }
        }
    }

    private void PlayPatternList(List<string> patterns, CInstance* self, CInstance* other, IUtil utils, IBattleScripts scrbp) {
        int totalLength = BattleData.GetTotalLength(patterns);
        int currTime = 0;
        int totalTime = currTime;
        int currentBag = (this.atkNo) / patterns.Count;
        this.rng = new Random(currentBag); // add hallwaySeed to randomize this
        patterns = patterns.OrderBy(x => this.rng.Next()).ToList();
        // play every pattern in bag
        foreach (string pattern in patterns) {
            if (scrbp.time(self, other, ANIM_TIME + totalTime + totalLength * currentBag)) {
                this.execute_pattern(self, other, pattern, []);
                this.atkNo++;
                utils.GetGlobalVar("gameTimeSpeed")->Real = this.gameSpeed;
                if (this.config.AccelerateSpeed) {
                    this.gameSpeed += 0.05;
                }
            }
            totalTime += BattleData.GetLengthByPattern(pattern);
        }
        // forces loop
        if (this.atkNo % patterns.Count == 0) {
            scrbp.time(self, other, ANIM_TIME + totalLength * (currentBag + 1));
        }
        currTime += totalLength;
    }

    private void PlayMix(Mixes mix, CInstance* self, CInstance* other, IUtil utils, IBattleScripts scrbp) {
        List<string> patterns = new List<string> { };
        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            switch (mix) {
                case Mixes.None:
                    break;
                case Mixes.Avy2_S:
                    patterns = new List<string> {
                        "bp_frog_idol1_pt2_s",
                        "bp_frog_idol1_pt3_s",
                        "bp_frog_idol1_pt4_s",
                        "bp_frog_idol1_pt5_s",
                        "bp_frog_idol1_pt6_s",
                        "bp_frog_idol1_pt7_s",
                        "bp_frog_idol1_pt8_s",
                        "bp_frog_idol1_pt9_s"
                    };
                    break;
                case Mixes.Ranalie1_S:
                    patterns = new List<string> {
                        "bp_dragon_mythril0_pt2_s",
                        "bp_dragon_mythril0_pt3_s",
                        "bp_dragon_mythril0_pt4_s",
                        "bp_dragon_mythril0_pt5_s",
                        "bp_dragon_mythril0_pt6_s",
                        "bp_dragon_mythril0_pt7_s",
                        "bp_dragon_mythril0_pt8_s"
                    };
                    break;
                case Mixes.Shira2_S:
                    patterns = new List<string> {
                        "bp_rabbit_queen1_pt2_s",
                        // "bp_rabbit_queen1_pt3_s", // steel, baited
                        "bp_rabbit_queen1_pt4_s",
                        "bp_rabbit_queen1_pt5_s", // steel
                        "bp_rabbit_queen1_pt6_s",
                        "bp_rabbit_queen1_pt7_s", // steel
                        "bp_rabbit_queen1_pt8_s",
                        // "bp_rabbit_queen1_pt9_s",
                    };
                    break;
            }
            this.PlayPatternList(patterns, self, other, utils, scrbp);
        }
    }

    private RValue* StartRunDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_charselect2_start_run"];
        // update config on new run
        this.ConfigSetupHooks();
        return hook!.OriginalFunction(self, other, returnValue, argc, argv);
    }

    private RValue* EncounterDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        var hook = ScriptHooks["scrdt_encounter"];
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            this.battleName = Enum.GetName(this.config.ActivePattern) ?? "";
            string enemy = BattleData.GetEnemy(this.battleName);

            // replace current enc with chosen one
            rnsReloaded.CreateString(argv[0], "enc_" + enemy!);

            CScript* enemyScriptS = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("bp_" + enemy + "_s") - SCRIPTCONST);
            this.enemyHookS = hooks.CreateHook<ScriptDelegate>(this.EnemyDetour, enemyScriptS->Functions->Function);
            this.enemyHookS.Activate();
            this.enemyHookS.Enable();

            CScript* enemyScriptM = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("bp_" + enemy) - SCRIPTCONST);
            this.enemyHookM = hooks.CreateHook<ScriptDelegate>(this.EnemyMDetour, enemyScriptM->Functions->Function);
            this.enemyHookM.Activate();
            this.enemyHookM.Enable();

            Console.WriteLine("bp_" + enemy + "_s");

            // set level
            var enemyLevel = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "enemyLevel");
            *enemyLevel = new RValue(999);
        }

        this.enFlag = false;
        returnValue = hook.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private RValue* EnemyMDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        bool basic = BattleData.GetBasic(this.battleName);
        if (this.enFlag && basic) {
            if (this.enemyHookM != null) return this.enemyHookM.OriginalFunction(self, other, returnValue, argc, argv);
            else return returnValue;
        }
        return this.EnemyDetour(self, other, returnValue, argc, argv);
    }
    private RValue* EnemyDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        // initial setup, fetch battledata
        int length = BattleData.GetLength(this.battleName);
        string pattern = BattleData.GetPattern(this.battleName);
        double zoom = BattleData.GetZoom(this.battleName);
        int stage = BattleData.GetStage(this.battleName);
        Anims anim = BattleData.GetAnim(this.battleName);
        bool basic = BattleData.GetBasic(this.battleName);

        if (this.enFlag && basic) {
            if (this.enemyHookS != null) return this.enemyHookS.OriginalFunction(self, other, returnValue, argc, argv);
            else return returnValue;
        }
        this.enFlag = true;
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            if (scrbp.time(self, other, 0)) {
                // change environment
                this.RunAnimation(anim, self, other);
                rnsReloaded.ExecuteScript("scrbp_zoom", self, other, [new RValue(zoom)]);
                rnsReloaded.ExecuteScript("scr_stage_change", self, other, [new RValue(stage)]);
                // set tracking variables
                this.atkNo = 0;
                this.gameSpeed = utils.GetGlobalVar("gameTimeSpeed")->Real;
            }

            // check if play mix
            Mixes mix = BattleData.GetMix(this.battleName);
            if (mix != Mixes.None) {
                this.PlayMix(mix, self, other, utils, scrbp);
                return returnValue;
            }

            Console.WriteLine($"Executing pattern: {pattern}");

            if (basic) {
                Console.Write("basic");
                // this.execute_pattern(self, other, pattern, []);
                rnsReloaded.ExecuteScript(pattern, self, other, argc, argv);
            }

            else if (scrbp.time_repeating(self, other, ANIM_TIME, length)) {
                // call pattern on set loop
                if (!BattleData.GetBasic(this.battleName)) {
                    this.execute_pattern(self, other, pattern, []);
                }

                // accelerate speed
                utils.GetGlobalVar("gameTimeSpeed")->Real = this.gameSpeed;
                if (this.config.AccelerateSpeed) {
                    this.gameSpeed += 0.1;
                }
            }
        }

        return returnValue;
    }

    private RValue* EnrageTimeDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var a = new RValue(-1);
        return &a;
    }

    // INVULN CONTROL
    private RValue* SteelActivateDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        // disables steel yourself attack, should always be active
        return returnValue;
    }

    private RValue* PlayerDmgDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_pattern_deal_damage_ally"];
        // keep track for hit invuln
        this.isTakingDamage = true;
        returnValue = hook.OriginalFunction(self, other, returnValue, argc, argv);
        this.isTakingDamage = false;
        return returnValue;
    }

    private RValue* InvulnDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_player_invuln"];
        // this is basically steelheart's implementation
        if (!this.isTakingDamage) { argv[0]->Real = -30000; }
        return hook.OriginalFunction(self, other, returnValue, argc, argv);
    }

    private RValue* AddHbsFlagCheckDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_hbsflag_check"];
        returnValue = hook.OriginalFunction(self, other, returnValue, argc, argv);
        if (argv[2]->Real == 1 || argv[2]->Real == 2 || argv[2]->Real == 32) { // Vanish/Ghost, Stoneskin, Super
            returnValue->Real = 0;
        }
        return returnValue;
    }

    // PERMADEATH CONTROL
    private RValue* ReviveDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        // canRevive is false
        RValue result = new RValue(0);
        return &result;
    }


    public void Suspend() { }

    public void Resume() { }

    public bool CanSuspend() => false; // Add suspend/resume code and set to true once ready

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
