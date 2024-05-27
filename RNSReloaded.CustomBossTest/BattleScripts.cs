using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.CustomBossTest;

public unsafe class BattleScripts {
    private IRNSReloaded rnsReloaded;
    private Util utils;
    private ILoggerV1 logger;

    public BattleScripts(IRNSReloaded rnsReloaded, Util utils, ILoggerV1 logger) {
        this.rnsReloaded = rnsReloaded;
        this.utils = utils;
        this.logger = logger;
    }

    public bool time(CInstance* self, CInstance* other, int time) {
        RValue[] args = [new RValue(time)];
        return this.rnsReloaded.ExecuteScript("scrbp_time", self, other, args)!.Value.Real > 0.5;
    }

    public bool time_repeating(CInstance* self, CInstance* other, int loopOffset, int loopLength) {
        RValue[] args = [new RValue(loopOffset), new RValue(loopLength)];
        return this.rnsReloaded.ExecuteScript("scrbp_time_repeating", self, other, args)!.Value.Real > 0.5;
    }

    public void move_character(CInstance* self, CInstance* other, double x, double y, int moveTime) {
        RValue[] args = [new RValue(x), new RValue(y), new RValue(moveTime), new RValue(0)];
        this.rnsReloaded.ExecuteScript("scrbp_move_character", self, other, args);
    }

    public void move_character_absolute(CInstance* self, CInstance* other, double x, double y, int moveTime) {
        RValue[] args = [new RValue(x), new RValue(y), new RValue(moveTime), new RValue(0)];
        this.rnsReloaded.ExecuteScript("scrbp_move_character_absolute", self, other, args);
    }

}
