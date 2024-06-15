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

    public void move_character(CInstance* self, CInstance* other, double x, double y, int moveTime) {
        RValue[] args = [new RValue(x), new RValue(y), new RValue(moveTime), new RValue(0)];
        this.rnsReloaded.ExecuteScript("scrbp_move_character", self, other, args);
    }

    public void move_character_absolute(CInstance* self, CInstance* other, double x, double y, int moveTime) {
        RValue[] args = [new RValue(x), new RValue(y), new RValue(moveTime), new RValue(0)];
        this.rnsReloaded.ExecuteScript("scrbp_move_character_absolute", self, other, args);
    }

    public bool time(CInstance* self, CInstance* other, int time) {
        RValue[] args = [new RValue(time)];
        return this.rnsReloaded.ExecuteScript("scrbp_time", self, other, args)!.Value.Real > 0.5;
    }

    public bool time_repeating(CInstance* self, CInstance* other, int loopOffset, int loopLength) {
        RValue[] args = [new RValue(loopOffset), new RValue(loopLength)];
        return this.rnsReloaded.ExecuteScript("scrbp_time_repeating", self, other, args)!.Value.Real > 0.5;
    }

    public bool time_repeat_times(CInstance* self, CInstance* other, int startTime, int timeBetween, int times) {
        RValue[] args = [new RValue(startTime), new RValue(timeBetween), new RValue(times)];
        return this.rnsReloaded.ExecuteScript("scrbp_time_repeat_times", self, other, args)!.Value.Real > 0.5;
    }

    public void order_random(CInstance* self, CInstance* other, bool excludeKO, params int[] groupings) {
        RValue[] args = [new RValue(excludeKO)];
        args = args.Concat(groupings.Select(x => new RValue(x)).ToArray()).ToArray();
        this.rnsReloaded.ExecuteScript("scrbp_order_random", self, other, args);
    }

    public void pattern_deal_damage_enemy_subtract(CInstance* self, CInstance* other, int teamId, int playerId, int damageAmount) {
        RValue[] args = [new RValue(teamId), new RValue(playerId), new RValue(damageAmount)];
        this.rnsReloaded.ExecuteScript("scr_pattern_deal_damage_enemy_subtract", self, other, args);

        // function scr_pattern_deal_damage_enemy_subtract( tId, pId, damageAmount ){
        //     global.playerHp[tId,pId] -= damageAmount;
        //     global.playerHp[tId,pId] =
        //         math_bound( global.playerHp[tId,pId],
        //             0, global.playerStat[tId][pId][stat.hp] );
        //
        //     if( global.playerSpecialFlags[tId][pId] & SP_FLAG_HOLMGANG > 0 ) {
        //         global.playerHp[tId,pId] = max(1,global.playerHp[tId,pId]);
        //     }
        //     if( scr_obswitch(false,false,true) ) { // clients can't kill bosses
        //         global.playerHp[tId,pId] = max(1,global.playerHp[tId,pId]);
        //     }
        //     if( scr_obswitch(false,false,true) && !global.notchExOnlineReady ) { // clients can't kill bosses
        //         global.playerHp[tId,pId] = max(1,global.playerHp[tId,pId]);
        //     }
        //
        //     else if( global.playerHp[tId,pId] <= 0 ) { // send boss kill immediately
        //         scr_udpcont_send_now();
        //     }
        // }
    }

}
