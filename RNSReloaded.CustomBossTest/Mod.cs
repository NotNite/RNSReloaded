using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System.Diagnostics.CodeAnalysis;

namespace RNSReloaded.CustomBossTest;

using Position = (double x, double y);
using PosRot = ((double x, double y) position, double angle);

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;
    private Random random = new Random();

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

    private (double, double) rotatePoint((double, double) point, double angle) {
        return (point.Item1 * Math.Cos(angle * (Math.PI/180)) - (point.Item2 * Math.Sin(angle * (Math.PI / 180))), (point.Item2 * Math.Cos(angle * (Math.PI / 180))) + (point.Item1 * Math.Sin(angle * (Math.PI/180))));
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
            //rnsReloaded.ExecuteScript("bp_examples", self, other, argc, argv);
            //return returnValue;
            var bf_center_x = utils.GetGlobalVar("bfCenterX")->Real;
            var bf_center_y = utils.GetGlobalVar("bfCenterY")->Real;
            var player_x = utils.GetPlayerVar(0, "distMovePrevX")->Real;
            var player_y = utils.GetPlayerVar(0, "distMovePrevY")->Real;
            var boss_x = utils.GetEnemyVar(0, "distMovePrevX")->Real;
            var boss_y = utils.GetEnemyVar(0, "distMovePrevY")->Real;

            var bf_center = (bf_center_x, bf_center_y);
            if (scrbp.time(self, other, 500)) {
                scrbp.move_character_absolute(self, other, bf_center_x, bf_center_y, 1200);
            }
            if (scrbp.time(self, other, 1500)) {
                bp.fire_aoe(self, other, positions: [bf_center]);
            }
            if (scrbp.time(self, other, 10000)) {
                bp.enrage(self, other);
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
