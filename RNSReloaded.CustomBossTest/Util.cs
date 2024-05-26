using System.Drawing;
using System.Runtime.InteropServices;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.CustomBossTest {
    public unsafe class Util {
        private WeakReference<IRNSReloaded> rnsReloadedRef;
        private ILoggerV1 logger;

        public Util(WeakReference<IRNSReloaded> rnsReloadedRef, ILoggerV1 logger) {
            this.rnsReloadedRef = rnsReloadedRef;
            this.logger = logger;
        }

        public RoutineDelegate? GetCodeFunction(string name) {
            if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
                var id = rnsReloaded.CodeFunctionFind(name)!.Value;
                var funcRef = rnsReloaded.GetTheFunction(id);
                var func = Marshal.GetDelegateForFunctionPointer<RoutineDelegate>((nint) funcRef.Routine);
                return func;
            }
            return null;
        }

        public ScriptDelegate? GetScriptFunction(string name) {
            if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
                var id = rnsReloaded.ScriptFindId(name) - 100000;
                if (id != 0) {
                    var script = rnsReloaded.GetScriptData(id);
                    if (script != null) {
                        var funcRef = script->Functions->Function;
                        var func = Marshal.GetDelegateForFunctionPointer<ScriptDelegate>(funcRef);
                        return func;
                    }
                }
            }
            return null;
        }

        public RValue? CreateString(string str) {
            if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
                RValue result;
                rnsReloaded.CreateString(&result, str);
                return result;
            }
            return null;
        }

        public RValue* GetPlayerVar(string key) {
            if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
                var instance = rnsReloaded.GetGlobalInstance();
                var combatantList = rnsReloaded.FindValue(instance, "player");
                var playerList = combatantList->Get(0);
                var player_0 = playerList->Get(0);
                return player_0->Get(key);
            }
            return null;
        }

        public RValue? RunGMLScript(string name, CInstance* self, CInstance* other, RValue[]? args) {
            if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
                var script = this.GetScriptFunction(name);
                if (script == null) {
                    this.logger.PrintMessage($"script {name} could not be found", Color.Red);
                }
                if (script != null) {
                    //this.logger.PrintMessage($"script = '{script}'", Color.Red);
                    fixed (RValue* argsPtr = args) {
                        var argsArr = args == null ? null : new RValue*[args.Length];
                        RValue returnValue;
                        if (argsArr != null) {
                            for (var i = 0; i < args!.Length; i++) argsArr[i] = &argsPtr[i];
                        }
                        fixed (RValue** argsPtrPtr = argsArr) {
                            //this.logger.PrintMessage($"cargs = {args.Length}", Color.Red);
                            script(self, other, &returnValue, args == null ? 0 : args.Length, argsPtrPtr);
                            //this.logger.PrintMessage($"result = {returnValue}", Color.Red);
                        }
                        return returnValue;
                    }
                }
            }
            return null;
        }

    }
}
