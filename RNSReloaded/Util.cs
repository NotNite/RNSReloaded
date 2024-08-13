using System.Drawing;
using System.Runtime.InteropServices;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded;

public unsafe class Util : IUtil {
    private IRNSReloaded rnsReloaded;
    private ILoggerV1 logger;

    public Util(IRNSReloaded rnsReloaded, ILoggerV1 logger) {
        this.rnsReloaded = rnsReloaded;
        this.logger = logger;
    }

    public long RValueToLong(RValue* arg) {
        string type = arg->Type.ToString();
        return type switch {
            "Real" => (long) (*arg).Real,
            "Int32" => (long) (*arg).Int32,
            "Int64" => (long) (*arg).Int64,
            _ => 0,
        };
    }

    public double RValueToDouble(RValue* arg) {
        string type = arg->Type.ToString();
        return type switch {
            "Real" => (double) (*arg).Real,
            "Int32" => (double) (*arg).Int32,
            "Int64" => (double) (*arg).Int64,
            _ => 0,
        };
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

    public int GetNumPlayers() {
        var playerNum = this.GetGlobalVar("playerNum");
        var numAllies = playerNum->Get(0);
        // In single player this is a real. In multiplayer it's a float. Why?!
        if (numAllies->Type == RValueType.Real) {
            return (int) numAllies->Real;
        }
        if (numAllies->Type == RValueType.Int32) {
            return numAllies->Int32;
        }
        return 1;
    }

    public int GetNumEnemies() {
        var playerNum = this.GetGlobalVar("playerNum");
        var numAllies = playerNum->Get(1);
        if (numAllies->Type == RValueType.Real) {
            return (int) numAllies->Real;
        }
        if (numAllies->Type == RValueType.Int32) {
            return numAllies->Int32;
        }
        return 1;
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

    public double GetEnemyHP(double id) {
        var instance = this.rnsReloaded.GetGlobalInstance();
        var hpList = this.rnsReloaded.FindValue(instance, "playerHp");
        var enemyHps = this.rnsReloaded.ArrayGetEntry(hpList, 1);
        return this.rnsReloaded.ArrayGetEntry(enemyHps, (int) id)->Real;
    }

    public void setHallway(List<Notch> hallway, CInstance* self, IRNSReloaded rnsReloaded) {
        if (hallway.Count > 20) {
            throw new Exception("Hallway too long, 20 elements max");
        }
        var notches = rnsReloaded.FindValue(self, "notches");
        for (int i = 0; i < hallway.Count; i++) {
            var notch = rnsReloaded.ArrayGetEntry(notches, i);
            var notch_type = rnsReloaded.ArrayGetEntry(notch, 0);
            var notch_enemy = rnsReloaded.ArrayGetEntry(notch, 1);
            var notch_seed = rnsReloaded.ArrayGetEntry(notch, 2);
            var notch_flags = rnsReloaded.ArrayGetEntry(notch, 3);

            notch_type->Int32 = (int) hallway[i].Type;
            rnsReloaded.CreateString(notch_enemy, hallway[i].Encounter);
            notch_seed->Real = hallway[i].Seed;
            notch_flags->Real = hallway[i].Flags;
        }
        var notchNum = rnsReloaded.FindValue(self, "notchNumber");
        notchNum->Real = hallway.Count;
    }

}
