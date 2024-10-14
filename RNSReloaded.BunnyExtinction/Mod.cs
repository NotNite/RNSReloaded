using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

using RNSReloaded.BunnyExtinction.Config;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

/*
    🐀   🐀
🐀           🐀
  "TO ME, MY ALLIES!"
      🎷🐁
🐀           🐀
      🐀
*/

namespace RNSReloaded.BunnyExtinction;

public unsafe class Mod : IMod {
    private const int SCRIPTCONST = 100000;
    private const double BBQSPEED = 6.30;

    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Configurator configurator = null!;
    private Config.Config config = null!;

    private bool newRun = true;
    private bool inBattle = false;
    private bool invulnOn = false;
    private int deadPlayers = 0; // bitmask representing dead players
    private List<(int, long)> hpItems = [];
    private double[] movementSpeeds = { BBQSPEED, BBQSPEED, BBQSPEED, BBQSPEED };

    private bool isTakingDamage = false;

    private static Dictionary<string, IHook<ScriptDelegate>> ScriptHooks = new();
    private List<string> bbqScripts = [
        "scr_diffswitch", // max health
        "scr_player_charspeed_calc",  // speed limit
        "scrbp_movespeed_mult",
        "scrbp_erase_radius", // bullet deletion
        "scr_kotracker_draw_timer",  // permadeath
        "scr_kotracker_can_revive",
        "scrbp_time_repeating",
        "scr_player_invuln", // invuln
        "scr_pattern_deal_damage_ally",
        "scrbp_warning_msg_enrage",
        "scr_hbsflag_check",
    ];

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
            CScript* script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(scriptName) - SCRIPTCONST);
            hook = hooks.CreateHook(detour, script->Functions->Function);
            hook.Activate();
            hook.Enable();
            return;
        }
        hook = null;
    }

    public void InitializeHooks() {
        var detourMap = new Dictionary<string, ScriptDelegate>{
            // flag setting
            { "scr_hallwayprogress_choose_halls", this.ChooseHallsDetour},
            { "scr_rankbar_give_rewards", this.GiveRewardsDetour},
            { "scrdt_encounter", this.EncounterDetour},
            // max health/speed
            { "scr_diffswitch", this.DiffSwitchDetour},
            { "scr_player_charspeed_calc", this.SpeedCalcDetour }, 
            { "scrbp_movespeed_mult", this.MovespeedMultDetour},
            // bullet deletion
            { "scrbp_erase_radius", this.EraseRadiusDetour },
            // permadeath
            { "scr_kotracker_can_revive", this.ReviveDetour },
            { "scr_kotracker_draw_timer", this.KOTimerDetour },
            { "scrbp_time_repeating", this.TimeRepeatingDetour },
            // invuln control
            { "scr_player_invuln", this.InvulnDetour },
            { "scr_pattern_deal_damage_ally", this.PlayerDmgDetour },
            { "scr_hbsflag_check", this.AddHbsFlagCheckDetour },
            // shira invuln
            { "scrbp_warning_msg_enrage", this.SteelWarningDetour },
            { "bp_rabbit_queen1_pt4", this.CreatePostSteelDetour("bp_rabbit_queen1_pt4")},
            { "bp_rabbit_queen1_pt4_s", this.CreatePostSteelDetour("bp_rabbit_queen1_pt4_s")},
            { "bp_rabbit_queen1_pt6", this.CreatePostSteelDetour("bp_rabbit_queen1_pt6")},
            { "bp_rabbit_queen1_pt6_s", this.CreatePostSteelDetour("bp_rabbit_queen1_pt6_s")},
            { "bp_rabbit_queen1_pt8", this.CreatePostSteelDetour("bp_rabbit_queen1_pt8")},
            { "bp_rabbit_queen1_pt8_s", this.CreatePostSteelDetour("bp_rabbit_queen1_pt8_s")},
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
        if (!this.config.InfernalBBQ) {
            // playing BEX
            foreach (var script in this.bbqScripts) ScriptHooks[script].Disable();
            this.UnlimitHealth();
        } else {
            // playing BBQ
            foreach (var script in this.bbqScripts) ScriptHooks[script].Enable();
            this.LimitHealth();
        }
    }

    // flags for inBattle / newRun
    private RValue* ChooseHallsDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_hallwayprogress_choose_halls"];
        // use choosehalls to tell if a run is reset
        // for things that activate on run start, activate on the next encounter.
        this.inBattle = false;
        this.newRun = true;
        return hook!.OriginalFunction(self, other, returnValue, argc, argv);
    }

    private RValue* GiveRewardsDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        // to determine if a battle is over
        var hook = ScriptHooks["scr_rankbar_give_rewards"];
        this.inBattle = false;
        return hook.OriginalFunction(self, other, returnValue, argc, argv);
    }

    private RValue* EncounterDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scrdt_encounter"];
        this.inBattle = true;
        if (this.newRun) {
            // reset things
            this.ConfigSetupHooks(); // update settings at the start of every run
            this.deadPlayers = 0; // reset mask
            this.newRun = false;
            this.invulnOn = false;
        }
        return hook.OriginalFunction(self, other, returnValue, argc, argv);
    }

    // PERMADEATH
    private RValue* KOTimerDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_kotracker_draw_timer"];
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            // if KOtimer is drawn, add player to mask
            int id = (int) utils.RValueToLong(argv[0]);
            this.deadPlayers |= (1 << id);
        }
        return hook!.OriginalFunction(self, other, returnValue, argc, argv);
    }

    private RValue* ReviveDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_kotracker_can_revive"];
        RValue result = new RValue(0);
        return &result;
    }

    private void EnforceDeath(CInstance* self, CInstance* other) {
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            // setting hp to smite players
            RValue* playerHp = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "playerHp");
            for (int i = 0; i < 4; i++) { // iterate through bitmask
                if ((this.deadPlayers & (1 << i)) != 0) { // check if player has died
                    *rnsReloaded.ArrayGetEntry(rnsReloaded.ArrayGetEntry(playerHp, 0), i) = new RValue(0);
                }
            }
        }
    }

    private RValue* TimeRepeatingDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scrbp_time_repeating"];
        if (this.inBattle) this.EnforceDeath(self, other);
        return hook!.OriginalFunction(self, other, returnValue, argc, argv);
    }

    // MAX HEALTH/SPEED
    private RValue* DiffSwitchDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_diffswitch"];
        returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
        // diifSwitch is used for multiple things
        // to intercept health, we check all the arguments
        if (argv[0]->Real == 8 && argv[1]->Real == 5 && argv[2]->Real == 5 && argv[3]->Real == 3) {
            RValue newHealth = new RValue(1);
            return &newHealth;
        } else {
            return returnValue;
        }
    }

    private void UnlimitHealth() {
        // readds hp to items
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            RValue* itemStat = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "itemStat");
            foreach ((int, long) item in this.hpItems) {
                RValue* itemHp = itemStat->Get(item.Item1)->Get(0)->Get(1); // location of hp stat
                *itemHp = new RValue(item.Item2);
            }
            this.hpItems.Clear();
        }
    }

    private void LimitHealth() {
        // goes through and removes +hp from items
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            RValue* itemStat = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "itemStat");
            RValue rlength = rnsReloaded.ArrayGetLength(itemStat) ?? new RValue(-1);
            for (int i = 0; i < utils.RValueToLong(&rlength); i++) {
                RValue* itemHp = itemStat->Get(i)->Get(0)->Get(1); // location of hp stat
                long hp = utils.RValueToLong(itemHp);
                if (hp != 0) {
                    // save hp for if config turns off
                    this.hpItems.Add((i, hp));
                    *itemHp = new RValue(0);
                }
            }
        }
    }

    private RValue* SpeedCalcDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_player_charspeed_calc"];
        returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            // store original speed
            var s = new RValue(self);
            int id = (int) utils.RValueToLong(rnsReloaded.FindValue(self, "playerId"));
            this.movementSpeeds[id] = utils.RValueToDouble(returnValue);
            // cap at -2
            if (utils.RValueToDouble(returnValue) > BBQSPEED) {
                *returnValue = new RValue(BBQSPEED);
            }
        }
        return returnValue;
    }

    private RValue* MovespeedMultDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scrbp_movespeed_mult"];
        if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
            int id = (int) utils.RValueToLong(rnsReloaded.FindValue(self, "playerId"));
            // change mult to be based off original movement speed and cap at -2
            if (argv[0]->Real * this.movementSpeeds[id] > BBQSPEED) {
                double cutSpeed = this.movementSpeeds[id] < BBQSPEED ? this.movementSpeeds[id] : BBQSPEED;
                //double cutSpeed = BBQSPEED; // temp
                argv[0]->Real = BBQSPEED / cutSpeed;
            }
        }
        returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    // BULLET DELETION
    private RValue* EraseRadiusDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scrbp_erase_radius"];
        RValue newRadius = new RValue(-1);
        argv[2] = &newRadius;
        return hook!.OriginalFunction(self, other, returnValue, argc, argv);
    }

    // INVUL CONTROL
    private void EnableInvuls() {
        ScriptHooks["scr_pattern_deal_damage_ally"].Disable();
        ScriptHooks["scr_hbsflag_check"].Disable();
        this.invulnOn = true;
    }

    private void DisableInvuls() {
        ScriptHooks["scr_pattern_deal_damage_ally"].Enable();
        ScriptHooks["scr_hbsflag_check"].Enable();
        this.invulnOn = false;
    }

    // steel hooks
    private RValue* SteelWarningDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scrbp_warning_msg_enrage"];
        if (argv[1]->ToString() == "eff_steelyourself") this.EnableInvuls();
        returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }
    
    private ScriptDelegate CreatePostSteelDetour(string scriptName) {
        // add script to list to enable/disable hook on config update
        this.bbqScripts.Add(scriptName);
        return (CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) => {
            var hook = ScriptHooks[scriptName];
            this.DisableInvuls();
            returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        };
    }

    // invuln hooks
    private RValue* PlayerDmgDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_pattern_deal_damage_ally"];
        this.isTakingDamage = true;
        returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
        this.isTakingDamage = false;
        return returnValue;
    }

    private RValue* InvulnDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_player_invuln"];
        if (!this.isTakingDamage && this.invulnOn == false) argv[0]->Real = -30000; // this is basically steelheart's implementation
        else argv[0]->Real = 1000; // allows player to invuln during STEEL YOURSELF
        returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private RValue* AddHbsFlagCheckDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_hbsflag_check"];
        returnValue = hook!.OriginalFunction(self, other, returnValue, argc, argv);
        if (argv[2]->Real == 1 || argv[2]->Real == 2 || argv[2]->Real == 32) { // Vanish/Ghost, Stoneskin, Super
            returnValue->Real = 0;
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
