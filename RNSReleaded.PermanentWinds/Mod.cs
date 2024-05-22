using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace PermanentWinds;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private IHook<ScriptDelegate>? addPatternHook;
    private IHook<ScriptDelegate>? bossHealHook;

    public void Start(IModLoaderV1 loader) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
        this.hooksRef = loader.GetController<IReloadedHooks>()!;

        this.logger = loader.GetLogger();

        this.logger.PrintMessage("Permanent winds started", this.logger.ColorBlueLight);
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

            var encounterId = rnsReloaded.ScriptFindId("scr_enemy_add_pattern");
            var encounterScript = rnsReloaded.GetScriptData(encounterId - 100000);

            this.addPatternHook =
                hooks.CreateHook<ScriptDelegate>(this.AddPatternDetour, encounterScript->Functions->Function);
            this.addPatternHook.Activate();
            this.addPatternHook.Enable();

            var healId = rnsReloaded.ScriptFindId("scrbp_boss_heal");
            var healScript = rnsReloaded.GetScriptData(healId - 100000);
            this.bossHealHook = hooks.CreateHook<ScriptDelegate>(this.BossHealDetour, healScript->Functions->Function);
            this.bossHealHook.Activate();
            this.bossHealHook.Enable();
        }
    }

    private RValue* CallTailwindsScript(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        // find bp_tailwind_permanent script and run it
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            RValue scriptId = new RValue(rnsReloaded.ScriptFindId("bp_tailwind_permanent"));
            RValue* arg = &scriptId;
            RValue** argArr = &arg;
            returnValue = this.addPatternHook!.OriginalFunction(self, other, returnValue, 1, argArr);
        }
        return returnValue;
    }

    private RValue* AddPatternDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        this.CallTailwindsScript(self, other, returnValue, argc, argv);
        returnValue = this.addPatternHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private RValue* BossHealDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        this.CallTailwindsScript(self, other, returnValue, argc, argv);
        returnValue = this.bossHealHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    public void Suspend() {
        this.addPatternHook?.Disable();
        this.bossHealHook?.Disable();
    }

    public void Resume() {
        this.addPatternHook?.Enable();
        this.bossHealHook?.Enable();
    }

    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}

