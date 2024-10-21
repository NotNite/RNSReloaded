using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.Interfaces;

public enum NotchType {
    IntroRoom = 0,
    Encounter = 1,
    Boss = 2,
    FinalBoss = 3,
    TutorialEncounter = 4,
    Chest = 5,
    Shop = 6,
    PinnacleCutscene = 7,
    EndRun = 12
}

public readonly struct Notch {
    public const double BOSS_FLAG = 1;
    public const double FINAL_BOSS_FLAG = 3;

    public readonly NotchType Type;
    public readonly string Encounter;
    public readonly double Seed;
    public readonly double Flags;

    public Notch(NotchType type, string enc, double seed, double flags) {
        this.Type = type;
        this.Encounter = enc;
        this.Seed = seed;
        this.Flags = flags;
    }
}

public unsafe interface IUtil {
    public long RValueToLong(RValue* arg);

    public long RValueToLong(RValue arg);

    public double RValueToDouble(RValue* arg);

    public RValue? CreateString(string str);

    public RValue* GetGlobalVar(string key);

    public int GetNumPlayers();

    public int GetNumEnemies();

    public RValue* GetPlayerVar(int index, string key);

    public RValue* GetEnemyVar(int index, string key);

    public double GetEnemyHP(double id);

    public void setHallway(List<Notch> hallway, CInstance* self, IRNSReloaded rnsReloaded);

}
