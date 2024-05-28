using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System.Diagnostics.CodeAnalysis;

namespace RNSReloaded.CustomBossTest;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Util? utils;
    private BattleScripts? battleScripts;
    private BattlePatterns? battlePatterns;

    private IHook<ScriptDelegate>? addEncounterHook;

    public void Start(IModLoaderV1 loader) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
        this.hooksRef = loader.GetController<IReloadedHooks>()!;

        this.logger = loader.GetLogger();

        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.OnReady += this.Ready;
        }
    }

    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
        ) {
            rnsReloaded.LimitOnlinePlay();

            this.utils = new Util(rnsReloaded, this.logger);
            this.battleScripts = new BattleScripts(rnsReloaded, this.utils, this.logger);
            this.battlePatterns = new BattlePatterns(rnsReloaded, this.utils, this.logger);

            var encounterId = rnsReloaded.ScriptFindId("bp_dragon_granite1_s");
            var encounterScript = rnsReloaded.GetScriptData(encounterId - 100000);

            this.addEncounterHook =
                hooks.CreateHook<ScriptDelegate>(this.AddEncounterDetour, encounterScript->Functions->Function);
            this.addEncounterHook.Activate();
            this.addEncounterHook.Enable();
        }
    }

    private double getAngle((double, double) source, (double, double) target) {
        return (180 / Math.PI) * Math.Atan2(target.Item2 - source.Item2, target.Item1 - source.Item1);
    }

    private bool IsReady(
        [MaybeNullWhen(false), NotNullWhen(true)] out IRNSReloaded rnsReloaded,
        [MaybeNullWhen(false), NotNullWhen(true)] out Util utils,
        [MaybeNullWhen(false), NotNullWhen(true)] out BattleScripts scrpb,
        [MaybeNullWhen(false), NotNullWhen(true)] out BattlePatterns bp
    ) {
        if(this.rnsReloadedRef != null) {
            this.rnsReloadedRef.TryGetTarget(out var rnsReloadedRef);
            rnsReloaded = rnsReloadedRef;
            utils = this.utils!;
            scrpb = this.battleScripts!;
            bp = this.battlePatterns!;
            return rnsReloaded != null;
        }
        rnsReloaded = null;
        utils = null;
        scrpb = null;
        bp = null;
        return false;
    }

    private RValue* AddEncounterDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        if (this.IsReady(out var rnsReloaded, out var utils, out var scrbp, out var bp)) {
            var bf_center_x = utils.GetGlobalVar("bfCenterX")->Real;
            var bf_center_y = utils.GetGlobalVar("bfCenterY")->Real;
            var tornado1 = (bf_center_x - bf_center_x * 4 / 5, bf_center_y);
            var tornado2 = (bf_center_x + bf_center_x * 4 / 5, bf_center_y);
            var player_x = utils.GetPlayerVar(0, "distMovePrevX")->Real;
            var player_y = utils.GetPlayerVar(0, "distMovePrevY")->Real;
            var boss_x = utils.GetEnemyVar(0, "distMovePrevX")->Real;
            var boss_y = utils.GetEnemyVar(0, "distMovePrevY")->Real;

            if (scrbp.time(self, other, 500)) {
                scrbp.move_character_absolute(self, other, bf_center_x, bf_center_y, 1200);
            }

            if (scrbp.time_repeating(self, other, 500, 6000)) {
                bp.apply_hbs_synced(self, other, 1000, 0, "hbs_stoneskin", 3000, 1, 63);
                var doll_1 = (bf_center_x - 180, bf_center_y - 180);
                var doll_2 = (bf_center_x - 180, bf_center_y + 180);
                var doll_3 = (bf_center_x + 180, bf_center_y - 180);
                var doll_4 = (bf_center_x + 180, bf_center_y + 180);
                bp.fire_aoe(self, other, 0, 2000, 3000, 2, [doll_1, doll_2, doll_3, doll_4]);
            }
            return returnValue;

            if (scrbp.time(self, other, 1500)) {
                bp.fire_aoe(self, other, 0, 1500, 25000, 1.6, [tornado1, tornado2]);
            }
            if (scrbp.time(self, other, 3000)) {
                var angle_1 = this.getAngle(tornado1, (player_x, player_y));
                bp.cone_direction(self, other, 0, 2000, 30, tornado1, [angle_1]);
                var angle_2 = this.getAngle(tornado2, (player_x, player_y));
                bp.cone_direction(self, other, 0, 2000, 30, tornado2, [angle_2]);
            }
            if (scrbp.time(self, other, 7000)) {
                bp.cone_spreads(self, other, 0, 0, 0, 30, tornado1, null);
                bp.cone_spreads(self, other, 0, 0, 0, 30, tornado2, null);

                var doll_1 = (bf_center_x - 180, bf_center_y - 180);
                var doll_2 = (bf_center_x - 180, bf_center_y + 180);
                var doll_3 = (bf_center_x + 180, bf_center_y - 180);
                var doll_4 = (bf_center_x + 180, bf_center_y + 180);
                bp.fire_aoe(self, other, 0, 1000, 1500, 2, [doll_1, doll_2, doll_3, doll_4]);
            }
            if (scrbp.time(self, other, 8000)) {
                var angle_1 = this.getAngle(tornado1, (player_x, player_y));
                var angle_2 = this.getAngle(tornado2, (player_x, player_y));

                bp.water2_line(self, other, 0, 0, 1500, tornado1, angle_1, 0, 0, 2, 3);
                bp.water2_line(self, other, 0, 0, 1500, tornado2, angle_2, 0, 0, 2, 3);
            }
            if (scrbp.time(self, other, 10000)) {
                bp.enrage(self, other, 0, 1500, 3000, true);
            }
        }
        return returnValue;
    }

    public void Suspend() {
        this.addEncounterHook?.Disable();
    }

    public void Resume() {
        this.addEncounterHook?.Enable();
    }

    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
