namespace RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

using Position = (double x, double y);
using PosRot = ((double x, double y) position, double angle);

public unsafe interface IBattlePatterns {
    // Passing 0 or 1 to colormatch color gives a weird vibrating blue that doesn't spin
    // Passing 7+ gives more weird blue variants that don't spin
    public const int COLORMATCH_PURPLE = 2;
    public const int COLORMATCH_BLUE = 3;
    public const int COLORMATCH_RED = 4;
    public const int COLORMATCH_YELLOW = 5;
    public const int COLORMATCH_GREEN = 6;

    // Passing 7+ crashes the game
    // White and purple have the same buff name
    public const int FIELDLIMIT_WHITE = 1;
    public const int FIELDLIMIT_PURPLE = 2;
    public const int FIELDLIMIT_BLUE = 3;
    public const int FIELDLIMIT_RED = 4;
    public const int FIELDLIMIT_YELLOW = 5;
    public const int FIELDLIMIT_GREEN = 6;

    public RValue? bpsw_circlespr_default(CInstance* self, CInstance* other, RValue scale);

    public RValue? bpsw_conespr_default(CInstance* self, CInstance* other, RValue scale);

    public RValue? bpsw_plnum(CInstance* self, CInstance* other, RValue p1, RValue p2, RValue p3, RValue p4);

    public void apply_hbs_synced(
        CInstance* self, CInstance* other, int? delay = null, int? hbsHitDelay = null, string? hbs = null, int? hbsDuration = null, int? hbsStrength = null, int? targetMask = null
    );

    public void bind_h(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? targetMask = null
    );

    public void bind_v(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? targetMask = null
    );

    public void bullet_enlarge(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? timeBetween = null, double? scale = null, double? scaleInc = null, int? num = null, Position[]? positions = null);

    public void cardinal_r(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warningDelay2 = null, int? displayNumber = null, int? spawnDelay = null, int? eraseDelay = null, Position? position = null, double? rot = null, int? speed = null, int? width = null
    );

    public void circle_position(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? radius = null, Position[]? positions = null
    );

    public void circle_spreads(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? radius = null, int? targetMask = null
    );

    public void cleave(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, (double rotation, int? targetMask)[]? cleaves = null
    );

    public void cleave_enemy(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? angle = null
    );

    public void cleave_fixed(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, PosRot[]? positions = null
    );

    public void clockspot(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warningDelay2 = null, int? warnMsg = null, int? displayNumber = null, int? spawnDelay = null, int? radius = null, int? fanAngle = null, Position? position = null, int? targetMask = null
    );

    public void colormatch(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? radius = null, int? targetMask = null, int? color = null
    );

    public void cone_direction(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? fanAngle = null, Position? position = null, double[]? rots = null
    );

    public void cone_spreads(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? fanAngle = null, Position? position = null, int? targetMask = null
    );

    public void dark2_cr_circle(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? speed = null, int? angle = null, Position? position = null
    );

    public void dark_targeted(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? scale = null, Position[]? positions = null
    );

    // Display number maxes at 6
    public void displaynumbers(
        CInstance* self, CInstance* other, int? displayNumber = null, int? warningDelay = null, int? spawnDelay = null, Position[]? positions = null
    );

    public void enrage(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? timeBetween = null, bool? resetAnim = null);

    public void enrage_deco(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null);

    // (x, y) refers to the center of the field. Element is which color (purple, yellow, red, blue)
    public void fieldlimit_rectangle(
        CInstance* self, CInstance* other, Position? position = null, int? width = null, int? height = null, int? color = null, int? targetMask = null
    );

    public void fieldlimit_rectangle_temporary(
        CInstance* self, CInstance* other, Position? position = null, int? width = null, int? height = null, int? color = null, int? targetMask = null, int? eraseDelay = null
    );

    public void fire_aoe(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? scale = null, Position[]? positions = null
    );

    public void fire2_line(
        CInstance* self, CInstance* other, int? warningDelay = null, int? showWarning = null, int? spawnDelay = null, Position? position = null, double? angle = null, double? lineAngle = null,
        int? lineLength = null, int? numBullets = null, int? spd = null
    );

    public void gravity_fall(
        CInstance* self, CInstance* other, double? mult = null
    );

    public void gravity_fall_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, double? mult = null, int? targetMask = null
    );

    public void gravity_pull(
        CInstance* self, CInstance* other, double? mult = null
    );

    public void gravity_pull_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, double? mult = null, int? targetMask = null
    );

    public void heavy(
        CInstance* self, CInstance* other, int? targetMask = null
    );

    public void heavy_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? hbsDuration = null, int? targetMask = null
    );

    public void heavyextra(
        CInstance* self, CInstance* other, int? targetMask = null
    );

    public void heavyextra_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? hbsDuration = null, int? targetMask = null
    );

    public void invulncancel(
        CInstance* self, CInstance* other, int? delay = null, int? targetMask = null
    );

    public void knockback_circle(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? kbAmount = null, int? radius = null,
        Position? position = null
    );

    public void knockback_line(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? kbAmount = null, Position? position = null, bool? horizontal = null, int? targetMask = null
    );

    public void light_crosswave(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? rotation = null,
        Position? position = null
    );

    public void light_line(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, Position? position = null, int? lineAngle = 0, int? angle = null, int? spd = null, int? lineLength = null, int? numBullets = null, int? type = null, bool? showWarning = null
    );

    public void line_direction(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? width = null, PosRot[]? positions = null
    );

    public void line_spreads_h(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? width = null, int? targetMask = null
    );

    public void line_spreads_v(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? width = null, int? targetMask = null
    );

    public void marching_bullet(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? timeBetween = null, double? scale = null, Position[]? positions = null);

    public void movementcheck(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, bool? shouldMove = null, int? radius = null, int? targetMask = null
    );

    public void move_position_synced(
        CInstance* self, CInstance* other, int? spawnDelay = null, bool? resetAnim = null, int? duration = null, Position? position = null
    );

    public void painsplit(
        CInstance* self, CInstance* other, bool isPrimary
    );

    public void prscircle(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? radius = null, int? numBullets = null, int? speed = null, Position? position = null, int? angle = null
    );

    public void prscircle_follow(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? radius = null, int? numBullets = null, int? speed = null, int? targetId = null
    );

    public void prscircle_follow_bin(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? radius = null, int? numBullets = null, int? speed = null, int? targetMask = null
    );

    public void prsline_h(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? width = null, int? offset = null, int? speed = null, double? yPosition = null
    );

    public void prsline_h_follow(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? width = null, int? offset = null, int? speed = null, int? targetId = null
    );

    public void prsline_v(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? width = null, int? offset = null, int? speed = null, double? xPosition = null
    );

    public void ray_multi_h(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, Position[]? positions = null);

    public void ray_multi_slice(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? timeBetween = null, int? width = null, PosRot[]? positions = null);

    public void ray_multi_v(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, Position[]? positions = null);

    public void ray_single(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, Position? position = null, int? angle = null);

    public void ray_spinfast(CInstance* self, CInstance* other, int? warningDelay = null, int? warningRadius = null, int? displayNumber = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, double? angle = null, Position? position = null, double? rot = null, int? numLasers = null);

    public void showgroups(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, (int, int, int, int)? groupMasks = null
    );

    public void showorder(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, int? timeBetween = null, (int, int, int, int)? orderMasks = null
    );

    public void tailwind(
        CInstance* self, CInstance* other, int? eraseDelay = null
    );

    public void tailwind_permanent(
        CInstance* self, CInstance* other
    );

    public void tether(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? radius = null, int? targetMask = null
    );

    public void tether_enemy(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? radius = null, Position? position = null, int? targetMask = null
    );

    public void tether_fixed(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? radius = null, Position? position = null, int? targetMask = null
    );

    public void thorns(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, double? radius = null, int? targetMask = null
    );

    public void thorns_fixed(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? spawnDelay = null, double? radius = null, Position? position = null, int? targetMask = null
    );

    public void water2_line(
        CInstance* self, CInstance* other, int? warningDelay = null, int? showWarning = null, int? spawnDelay = null, Position? position = null, double? angle = null, double? lineAngle = null,
        int? lineLength = null, int? numBullets = null, int? spd = null
    );
}
