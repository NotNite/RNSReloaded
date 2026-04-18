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

    // scrbp_make_warning_colormatch_mirror
    // Might be adding multiple things in one call?
    private Dictionary<string, FuncData> funcLookup = new Dictionary<string, FuncData>() {
        { "scrbp_make_warning_colormatch", new FuncData() { layer = "BattleWarningOver", colorIndex = 3} },
        { "scrbp_make_warning_colormatch_targ", new FuncData() { layer = "BattleEffect", colorIndex = 2} },
        { "scrbp_make_warning_colormatch2_targ", new FuncData() { layer = "BattleWarningUnder", colorIndex = 2} },
        { "scrbp_make_warning_colormatch3", new FuncData() { layer = "BattleWarningOver", colorIndex = 3} },
        { "scrbp_make_warning_colormatch3_targ", new FuncData() { layer = "BattleEffect", colorIndex = 2} },
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
            var scriptMirror = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrbp_make_warning_colormatch_mirror") - 100000);
            var hookMirror = hooks.CreateHook<ScriptDelegate>(this.ColormatchWarnMirrorDetour, scriptMirror->Functions->Function)!;
            hookMirror.Activate().Enable();
            ScriptHooks["scrbp_make_warning_colormatch_mirror"] = hookMirror;
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

    private void RecolorRValue(RValue* color, long colorId) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            double colorToUse = rnsReloaded.utils.RValueToDouble(color);
            switch (colorId) {
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
                    this.logger.PrintMessage("Couldn't figure out what color to use. for id " + colorId, this.logger.ColorRed);
                    break;
            }

            if (color->Type == RValueType.Real || color->Type == RValueType.Int32 || color->Type == RValueType.Int64) {
                color->Type = RValueType.Real;
                color->Real = colorToUse;
            } else if (color->Type == RValueType.Array) {
                color->Get(0)->Type = RValueType.Real;
                color->Get(0)->Real = colorToUse;
            } else {
                this.logger.PrintMessage("Unknown color type: " + color->Type + ", value: " + color->ToString(), this.logger.ColorRed);
            }
        }
    }

    private RValue* ColormatchWarnDetour(
        string name, CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        var hook = ScriptHooks[name];
        returnValue = hook.OriginalFunction(self, other, returnValue, argc, argv);

        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            CLayerInstanceElement* ring = this.GetMostRecentObjectFromLayer(this.funcLookup[name].layer);
            if (ring != null) {
                RValue* color = rnsReloaded.FindValue(ring->Instance, "color");
                if (color == null) {
                    this.logger.PrintMessage("Color is null", this.logger.ColorRedLight);
                    return returnValue;
                }
                RValue* colorIdPtr = this.funcLookup[name].colorIndex < argc ? argv[this.funcLookup[name].colorIndex] : null;
                long colorId = colorIdPtr == null ? -1 : rnsReloaded.utils.RValueToLong(colorIdPtr);
                this.RecolorRValue(color, colorId);

            }
        }
        return returnValue;
    }

    // Mirror creates multiple elements in the layer, supposedly.
    private RValue* ColormatchWarnMirrorDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        var hook = ScriptHooks["scrbp_make_warning_colormatch_mirror"];
        returnValue = hook.OriginalFunction(self, other, returnValue, argc, argv);

        // scrbp_make_warning_colormatch_mirror(0, 250, 4000, 4, 4, 5, 5) -> undefined
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {

            int numCircles = argc - 3; // First 3 args aren't colors.
            List<long> colorIds = new List<long>();
            for (int i = 0; i < numCircles; i++) {
                colorIds.Add(rnsReloaded.utils.RValueToLong(argv[i + 3]));
            }
            var layer = this.FindLayer("BattleEffect");
            List<nint> elems = new List<nint>();
            if (layer != null) {
                CLayerElementBase* elem = layer->Elements.First;
                CLayerElementBase* maxElem = elem;
                while (elem != null) {
                    elems.Add((nint) elem);
                    elem = elem->Next;
                }
            } else {
                this.logger.PrintMessage("Failed to find layer BattleEffect", this.logger.ColorRed);
                return returnValue;
            }
            elems.Sort((a, b) => {
                var elemA = (CLayerElementBase*) a;
                var elemB = (CLayerElementBase*) b;
                return elemB->ID - elemA->ID; // Descending order
            });

            for (int i = 0; i < Math.Min(numCircles, elems.Count); i++) {
                CLayerInstanceElement* ring = (CLayerInstanceElement*) elems[i];
                if (ring != null) {
                    RValue* color = rnsReloaded.FindValue(ring->Instance, "color");
                    if (color == null) {
                        this.logger.PrintMessage("Color is null at ring " + i, this.logger.ColorRedLight);
                    } else {
                        double colorToUse = rnsReloaded.utils.RValueToDouble(color);
                        this.RecolorRValue(color, colorIds[colorIds.Count - (i + 1)]);
                    }
                } else {
                    this.logger.PrintMessage("Ring is null at ring " + i, this.logger.ColorRedLight);
                }
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
