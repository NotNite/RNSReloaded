using System.Drawing;
using System.Runtime.InteropServices;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.CustomBossTest;

public unsafe class Util {
    private IRNSReloaded rnsReloaded;
    private ILoggerV1 logger;

    public Util(IRNSReloaded rnsReloaded, ILoggerV1 logger) {
        this.rnsReloaded = rnsReloaded;
        this.logger = logger;
    }

    public RoutineDelegate? GetCodeFunction(string name) {
        var id = this.rnsReloaded.CodeFunctionFind(name)!.Value;
        var funcRef = this.rnsReloaded.GetTheFunction(id);
        var func = Marshal.GetDelegateForFunctionPointer<RoutineDelegate>((nint) funcRef.Routine);
        return func;
    }

    public ScriptDelegate? GetScriptFunction(string name) {
        var id = this.rnsReloaded.ScriptFindId(name) - 100000;
        if (id != 0) {
            var script = this.rnsReloaded.GetScriptData(id);
            if (script != null) {
                var funcRef = script->Functions->Function;
                var func = Marshal.GetDelegateForFunctionPointer<ScriptDelegate>(funcRef);
                return func;
            }
        }
        return null;
    }

    public RValue? CreateString(string str) {
        RValue result;
        this.rnsReloaded.CreateString(&result, str);
        return result;
    }

    public RValue* GetGlobalVar(string key) {
        var instance = this.rnsReloaded.GetGlobalInstance();
        return this.rnsReloaded.FindValue(instance, key);
    }

    public RValue* GetPlayerVar(int index, string key) {
        var instance = this.rnsReloaded.GetGlobalInstance();
        var combatantList = this.rnsReloaded.FindValue(instance, "player");
        var playerList = combatantList->Get(0);
        var player = playerList->Get(index);
        return player->Get(key);
    }

    public RValue* GetEnemyVar(int index, string key) {
        var instance = this.rnsReloaded.GetGlobalInstance();
        var combatantList = this.rnsReloaded.FindValue(instance, "player");
        var enemyList = combatantList->Get(1);
        var enemy = enemyList->Get(index);
        return enemy->Get(key);
    }
}
