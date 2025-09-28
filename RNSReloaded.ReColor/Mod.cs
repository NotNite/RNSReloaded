using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using RNSReloaded.ReColor.Config;
using System.Runtime.InteropServices;

namespace RNSReloaded.ReColor;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private static Dictionary<string, IHook<ScriptDelegate>> ScriptHooks = new();

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
    private struct FuncData {
        public string layer;
        public int colorIndex;
    }

    private Dictionary<string, FuncData> funcLookup = new Dictionary<string, FuncData>() {
        { "scrbp_make_warning_colormatch", new FuncData() { layer = "BattleWarningOver", colorIndex = 3} },
        { "scrbp_make_warning_colormatch_targ", new FuncData() { layer = "BattleEffect", colorIndex = 2} },
        { "scrbp_make_warning_colormatch2_targ", new FuncData() { layer = "BattleEffect", colorIndex = 2} },
        { "scrbp_make_warning_colormatch3", new FuncData() { layer = "BattleWarningOver", colorIndex = 3} },
        { "scrbp_make_warning_colormatch3_targ", new FuncData() { layer = "BattleEffect", colorIndex = 2} }
    };

    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
        ) {

            foreach (var hookStr in this.funcLookup.Keys) {
                RValue* Detour(
                    CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
                ) {
                    return this.ColormatchWarnDetour(hookStr, self, other, returnValue, argc, argv);
                }

                var script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(hookStr) - 100000);

                var hook = hooks.CreateHook<ScriptDelegate>(Detour, script->Functions->Function)!;

                hook.Activate();
                hook.Enable();

                ScriptHooks[hookStr] = hook;
            }
        }
    }

    private CLayer* FindLayer(string layerName = "") {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var layers = rnsReloaded.GetCurrentRoom()->Layers;
            CLayer* curLayer = layers.First;
            CLayer* searchLayer = null;
            while (curLayer != null) {
                // If no layer name, then we want to find the max layer ID
                if (layerName == "") {
                    if (searchLayer == null) {
                        searchLayer = curLayer;
                    } else if (curLayer->ID >= searchLayer->ID) {
                        searchLayer = curLayer;
                    }
                    // If a layer name is provided, break as soon as we find it
                } else {
                    if (Marshal.PtrToStringAnsi((nint) curLayer->Name)! == layerName) {
                        searchLayer = curLayer;
                        break;
                    }
                }
                curLayer = curLayer->Next;
            }
            return searchLayer;
        } else {
            return null;
        }
    }
    private CLayerInstanceElement* GetMostRecentObjectFromLayer(string layerName = "") {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var layers = rnsReloaded.GetCurrentRoom()->Layers;
            CLayer* searchLayer = this.FindLayer(layerName);

            if (searchLayer != null) {
                CLayerElementBase* elem = searchLayer->Elements.First;
                CLayerElementBase* maxElem = elem;
                while (elem != null) {
                    if (elem->ID > maxElem->ID) {
                        maxElem = elem;
                    }
                    elem = elem->Next;
                }
                if (maxElem == null) {
                    this.logger.PrintMessage("Layer " + layerName + " has no elements", this.logger.ColorRed);
                }
                return (CLayerInstanceElement*) maxElem;
            } else {
                this.logger.PrintMessage("Failed to find layer with name " + layerName, this.logger.ColorRed);
            }
        }
        return null;
    }



    private RValue* ColormatchWarnDetour(
        string name, CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        var hook = ScriptHooks[name];
        hook.OriginalFunction(self, other, returnValue, argc, argv);
        
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var ring = this.GetMostRecentObjectFromLayer(this.funcLookup[name].layer);
            if (ring != null) {
                var color = rnsReloaded.FindValue(ring->Instance, "color");
                color->Type = RValueType.Real;
                double colorToUse = rnsReloaded.utils.RValueToDouble(color);
                switch (rnsReloaded.utils.RValueToLong(argv[this.funcLookup[name].colorIndex])) {
                    case IBattlePatterns.COLORMATCH_RED:
                        colorToUse = this.config.RedColor;
                        break;
                    case IBattlePatterns.COLORMATCH_YELLOW:
                        colorToUse = this.config.YellowColor;
                        break;
                    case IBattlePatterns.COLORMATCH_GREEN:
                        colorToUse = this.config.GreenColor;
                        break;
                    case IBattlePatterns.COLORMATCH_BLUE:
                        colorToUse = this.config.BlueColor;
                        break;
                    case IBattlePatterns.COLORMATCH_PURPLE:
                        colorToUse = this.config.PurpleColor;
                        break;
                    default:
                        string[] args = new string[argc];
                        for (int i = 0; i < argc; i++) {
                            args[i] = argv[i]->ToString();
                        }
                        this.logger.PrintMessage("Couldn't figure out what color to use. Args: " + string.Join(", ", args), this.logger.ColorRed);
                        break;
                }
                
                color->Real = colorToUse;
            }
        }
        return returnValue;
    }

    public void Suspend() {
    }

    public void Resume() {
    }

    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
