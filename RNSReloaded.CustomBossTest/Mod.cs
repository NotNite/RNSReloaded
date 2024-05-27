using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.CustomBossTest;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Util? utils;
    private Patterns? patterns;

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

            this.utils = new Util(this.rnsReloadedRef, this.logger);
            this.patterns = new Patterns(this.rnsReloadedRef, this.utils, this.logger);

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

    private bool scrbp_time(CInstance* self, CInstance* other, int time) {
        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            RValue[] args = [new RValue(time)];
            return rnsReloaded.ExecuteScript("scrbp_time", self, other, args)!.Value.Real > 0.5;
        }
        return false;
    }

    private bool scrbp_time_repeating(CInstance* self, CInstance* other, int loopOffset, int loopLength) {
        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            RValue[] args = [new RValue(loopOffset), new RValue(loopLength)];
            return rnsReloaded.ExecuteScript("scrbp_time_repeating", self, other, args)!.Value.Real > 0.5;
        }
        return false;
    }

    private void scrbp_move_character(CInstance* self, CInstance* other, double x, double y, int moveTime) {
        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            RValue[] args = [new RValue(x), new RValue(y), new RValue(moveTime), new RValue(0)];
            rnsReloaded.ExecuteScript("scrbp_move_character", self, other, args);
        }
    }

    private void scrbp_move_character_absolute(CInstance* self, CInstance* other, double x, double y, int moveTime) {
        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            RValue[] args = [new RValue(x), new RValue(y), new RValue(moveTime), new RValue(0)];
            rnsReloaded.ExecuteScript("scrbp_move_character_absolute", self, other, args);
        }
    }

    private RValue* AddEncounterDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            var bf_center_x = this.utils!.GetGlobalVar("bfCenterX")->Real;
            var bf_center_y = this.utils!.GetGlobalVar("bfCenterY")->Real;
            var tornado1 = (bf_center_x - bf_center_x * 4 / 5, bf_center_y);
            var tornado2 = (bf_center_x + bf_center_x * 4 / 5, bf_center_y);
            var player_x = this.utils!.GetPlayerVar(0, "distMovePrevX")->Real;
            var player_y = this.utils!.GetPlayerVar(0, "distMovePrevY")->Real;
            var boss_x = this.utils!.GetEnemyVar(0, "distMovePrevX")->Real;
            var boss_y = this.utils!.GetEnemyVar(0, "distMovePrevY")->Real;

            if (this.scrbp_time(self, other, 500)) {
                this.scrbp_move_character_absolute(self, other, bf_center_x, bf_center_y, 1200);
            }

            if (this.scrbp_time(self, other, 1500)) {
                this.patterns!.bp_fire_aoe(self, other, 1500, 25000, 1.6, [tornado1, tornado2]);
            }
            if (this.scrbp_time(self, other, 3000)) {
                var angle_1 = this.getAngle(tornado1, (player_x, player_y));
                this.patterns!.bp_cone_direction(self, other, 2000, 30, tornado1, [angle_1]);
                var angle_2 = this.getAngle(tornado2, (player_x, player_y));
                this.patterns!.bp_cone_direction(self, other, 2000, 30, tornado2, [angle_2]);
            }
            if (this.scrbp_time(self, other, 7000)) {
                this.patterns!.bp_cone_spreads(self, other, 0, 30, tornado1, null);
                this.patterns!.bp_cone_spreads(self, other, 0, 30, tornado2, null);

                var doll_1 = (bf_center_x - 180, bf_center_y - 180);
                var doll_2 = (bf_center_x - 180, bf_center_y + 180);
                var doll_3 = (bf_center_x + 180, bf_center_y - 180);
                var doll_4 = (bf_center_x + 180, bf_center_y + 180);
                this.patterns!.bp_fire_aoe(self, other, 1000, 1500, 2, [doll_1, doll_2, doll_3, doll_4]);
            }
            if (this.scrbp_time(self, other, 8000)) {
                var angle_1 = this.getAngle(tornado1, (player_x, player_y));
                var angle_2 = this.getAngle(tornado2, (player_x, player_y));

                this.patterns!.bp_water2_line(self, other, 1500, tornado1, angle_1, 0, 0, 2, 3);
                this.patterns!.bp_water2_line(self, other, 1500, tornado2, angle_2, 0, 0, 2, 3);
            }
            if (this.scrbp_time(self, other, 100000)) {
                this.patterns!.bp_enrage(self, other, 1500, 3000);
            }
        }
        return returnValue;
    }

    private RValue* PatternExamples(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            //if (this.scrbp_time_repeating(self, other, 3000, 5000)) {
            //    var player_x = this.utils!.GetPlayerVar(0, "distMovePrevX")->Real;
            //    var player_y = this.utils!.GetPlayerVar(0, "distMovePrevY")->Real;
            //    this.patterns!.bp_fire_aoe(self, other, 1500, 3000, 1.5, [(player_x, player_y)]);
            //}

            //if (this.scrbp_time_repeating(self, other, 2500, 5000)) {
            //    var player_x = this.utils!.GetEnemyVar(0, "distMovePrevX")->Real;
            //    var player_y = this.utils!.GetEnemyVar(0, "distMovePrevY")->Real;
            //    this.patterns!.bp_knockback_circle(self, other, 1500, 500, 250, 2, player_x, player_y);
            //}

            //if (this.scrbp_time_repeating(self, other, 3000, 5000)) {
            //    var player_x = this.utils!.GetPlayerVar(0, "distMovePrevX")->Real;
            //    var player_y = this.utils!.GetPlayerVar(0, "distMovePrevY")->Real;
            //    this.patterns!.bp_cone_direction(self, other, 1500, 30, player_x, player_y, [0, 90, 180, 270]);
            //}

            //if (this.scrbp_time_repeating(self, other, 1500, 3000)) {
            //    var x = this.utils!.GetEnemyVar(0, "distMovePrevX")->Real;
            //    var y = this.utils!.GetEnemyVar(0, "distMovePrevY")->Real;
            //    this.patterns!.bp_cone_spreads(self, other, 1500, 30, x, y);
            //}

            //if (this.scrbp_time_repeating(self, other, 1500, 3000)) {
            //    var bf_center_x = this.utils!.GetGlobalVar("bfCenterX")->Real;
            //    var bf_center_y = this.utils!.GetGlobalVar("bfCenterY")->Real;
            //    this.patterns!.bp_water2_line(self, other, 1500, (bf_center_x, bf_center_y), 0, 45, 100, 2, 3);
            //}

            //if (this.scrbp_time(self, other, 3000)) {
            //    this.patterns!.bp_enrage(self, other, 1500, 3000);
            //}
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
