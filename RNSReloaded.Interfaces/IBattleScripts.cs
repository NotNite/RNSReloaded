using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.Interfaces;

public unsafe interface IBattleScripts {
    public void move_character(CInstance* self, CInstance* other, double x, double y, int moveTime);
    public void move_character_absolute(CInstance* self, CInstance* other, double x, double y, int moveTime);

    public bool time(CInstance* self, CInstance* other, int time);

    public bool time_repeating(CInstance* self, CInstance* other, int loopOffset, int loopLength);

    public bool time_repeat_times(CInstance* self, CInstance* other, int startTime, int timeBetween, int times);

    public int[] order_random(CInstance* self, CInstance* other, bool excludeKO, params int[] groupings);

    public void pattern_deal_damage_enemy_subtract(CInstance* self, CInstance* other, int teamId, int playerId, int damageAmount);

}
