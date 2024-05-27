using System.Drawing;
using System.Runtime.InteropServices;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.CustomBossTest;

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

    public RValue* GetGlobalVar(string key) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var instance = rnsReloaded.GetGlobalInstance();
            return rnsReloaded.FindValue(instance, key);
        }
        return null;
    }

    public RValue* GetPlayerVar(int index, string key) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var instance = rnsReloaded.GetGlobalInstance();
            var combatantList = rnsReloaded.FindValue(instance, "player");
            var playerList = combatantList->Get(0);
            var player = playerList->Get(index);
            return player->Get(key);
        }
        return null;
    }

    public RValue* GetEnemyVar(int index, string key) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var instance = rnsReloaded.GetGlobalInstance();
            var combatantList = rnsReloaded.FindValue(instance, "player");
            var enemyList = combatantList->Get(1);
            var enemy = enemyList->Get(index);
            return enemy->Get(key);
        }
        return null;
    }
}
