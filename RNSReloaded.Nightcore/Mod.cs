using System.Xml.Linq;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using RNSReloaded.Nightcore.Config;

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
        this.changeMusicPitch();

        return returnValue;
    }

    private void changeMusicPitch() {

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

            RValue* gameSpeedRValue = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "gameTimeSpeed");
            string gameSpeedRValueType = rnsReloaded.ExecuteCodeFunction("typeof", null, null, 1, (RValue**) gameSpeedRValue).ToString() ?? "none";
            double gameSpeed = 0;
            switch (gameSpeedRValueType) {
                case "number":
                    gameSpeed = gameSpeedRValue->Real;
                    break;
                case "int32":
                    gameSpeed = gameSpeedRValue->Int32;
                    break;
                case "int64":
                    gameSpeed = gameSpeedRValue->Int64;
                    break;
            }

            double p = Math.Max(0.1,(gameSpeed - 1) * this.config.SpeedMultiplier + 1 + this.config.SpeedShift);
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
