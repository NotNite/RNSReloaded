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

public static class NotchFlag {
    public const int BOSS = 1;           // Regular area boss
    public const int FINAL_BOSS = 2;     // Final boss
    public const int TOXBOX_BOSS = 4;    // Toybox boss
    public const int SHOPKEEP_CAT = 8;   // Asha shop
    public const int SHOPKEEP_WOLF = 16; // Tassha rich wolf!
    public const int SHOPKEEP_BIRD = 32; // Saya shop
    public const int FLOWER_FIGHT = 64;  // Spawns vine deco/background change
    public const int FINALSONG = 128;    // For the credits, play the last CS song
    public const int MUSICFADE = 256;    // For the credits, fade out the music instead after notch
}

public readonly struct Notch {
    public readonly NotchType Type;
    public readonly string Encounter;
    public readonly double Seed;
    public readonly int Flags;

    public Notch(NotchType type, string enc, double seed, int flags) {
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
