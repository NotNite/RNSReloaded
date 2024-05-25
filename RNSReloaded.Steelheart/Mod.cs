using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.PermanentWinds;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private IHook<ScriptDelegate>? damageHook;
    private IHook<ScriptDelegate>? enrageHook;
    private IHook<ScriptDelegate>? newFightHook;
    private IHook<ScriptDelegate>? phaseChangeHook;
    private IHook<ScriptDelegate>? bossStartHook;

    // Flag to track if the enemy has started enrage
    // Set to false on combat start and phase change (scrdt_encounter, scrbp_boss_heal)
    // Set to true on enrage (scrbp_make_warning_enrage) (actual enrage pattern not used by bosses)
    private bool enraged = false;
    // Flag to see if this is phase 1 of a boss, since boss phase transition needs to be prevented
    // Set to false on combat start and phase change (same as above)
    // Set to true when boss music starts (scr_music_play_boss_track) - note that this seems to be integrated
    // into the track, so using Steel Yourself to force an encounter won't enable this flag.
    // Also might not work on shira, so ideally a better enable method could be found.
    // Maybe look into scrbp_health_threshold?
    private bool isBossP1 = false;

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

            var damageId = rnsReloaded.ScriptFindId("scr_pattern_deal_damage_enemy_subtract");
            var damageScript = rnsReloaded.GetScriptData(damageId - 100000);

            this.damageHook =
                hooks.CreateHook<ScriptDelegate>(this.EnemyDamageDetour, damageScript->Functions->Function);
            this.damageHook.Activate();
            this.damageHook.Enable();

            var enrageId = rnsReloaded.ScriptFindId("scrbp_make_warning_enrage");
            var enrageScript = rnsReloaded.GetScriptData(enrageId - 100000);
            this.enrageHook =
                hooks.CreateHook<ScriptDelegate>(this.EnrageDetour, enrageScript->Functions->Function);
            this.enrageHook.Activate();
            this.enrageHook.Enable();

            var newFightId = rnsReloaded.ScriptFindId("scrdt_encounter");
            var newFightScript = rnsReloaded.GetScriptData(newFightId - 100000);
            this.newFightHook =
                hooks.CreateHook<ScriptDelegate>(this.NewFightDetour, newFightScript->Functions->Function);
            this.newFightHook.Activate();
            this.newFightHook.Enable();

            var phaseChangeId = rnsReloaded.ScriptFindId("scrbp_boss_heal");
            var phaseChangeScript = rnsReloaded.GetScriptData(phaseChangeId - 100000);
            this.phaseChangeHook =
                hooks.CreateHook<ScriptDelegate>(this.PhaseChangeDetour, phaseChangeScript->Functions->Function);
            this.phaseChangeHook.Activate();
            this.phaseChangeHook.Enable();

            var bossStartId = rnsReloaded.ScriptFindId("scr_music_play_boss_track");
            var bossStartScript = rnsReloaded.GetScriptData(bossStartId - 100000);
            this.bossStartHook =
                hooks.CreateHook<ScriptDelegate>(this.BossStartDetour, bossStartScript->Functions->Function);
            this.bossStartHook.Activate();
            this.bossStartHook.Enable();

            // TODO: make this work with matti mice (need some flag when they get summoned, remove it at end of fight)
            // TODO: make this work with shira (investigation needed for shira defense required move/patterns in general)

            // TODO (long term): Disable iFrames except from taking damage
            // - Look into wolf boss time slow code for hints
        }
    }

    private RValue* GetEnemy(double id) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var instance = rnsReloaded.GetGlobalInstance();
            var playerList = rnsReloaded.FindValue(instance, "player");
            var enemyList = rnsReloaded.ArrayGetEntry(playerList, 1);
            return rnsReloaded.ArrayGetEntry(enemyList, (int) id);
        } else {
            throw new NullReferenceException("Hardcore mod not loaded properly - can't get RnsReloaded reference");
        }
    }
    private double GetEnemyHP(double id) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            return rnsReloaded.FindValue(this.GetEnemy(id)->Object, "displayHp")->Real;
        } else {
            throw new NullReferenceException("Hardcore mod not loaded properly - can't get RnsReloaded reference");
        }
    }

    private double GetEnemyMaxHP(double id) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            return rnsReloaded.FindValue(this.GetEnemy(id)->Object, "displayMaxHp")->Real;
        } else {
            throw new NullReferenceException("Hardcore mod not loaded properly - can't get RnsReloaded reference");
        }
    }

    private RValue* EnemyDamageDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        // Any enraged enemy is instantly killable
        if (!this.enraged) {
            double enemyHp = this.GetEnemyHP(argv[1]->Real);
            double enemyMaxHp = this.GetEnemyMaxHP(argv[1]->Real);
            // Bosses phase change ~40% HP
            // So to force p1 to go to enrage, we put a check there instead of at 250 HP
            if (this.isBossP1 && (enemyHp - argv[2]->Real) < (enemyMaxHp * 0.4 + 250)) {
                argv[2]->Real = Math.Max((enemyHp - enemyMaxHp * .4) - 250, 0);
            // Non bosses (and boss phase 2s) can go to 250 HP before waiting on enrage to be killable
            // 250 instead of 1 to hopefully make the bug where sometimes this lock could be broken
            // by enough damage instances in the same frame.
            } else if (enemyHp - argv[2]->Real < 250) {
                argv[2]->Real = Math.Max(enemyHp - 250, 0);
            }
        }

        returnValue = this.damageHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private RValue* EnrageDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        this.enraged = true;
        returnValue = this.enrageHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }
    
    private RValue* NewFightDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        this.enraged = false;
        this.isBossP1 = false;

        returnValue = this.newFightHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private RValue* PhaseChangeDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        this.enraged = false;
        this.isBossP1 = false;
        returnValue = this.phaseChangeHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private RValue* BossStartDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        this.isBossP1 = true;
        returnValue = this.bossStartHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    public void Suspend() {
        this.damageHook?.Disable();
    }

    public void Resume() {
        this.damageHook?.Enable();
    }

    public bool CanSuspend() => false; // Add suspend/resume code and set to true once ready

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
