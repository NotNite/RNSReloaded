using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

using RNSReloaded.SoloTogether.Config;
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

namespace RNSReloaded.SoloTogether;

public unsafe class Mod : IMod {
    private const int SCRIPTCONST = 100000;

    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Configurator configurator = null!;
    private Config.Config config = null!;

    private int deadPlayers = 0; // bitmask representing dead players

    private static Dictionary<string, IHook<ScriptDelegate>> ScriptHooks = new();
    private static readonly string[] PERMADEATHSCRIPTS = [
        "scr_kotracker_draw_timer",
        "scr_kotracker_can_revive",
        "scrbp_time_repeating"
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
            { "scr_charselect2_start_run", this.StartRunDetour },
            { "scr_kotracker_draw_timer", this.KOTimerDetour },
            { "scr_kotracker_can_revive", this.ReviveDetour },
            { "scrbp_time_repeating", this.TimeRepeatingDetour },
        };

        Dictionary<string, string> soloPatternMap = new Dictionary<string, string> {
            // this dictionary is meant to redirect specicial cases where solo version isn't just adding _s
            {"bp_bird_student0_n", "bp_bird_student0_s"},
            {"bp_bird_student1_l", "bp_bird_student1_s"},
            {"bp_bird_whispering0_n", "bp_bird_whispering0_s"},
            {"bp_bird_whispering1_n", "bp_bird_whispering0_s"},
            {"bp_wolf_blackear2_n", "bp_wolf_blackear2_s"},
            {"bp_frog_seamstress1_h", "bp_frog_seamstress1_s"},
        };

        // complete list of encounters to redirect
        string[] multiPatterns = [
            // outskirts
            "bp_bird_sophomore1",
            "bp_bird_sophomore2",
            "bp_wolf_blackear1",
            "bp_wolf_blackear2",
            "bp_dragon_granite1",
            "bp_dragon_granite2",
            "bp_mouse_cadet1",
            "bp_mouse_cadet2",
            "bp_mouse_medic1",
            "bp_mouse_medic2",
            "bp_frog_tinkerer1",
            "bp_frog_tinkerer2",
            // nest
            "bp_bird_student0",
            "bp_bird_student1",
            "bp_bird_whispering0",
            "bp_bird_whispering1",
            "bp_bird_archon0",
            "bp_bird_valedictorian0",
            "bp_bird_valedictorian1",

            "bp_bird_student0_n",
            "bp_bird_student1_l",
            "bp_bird_whispering0_n",
            "bp_bird_whispering1_n",
            // arsenal
            "bp_wolf_greyeye0",
            "bp_wolf_greyeye1",
            "bp_wolf_bluepaw0",
            "bp_wolf_bluepaw1",
            "bp_wolf_redclaw0",
            "bp_wolf_redclaw1",
            "bp_wolf_snowfur0",
            "bp_wolf_steeltooth0",
            "bp_wolf_steeltooth1",

            "bp_wolf_blackear2_n",
            // lighthouse
            "bp_dragon_gold0",
            "bp_dragon_gold1",
            "bp_dragon_silver0",
            "bp_dragon_silver1",
            "bp_dragon_emerald0",
            "bp_dragon_emerald1",
            "bp_dragon_ruby0",
            "bp_dragon_mythril0",
            "bp_dragon_mythril1",
            // streets
            "bp_mouse_archer0",
            "bp_mouse_archer1",
            "bp_mouse_axewielder0",
            "bp_mouse_axewielder1",
            "bp_mouse_oakspear0",
            "bp_mouse_oakspear1",
            "bp_mouse_rosemage0",
            "bp_mouse_commander0",
            "bp_mouse_paladin0",
            "bp_mouse_paladin1",
            // lakeside
            "bp_frog_seamstress0",
            "bp_frog_seamstress1",
            "bp_frog_songstress0",
            "bp_frog_songstress1",
            "bp_frog_musician0",
            "bp_frog_musician1",
            "bp_frog_painter0",
            "bp_frog_idol0",
            "bp_frog_idol1",

            "bp_frog_seamstress1_h",
            // keep
            "bp_queens_axe0",
            "bp_queens_harp0",
            "bp_queens_knife_l0",
            "bp_queens_knife_r0",
            "bp_queens_spear0",
            "bp_queens_shield0",
            "bp_queens_staff0",
            // pinnacle
            "bp_rabbit_queen0",
            "bp_rabbit_queen1"
        ];

        // if there is no exception for the
        foreach (string pattern in multiPatterns) {
            string soloPattern = soloPatternMap.TryGetValue(pattern, out string? foundValue)
                                  ? foundValue ?? pattern + "_s"
                                  : pattern + "_s";
            detourMap[pattern] = this.CreateBattlePatternDetour(soloPattern);
        }

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
        if (this.config.Adorable) {
            foreach (string script in PERMADEATHSCRIPTS) {
                ScriptHooks[script].Disable();
            }
        } else {
            foreach (string script in PERMADEATHSCRIPTS) {
                ScriptHooks[script].Enable();
            }
        }
    }

    private RValue* StartRunDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        var hook = ScriptHooks["scr_charselect2_start_run"];
        this.deadPlayers = 0; // reset mask
        this.ConfigSetupHooks();
        return hook!.OriginalFunction(self, other, returnValue, argc, argv);
    }

    private ScriptDelegate CreateBattlePatternDetour(string scriptName) {
        return (CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) => {
            if (this.IsReady(out var rnsReloaded, out var hooks, out var utils, out var scrbp, out var bp)) {
                RValue result = rnsReloaded.ExecuteScript(scriptName, self, other, argc, argv) ?? new RValue(0);
                return &result;
            }
            return returnValue;
        };
    }

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
        this.EnforceDeath(self, other);
        return hook!.OriginalFunction(self, other, returnValue, argc, argv);
    }

    public void Suspend() {}

    public void Resume() {}

    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
