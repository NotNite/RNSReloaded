using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FuzzyMechPackInterfaces;

public unsafe interface IFuzzyMechPack {
    public event Action? OnReady;
    public event Action<ExecuteItArguments>? OnExecuteIt;

    public static IFuzzyMechPack Instance = null!;

    public void ColormatchSwap(CInstance* self, CInstance* other,
        int numColors = 2,
        int? warningDelay = null,
        int? spawnDelay = null,
        int? matchRadius = null,
        int? setRadius = null,
        int? warnMsg = null,
        int? displayNumber = null,
        (int x, int y)?[]? setCircles = null,
        (int x, int y)?[]? matchCircles = null,
        int[]? targetMask = null,
        int[]? colors = null
    );

    public void BuffOnHit(CInstance* self, CInstance* other,
        string? hbsName = null,
        int? hbsDuration = null,
        int? hbsStrength = null,
        int? targetMask = null,
        int? eraseDelay = null,
        int? timeBetweenBuffs = null
    );

}
