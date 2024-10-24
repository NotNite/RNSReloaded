using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded;

public unsafe class BattleScripts : IBattleScripts {
    private IRNSReloaded rnsReloaded;
    private IUtil utils;
    private ILoggerV1 logger;

    public BattleScripts(IRNSReloaded rnsReloaded, IUtil utils, ILoggerV1 logger) {
        this.rnsReloaded = rnsReloaded;
        this.utils = utils;
        this.logger = logger;
    }

    public void end(CInstance* self, CInstance* other) {
        this.rnsReloaded.ExecuteScript("scrbp_end", self, other, []);
    }
    public void heal(CInstance* self, CInstance* other, double amount) {
        this.rnsReloaded.ExecuteScript("scrbp_boss_heal", self, other, [new RValue(amount)]);
    }

    public bool health_threshold(CInstance* self, CInstance* other, double threshold) {
        return this.rnsReloaded.ExecuteScript("scrbp_health_threshold", self, other, [new RValue(threshold)])!.Value.Real > 0.5;
    }

    public void make_number_warning(CInstance* self, CInstance* other, int x, int y, int displayNumber, int duration) {
        RValue unsetVal = new RValue(0);
        unsetVal.Type = RValueType.Undefined;
        // Not sure what the last 3 args here are for (true, undef, undef)
        this.rnsReloaded.ExecuteScript("scrbp_make_number_warning", self, other, [new RValue(x), new RValue(y), new RValue(displayNumber), new RValue(duration), new RValue(true), unsetVal, unsetVal]);
    }

    public void make_warning_colormatch(CInstance* self, CInstance* other, int x, int y, int radius, int element, int duration) {
        this.rnsReloaded.ExecuteScript("scrbp_make_warning_colormatch", self, other, [new RValue(x), new RValue(y), new RValue(radius), new RValue(element), new RValue(duration)]);
    }

    public void make_warning_colormatch_burst(CInstance* self, CInstance* other, int x, int y, int radius, int element) {
        this.rnsReloaded.ExecuteScript("scrbp_make_warning_colormatch_burst", self, other, [new RValue(x), new RValue(y), new RValue(radius), new RValue(element)]);
    }

    public void make_warning_colormatch_targ(CInstance* self, CInstance* other, int playerId, int radius, int element, int duration) {
        this.rnsReloaded.ExecuteScript("scrbp_make_warning_colormatch_targ", self, other, [new RValue(playerId), new RValue(radius), new RValue(element), new RValue(duration)]);
    }

    public void move_character(CInstance* self, CInstance* other, double x, double y, int moveTime) {
        RValue[] args = [new RValue(x), new RValue(y), new RValue(moveTime), new RValue(0)];
        this.rnsReloaded.ExecuteScript("scrbp_move_character", self, other, args);
    }

    public void move_character_absolute(CInstance* self, CInstance* other, double x, double y, int moveTime) {
        RValue[] args = [new RValue(x), new RValue(y), new RValue(moveTime), new RValue(0)];
        this.rnsReloaded.ExecuteScript("scrbp_move_character_absolute", self, other, args);
    }

    public void set_special_flags(CInstance* self, CInstance* other, int flags) {
        this.rnsReloaded.ExecuteScript("scrbp_set_special_flags", self, other, [new RValue(flags)]);
    }

    public void pattern_set_color_colormatch(CInstance* self, CInstance* other, int color) {
        this.rnsReloaded.ExecuteScript("scrbp_pattern_set_color_colormatch", self, other, [new RValue(color)]);

    }

    public void pattern_set_drawlayer(CInstance* self, CInstance* other, int drawlayerId) {
        this.rnsReloaded.ExecuteScript("scrbp_pattern_set_drawlayer", self, other, [new RValue(drawlayerId)]);
    }

    public void pattern_set_projectile_key(CInstance* self, CInstance* other, string key, string sfxset) {
        RValue keyVal = new RValue(0);
        RValue sfxSetVal = new RValue(0);

        this.rnsReloaded.CreateString(&keyVal, key);
        this.rnsReloaded.CreateString(&sfxSetVal, sfxset);
        this.rnsReloaded.ExecuteScript("scrbp_pattern_set_projectile_key", self, other, [keyVal, sfxSetVal]);
    }

    public void phase_pattern_remove(CInstance* self, CInstance* other) {
        this.rnsReloaded.ExecuteScript("scrbp_phase_pattern_remove", self, other, []);
    }

    // I don't know what the args actually mean. Maybe something to do with volume?
    public void sound(CInstance* self, CInstance* other, double arg1, double arg2) {
        RValue undefVal = new RValue(0);
        undefVal.Type = RValueType.Undefined;
        this.rnsReloaded.ExecuteScript("scrbp_sound", self, other, [new RValue(arg1), new RValue(arg2), undefVal, undefVal]);
    }

    public void sound_x(CInstance* self, CInstance* other, int x) {
        this.rnsReloaded.ExecuteScript("scrbp_sound_x", self, other, [new RValue(x)]);
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

    public int[] order_random(CInstance* self, CInstance* other, bool excludeKO, params int[] groupings) {
        RValue[] args = [new RValue(excludeKO)];
        args = args.Concat(groupings.Select(x => new RValue(x)).ToArray()).ToArray();
        this.rnsReloaded.ExecuteScript("scrbp_order_random", self, other, args);

        // Return the results for easy use
        int[] results = new int[groupings.Length];
        var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
        for (int i = 0; i < groupings.Length; i++) {
            results[i] = (int) this.rnsReloaded.ArrayGetEntry(orderBin, i)->Real;
        }
        return results;
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

    public void warning_msg_pos(CInstance* self, CInstance* other, int x, int y, string msg, int warnMsg, int duration) {
        RValue msgVal = new RValue(0);
        this.rnsReloaded.CreateString(&msgVal, msg);
        this.rnsReloaded.ExecuteScript("scrbp_warning_msg_pos", self, other, [new RValue(x), new RValue(y), msgVal, new RValue(warnMsg), new RValue(duration)]);
    }

    public void warning_msg_t(CInstance* self, CInstance* other, int playerId, string msg, int warnMsg, int duration) {
        RValue msgVal = new RValue(0);
        this.rnsReloaded.CreateString(&msgVal, msg);
        RValue undefVal = new RValue(0);
        undefVal.Type = RValueType.Undefined;
        this.rnsReloaded.ExecuteScript("scrbp_warning_msg_t", self, other, [new RValue(playerId), msgVal, new RValue(warnMsg), new RValue(duration), undefVal]);
    }

    public RValue sbgv(CInstance* self, CInstance* other, string name, RValue defaultVal) {
        RValue key = new RValue(0);
        this.rnsReloaded.CreateString(&key, name);
        return this.rnsReloaded.ExecuteScript("sbgv", self, other, [key, defaultVal])!.Value;
    }

    public RValue sbsv(CInstance* self, CInstance* other, string name, RValue toSave) {
        RValue key = new RValue(0);
        this.rnsReloaded.CreateString(&key, name);
        return this.rnsReloaded.ExecuteScript("sbsv", self, other, [key, toSave])!.Value;
    }

}
