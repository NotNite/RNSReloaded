using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using Reloaded.Imgui.Hook;
using Reloaded.Imgui.Hook.Direct3D11;
using DearImguiSharp;
using Reloaded.Imgui.Hook.Implementations;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using RNSReloaded.DamageTracker.Config;
namespace RNSReloaded.DamageTracker;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private LogProducer? logProducer = null;
    private ImGuiConsumer? imGuiConsumer = null;
    private ImGuiBuffConsumer? imGuiBuffConsumer = null;
    private FileLogConsumer? fileLogConsumer = null;


    private bool useImGuiConsumer = false;
    private bool useImGuiBuffConsumer = false;
    private bool useFileConsumer = false;

    public void StartEx(IModLoaderV1 loader,IModConfigV1 modConfig) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        
        this.logger = loader.GetLogger();

        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.OnReady += this.Ready;
        }

        if (this.hooksRef != null && this.hooksRef.TryGetTarget(out var hooks)) {
            SDK.Init(hooks);
            ImguiHook.Create(this.Draw, new ImguiHookOptions() {
                Implementations = [new ImguiHookDx11(), new ImguiHookOpenGL3()],
                EnableViewports = true
            });
        }

        var configurator = new Configurator(((IModLoader) loader).GetModConfigDirectory(modConfig.ModId));
        var config = configurator.GetConfiguration<Config.Config>(0);

        this.useImGuiConsumer = config.ImGuiDisplay;
        this.useImGuiBuffConsumer = config.ImGuiBuffDisplay;
        this.useFileConsumer = config.WriteLogs;

        this.logger.PrintMessage("Set up discount ACT", this.logger.ColorGreen);
    }

    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
        ) {


            this.logProducer = new LogProducer(rnsReloaded, hooks, this.logger);

            if (this.useImGuiConsumer) {
                this.logger.PrintMessage("Using ImGui Log Consumer", this.logger.ColorGreen);
                this.imGuiConsumer = new ImGuiConsumer(this.logProducer, rnsReloaded);
            }
            if (this.useImGuiBuffConsumer) {
                this.imGuiBuffConsumer = new ImGuiBuffConsumer(this.logProducer, rnsReloaded);
            }
            if (this.useFileConsumer) {
                this.logger.PrintMessage("Using File Log Consumer", this.logger.ColorGreen);
                this.fileLogConsumer = new FileLogConsumer(this.logProducer, rnsReloaded, hooks, "./logs");
            }
            // Player applying debuffs or buffs: (note: called even for buffs that already exist)
            // args[0] = player/enemyID
            // args[1] = always 1?
            // args[2] = always undefined?
            // returnValue = did it actually apply maybe?
            // scrbp_add_hbs(0, 1, undefined) -> 1 


            // Phase change, we want to split damage by boss phase (probably) because separate enrage timers
            // scrbp_phase_pattern_remove
        }
    }

    public void Draw() {
        if (this.imGuiConsumer != null) {
            this.imGuiConsumer.Draw();
        }
        if (this.imGuiBuffConsumer != null) {
            this.imGuiBuffConsumer.Draw();
        }
    }

    public void Suspend() {
    }

    public void Resume() {
    }

    public bool CanSuspend() => false; // Add suspend/resume code and set to true once ready

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
