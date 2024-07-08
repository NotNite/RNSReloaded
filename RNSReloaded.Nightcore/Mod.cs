using System.Xml.Linq;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using RNSReloaded.Nightcore.Config;
using System;

namespace RNSReloaded.Nightcore;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private static Dictionary<string, IHook<ScriptDelegate>> ScriptHooks = new();
    private IHook<RoutineDelegate>? musicHook;

    private Configurator configurator = null!;
    private Config.Config config = null!;

    public void StartEx(IModLoaderV1 loader, IModConfigV1 modConfig) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
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


            string[] toHook = {
                "scr_stage_play_music",
                "scr_music_play",
                "scr_music_play_boss_track",
                "scr_music_transfer",
                "scr_music_transfer_logic",
                "scrbp_gamespeed"
            };


            foreach (var hookStr in toHook) {
                RValue* Detour(
                    CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
                ) {
                    return this.AddPatternDetour(hookStr, self, other, returnValue, argc, argv);
                }

                var script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(hookStr) - 100000);

                var hook = hooks.CreateHook<ScriptDelegate>(Detour, script->Functions->Function)!;

                hook.Activate();
                hook.Enable();

                ScriptHooks[hookStr] = hook;

            }

        }
    }

    private RValue* AddPatternDetour(
        string name, CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        var hook = ScriptHooks[name];
        returnValue = hook.OriginalFunction(self, other, returnValue, argc, argv);
        this.doTheThing();

        var a = argv[1];
        return returnValue;
    }

    private void doTheThing() {

        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            RValue action = new RValue();
            RValue calm = new RValue();
            var layers = rnsReloaded.GetCurrentRoom()->Layers;

            var layer = layers.First;

            while (layer != null) {

                var elements = layer->Elements;
                var element = elements.First;
                while (element != null) {
                    if (element->Type == LayerElementType.Instance) {

                        var instance = (CLayerInstanceElement*) element;
                        var instanceValue = new RValue(instance->Instance);
                        var value = &instanceValue;
                        foreach (var key in rnsReloaded.GetStructKeys(value)) {
                            var val = rnsReloaded.FindValue(value->Object, key);
                            if (key == "currentMusicAction") {
                                action = *val;
                            }
                            if (key == "currentMusicCalm") {
                                calm = *val;
                            }
                        }
                    }
                    element = element->Next;
                }
                layer = layer->Next;
            }

            double p = Math.Max(0.1,(rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "gameTimeSpeed")->Real - 1) * this.config.SpeedMultiplier + 1 + this.config.SpeedShift);
            RValue pitch = new RValue(p);
            RValue[] actionArgv = [action, pitch];
            RValue[] calmArgv = [calm, pitch];

            fixed (RValue* ptr = actionArgv) {
                rnsReloaded.ExecuteCodeFunction("audio_sound_pitch", null, null, 2, (RValue**) ptr);
            }

            fixed (RValue* ptr = calmArgv) {
                rnsReloaded.ExecuteCodeFunction("audio_sound_pitch", null, null, 2, (RValue**) ptr);
            }
        }
    }

    public void Resume() { }
    public void Suspend() { }
    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
