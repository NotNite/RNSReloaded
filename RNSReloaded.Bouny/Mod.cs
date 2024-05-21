using System.Drawing;
using System.Runtime.InteropServices;
using DearImguiSharp;
using Reloaded.Hooks.Definitions;
using Reloaded.Imgui.Hook;
using Reloaded.Imgui.Hook.Direct3D11;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.Bouny;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private bool ready = false;

    public void Start(IModLoaderV1 loader) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>()!;
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        this.logger = loader.GetLogger();

        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.OnReady += this.Ready;
        }

        if (this.hooksRef != null && this.hooksRef.TryGetTarget(out var hooks)) {
            SDK.Init(hooks);
            ImguiHook.Create(this.Draw, new ImguiHookOptions() {
                Implementations = [new ImguiHookDx11()]
            });
        }
    }

    public void Ready() => this.ready = true;

    private static string GlobalSearchQuery = string.Empty;
    private static List<int> OpenLayers = new();
    private static List<int> OpenElements = new();
    private static Dictionary<int, string> InstanceSearchQueries = new();

    private void Draw() {
        if (!this.ready) return;
        var open = true;
        var buttonSize = new ImVec2 {
            X = 0,
            Y = 0
        };

        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            var room = rnsReloaded.GetCurrentRoom();

            var global = rnsReloaded.GetGlobalInstance();
            if (global != null) {
                var globalValue = new RValue(global);
                if (ImGui.Begin("Globals", ref open, 0)) {
                    var dict = this.Enumerate(rnsReloaded, &globalValue, ignoreFunctions: true);
                    this.DrawSearch(ref GlobalSearchQuery, dict);
                }
                ImGui.End();
            }

            if (room != null) {
                var layerMap = new Dictionary<int, nint>();
                var elementMap = new Dictionary<int, nint>();
                var layerElementMap = new Dictionary<int, List<nint>>();

                {
                    var layers = room->Layers;
                    var layer = layers.First;

                    while (layer != null) {
                        layerMap[layer->ID] = (nint) layer;
                        layerElementMap[layer->ID] = new List<nint>();

                        var elements = layer->Elements;
                        var element = elements.First;
                        while (element != null) {
                            elementMap[element->ID] = (nint) element;
                            layerElementMap[layer->ID].Add((nint) element);
                            element = element->Next;
                        }

                        layer = layer->Next;
                    }
                }

                if (ImGui.Begin("Room", ref open, 0)) {
                    foreach (var (id, layerPtr) in layerMap.OrderBy(x => x.Key)) {
                        var layer = (CLayer*) layerPtr;
                        var name = layer->Name == null ? "Unnamed" : Marshal.PtrToStringAnsi((nint) layer->Name)!;

                        if (ImGui.Button($"{name} ({layer->ID})##layer_toggle_{layer->ID}", buttonSize)) {
                            if (OpenLayers.Contains(layer->ID)) {
                                OpenLayers.Remove(layer->ID);
                            } else {
                                OpenLayers.Add(layer->ID);
                            }
                        }
                    }
                }
                ImGui.End();

                foreach (var id in OpenLayers.ToList()) {
                    if (!layerMap.TryGetValue(id, out var value)) {
                        OpenLayers.Remove(id);
                        continue;
                    }

                    var layer = (CLayer*) value;
                    var layerName = layer->Name == null ? "Unnamed" : Marshal.PtrToStringAnsi((nint) layer->Name)!;
                    var layerOpen = true;
                    if (ImGui.Begin(layerName, ref layerOpen, 0)) {
                        foreach (var elementPtr in layerElementMap[layer->ID]) {
                            var element = (CLayerElementBase*) elementPtr;
                            var elementName = element->Name == null
                                                  ? "Unnamed"
                                                  : Marshal.PtrToStringAnsi((nint) element->Name)!;

                            if (ImGui.Button($"{elementName} ({element->ID})##element_toggle_{element->ID}",
                                    buttonSize)) {
                                if (OpenElements.Contains(element->ID)) {
                                    OpenElements.Remove(element->ID);
                                } else {
                                    OpenElements.Add(element->ID);
                                }
                            }
                        }
                    }
                    ImGui.End();

                    if (!layerOpen) OpenLayers.Remove(layer->ID);
                }

                foreach (var id in OpenElements.ToList()) {
                    if (!elementMap.TryGetValue(id, out var value)) {
                        OpenElements.Remove(id);
                        continue;
                    }

                    var element = (CLayerElementBase*) value;
                    var name = element->Name == null ? "Unnamed" : Marshal.PtrToStringAnsi((nint) element->Name)!;

                    var elementOpen = true;
                    if (ImGui.Begin(name, ref elementOpen, 0)) {
                        if (element->Type == LayerElementType.Instance) {
                            if (!InstanceSearchQueries.TryGetValue(element->ID, out var searchQuery)) {
                                searchQuery = string.Empty;
                            }

                            var instance = (CLayerInstanceElement*) element;
                            var instanceValue = new RValue(instance->Instance);

                            var dict = this.Enumerate(rnsReloaded, &instanceValue);
                            this.DrawSearch(ref searchQuery, dict);

                            InstanceSearchQueries[element->ID] = searchQuery;
                        }
                    }
                    ImGui.End();

                    if (!elementOpen) OpenElements.Remove(element->ID);
                }
            }
        }
    }

    private Dictionary<string, string> Enumerate(
        IRNSReloaded rnsReloaded,
        RValue* value,
        bool ignoreFunctions = false
    ) {
        var result = new Dictionary<string, string>();
        foreach (var key in rnsReloaded.GetStructKeys(value)) {
            var val = rnsReloaded.GetString(rnsReloaded.FindValue(value->Object, key));
            if (val == "function" && ignoreFunctions) continue;
            result[key] = val;
        }
        return result;
    }

    private void DrawSearch(ref string search, Dictionary<string, string> values) {
        // Damn these ImGui bindings suck
        fixed (char* ptr = search) {
            ImGui.InputText("Search", (sbyte*) ptr, 256, 0, null, nint.Zero);
        }

        ImGui.Separator();

        ImVec2 cra = new();
        ImGui.GetContentRegionAvail(cra);
        if (ImGui.BeginChildStr("##Results", cra, false, 0)) {
            foreach (var (key, value) in values.OrderBy(x => x.Key)) {
                if (key.ToLower().Contains(search.ToLower()) || value.ToLower().Contains(search.ToLower())) {
                    ImGui.TextUnformatted($"{key}: {value}", null);
                }
            }
        }
        ImGui.EndChild();
    }

    public void Suspend() { }
    public void Resume() { }
    public bool CanSuspend() => false;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
