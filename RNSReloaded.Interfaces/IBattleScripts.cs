using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.Interfaces;

public unsafe interface IBattleScripts {
    public const int FLAG_HOLMGANG      = 0b0001;
    public const int FLAG_NO_POSITIONAL = 0b0010;
    public const int FLAG_NO_TARGET     = 0b0100;

    public void end(CInstance* self, CInstance* other);

    public void heal(CInstance* self, CInstance* other, double amount);
    public bool health_threshold(CInstance* self, CInstance* other, double threshold);
    public void move_character(CInstance* self, CInstance* other, double x, double y, int moveTime);
    public void move_character_absolute(CInstance* self, CInstance* other, double x, double y, int moveTime);
    public void set_special_flags(CInstance* self, CInstance* other, int flags);

    public void phase_pattern_remove(CInstance* self, CInstance* other);

    public bool time(CInstance* self, CInstance* other, int time);

    public bool time_repeating(CInstance* self, CInstance* other, int loopOffset, int loopLength);

    public bool time_repeat_times(CInstance* self, CInstance* other, int startTime, int timeBetween, int times);

    public int[] order_random(CInstance* self, CInstance* other, bool excludeKO, params int[] groupings);

    public void pattern_deal_damage_enemy_subtract(CInstance* self, CInstance* other, int teamId, int playerId, int damageAmount);

}
