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

public enum NotchFlag {
    BOSS = 1,           // Regular area boss
    FINAL_BOSS = 2,     // Final boss
    TOXBOX_BOSS = 4,    // Toybox boss
    SHOPKEEP_CAT = 8,   // Asha shop
    SHOPKEEP_WOLF = 16, // Tassha rich wolf!
    SHOPKEEP_BIRD = 32, // Saya shop
    FLOWER_FIGHT = 64,  // Spawns vine deco/background change
    FINALSONG = 128,    // For the credits, play the last CS song
    MUSICFADE = 256     // For the credits, fade out the music instead after notch
}

public readonly struct Notch {
    public readonly NotchType Type;
    public readonly string Encounter;
    public readonly double Seed;
    public readonly NotchFlag Flags;

    public Notch(NotchType type, string enc, double seed, NotchFlag flags) {
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
