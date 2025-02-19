using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded;

using Position = (double x, double y);
using PosRot = ((double x, double y) position, double angle);

public unsafe class BattlePatterns : IBattlePatterns {
    private IRNSReloaded rnsReloaded;
    private IUtil utils;
    private ILoggerV1 logger;

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

    public BattlePatterns(IRNSReloaded rnsReloaded, IUtil utils, ILoggerV1 logger) {
        this.rnsReloaded = rnsReloaded;
        this.utils = utils;
        this.logger = logger;
    }

    void set_pattern_positions(CInstance* self, CInstance* other, Position[] positions) {
        RValue[] args = [];
        for (int i = 0; i < positions.Length; i++) {
            Position point = positions[i];
            args = args.Concat([
                new RValue(point.x),
                new RValue(point.y)
            ]).ToArray();
        }
        this.rnsReloaded.ExecuteScript("bpatt_positions", self, other, args);
    }

    void set_pattern_position_rotations(CInstance* self, CInstance* other, PosRot[] positions) {
        RValue[] args = [];
        for (int i = 0; i < positions.Length; i++) {
            args = args.Concat([
                new RValue(positions[i].position.x),
                new RValue(positions[i].position.x),
                new RValue(positions[i].angle)
            ]).ToArray();
        }
        this.rnsReloaded.ExecuteScript("bpatt_posrot", self, other, args);
    }

    void execute_pattern(CInstance* self, CInstance* other, string pattern, RValue[] args) {
        this.rnsReloaded.ExecuteScript("bpatt_var", self, other, args);
        args = [new RValue(this.rnsReloaded.ScriptFindId(pattern))];
        this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);
        this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
    }

    private RValue[] add_if_not_null(RValue[] args, string fieldName, int? value) {
        if (value != null) {
            return args.Concat([this.utils.CreateString(fieldName)!.Value, new RValue(value.Value)]).ToArray();
        }
        return args;
    }

    private RValue[] add_if_not_null(RValue[] args, string fieldName, bool? value) {
        if (value != null) {
            return args.Concat([this.utils.CreateString(fieldName)!.Value, new RValue(value.Value)]).ToArray();
        }
        return args;
    }

    private RValue[] add_if_not_null(RValue[] args, string fieldName, double? value) {
        if (value != null) {
            return args.Concat([this.utils.CreateString(fieldName)!.Value, new RValue(value.Value)]).ToArray();
        }
        return args;
    }

    public RValue? bpsw_circlespr_default(CInstance* self, CInstance* other, RValue scale) {
        return this.rnsReloaded.ExecuteScript("bpsw_circlespr_default", self, other, [scale]);
    }

    public RValue? bpsw_conespr_default(CInstance* self, CInstance* other, RValue scale) {
        return this.rnsReloaded.ExecuteScript("bpsw_conespr_default", self, other, [scale]);
    }

    public RValue? bpsw_plnum(CInstance* self, CInstance* other, RValue p1, RValue p2, RValue p3, RValue p4) {
        return this.rnsReloaded.ExecuteScript("bpsw_plnum", self, other, [p1, p2, p3, p4]);
    }

    public void apply_hbs_synced(
        CInstance* self, CInstance* other, int? delay = null, int? hbsHitDelay = null, string? hbs = null, int? hbsDuration = null, int? hbsStrength = null, int? targetMask = null
    ) {
        var hbsInfo = this.utils.GetGlobalVar("hbsInfo");
        for (var i = 0; i < this.rnsReloaded.ArrayGetLength(hbsInfo)!.Value.Real; i++) {
            if (hbsInfo->Get(i)->Get(0)->ToString() == hbs) {
                RValue[] args = [];
                args = this.add_if_not_null(args, "delay", delay);
                args = this.add_if_not_null(args, "hbsHitDelay", hbsHitDelay);
                args = args.Concat([this.utils.CreateString("hbsIndex")!.Value, new RValue(i)]).ToArray();
                args = this.add_if_not_null(args, "hbsDuration", hbsDuration);
                args = this.add_if_not_null(args, "hbsStrength", hbsStrength);
                args = this.add_if_not_null(args, "trgBinary", targetMask);

                this.execute_pattern(self, other, "bp_apply_hbs_synced", args);
                break;
            }
        }
    }

    public void bind_h(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_bind_h", args);
    }

    public void bind_v(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_bind_v", args);
    }

    public void bullet_enlarge(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? timeBetween = null, double? scale = null, double? scaleInc = null, int? num = null, Position[]? positions = null) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "timeBetween", timeBetween);
        args = this.add_if_not_null(args, "scale", scale);
        args = this.add_if_not_null(args, "scaleInc", scaleInc);
        args = this.add_if_not_null(args, "num", num);

        if (positions != null) {
            this.set_pattern_positions(self, other, positions);
        }

        this.execute_pattern(self, other, "bp_bullet_enlarge", args);
    }

    public void cardinal_r(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warningDelay2 = null, int? displayNumber = null, int? spawnDelay = null, int? eraseDelay = null, Position? position = null, double? rot = null, int? speed = null, int? width = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warningDelay2", warningDelay2);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "displayNumber", displayNumber);
        args = this.add_if_not_null(args, "rot", rot);
        args = this.add_if_not_null(args, "spd", speed);
        args = this.add_if_not_null(args, "width", width);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_cardinal_r", args);
    }

    public void circle_position(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? radius = null, Position[]? positions = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);

        if (positions != null) {
            args = args.Concat([this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length)]).ToArray();
            for (int i = 0; i < positions.Length; i++) {
                Position position = positions[i];
                args = args.Concat([
                    this.utils.CreateString($"posX_{i}")!.Value, new RValue(position.x),
                    this.utils.CreateString($"posY_{i}")!.Value, new RValue(position.y),
                ]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_circle_position", args);
    }

    public void circle_spreads(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? radius = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_circle_spreads", args);
    }

    public void cleave(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, (double rotation, int? targetMask)[]? cleaves = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);

        if (cleaves != null) {
            for (int i = 0; i < cleaves.Length; i++) {
                (double rotation, int? targetMask) cleave = cleaves[i];
                args = args.Concat([
                    this.utils.CreateString($"rot_{i}")!.Value, new RValue(cleave.rotation),
                    this.utils.CreateString($"orderBin_{i}")!.Value, new RValue(cleave.targetMask == null ? 63 : cleave.targetMask.Value)
                ]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_cleave", args);
    }

    public void cleave_enemy(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? angle = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "rot", angle);

        this.execute_pattern(self, other, "bp_cleave_enemy", args);
    }

    public void cleave_fixed(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, PosRot[]? positions = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);

        args = args.Concat([this.utils.CreateString("exTrgId")!.Value, new RValue(0)]).ToArray();
        if (positions != null) {
            args = args.Concat([this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length)]).ToArray();
            for (int i = 0; i < positions.Length; i++) {
                args = args.Concat([
                    this.utils.CreateString($"posX_{i}")!.Value, new RValue(positions[i].position.x),
                    this.utils.CreateString($"posY_{i}")!.Value, new RValue(positions[i].position.y),
                    this.utils.CreateString($"rot_{i}")!.Value, new RValue(positions[i].angle),
                ]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_cleave_fixed", args);
    }

    public void clockspot(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warningDelay2 = null, int? warnMsg = null, int? displayNumber = null, int? spawnDelay = null, int? radius = null, int? fanAngle = null, Position? position = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warningDelay2", warningDelay2);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "displayNumber", displayNumber);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "fanAngle", fanAngle);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_clockspot", args);
    }

    public void colormatch(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? radius = null, int? targetMask = null, int? color = null, Position? position = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "element", color);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "radius", radius);

        if (position != null) {
            args = this.add_if_not_null(args, "hasFixed", true);
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_colormatch", args);
    }

    public void colormatch2(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warningDelay2 = null, int? warnMsg = null, int? spawnDelay = null, int? radius = null, int? targetMask = null, int? color = null, int? ringNum = null, int? displayNumber = null, Position? position = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warningDelay2", warningDelay2);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);

        args = this.add_if_not_null(args, "element", color);

        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "ringNum", ringNum);

        args = this.add_if_not_null(args, "trgBinary", targetMask);

        args = this.add_if_not_null(args, "displayNumber", displayNumber);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_colormatch2", args);
    }

    public void cone_direction(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? fanAngle = null, Position? position = null, double[]? rots = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "fanAngle", fanAngle);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        if (rots != null) {
            args = args.Concat([this.utils.CreateString("numCones")!.Value, new RValue(rots.Length)]).ToArray();
            for (int i = 0; i < rots.Length; i++) {
                double rot = rots[i];
                args = args.Concat([
                    this.utils.CreateString($"rot_{i}")!.Value, new RValue(rot),
                ]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_cone_direction", args);
    }

    public void cone_spreads(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? fanAngle = null, Position? position = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "fanAngle", fanAngle);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_cone_spreads", args);
    }

    public void dark2_cr_circle(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? speed = null, int? angle = null, Position? position = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "spd", speed);
        args = this.add_if_not_null(args, "angle", angle);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_dark2_cr_circle", args);
    }

    public void dark_targeted(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? scale = null, Position[]? positions = null
    ) {
        RValue[] args = [];

        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "scale", scale);

        if (positions != null) {
            args = args.Concat([this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length)]).ToArray();
            for (int i = 0; i < positions.Length; i++) {
                args = args.Concat([
                    this.utils.CreateString($"posX_{i}")!.Value, new RValue(positions[i].x),
                    this.utils.CreateString($"posY_{i}")!.Value, new RValue(positions[i].y)
                ]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_dark_targeted", args);
    }

    public void dialog(CInstance* self, CInstance* other, int? time, int? dialogIndex0) {
        RValue[] args = [];

        args = this.add_if_not_null(args, "time", time);
        args = this.add_if_not_null(args, "dialogInd0", dialogIndex0);

        this.execute_pattern(self, other, "bp_dialog", args);
    }

    // Display number maxes at 6
    public void displaynumbers(
        CInstance* self, CInstance* other, int? displayNumber = null, int? warningDelay = null, int? spawnDelay = null, Position[]? positions = null
    ) {
        RValue[] args = [];

        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "displayNumber", displayNumber);

        if (positions != null) {
            args = args.Concat([this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length)]).ToArray();
            for (int i = 0; i < positions.Length; i++) {
                args = args.Concat([
                    this.utils.CreateString($"posX_{i}")!.Value, new RValue(positions[i].x),
                    this.utils.CreateString($"posY_{i}")!.Value, new RValue(positions[i].y)
                ]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_displaynumbers", args);
    }

    public void enrage(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? timeBetween = null, bool? resetAnim = null) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "timeBetween", timeBetween);

        if (resetAnim != null) {
            args = args.Concat([this.utils.CreateString("resetAnim")!.Value, new RValue(resetAnim == true ? 1.0 : 0.0)]).ToArray(); // Using a bool here doesnt work, no idea why
        }

        this.execute_pattern(self, other, "bp_enrage", args);
    }

    public void enrage_deco(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);

        this.execute_pattern(self, other, "bp_enrage_deco", args);
    }

    // (x, y) refers to the center of the field. Element is which color (purple, yellow, red, blue)
    public void fieldlimit_rectangle(
        CInstance* self, CInstance* other, Position? position = null, int? width = null, int? height = null, int? color = null, int? targetMask = null
    ) {
        RValue[] args = [];

        args = this.add_if_not_null(args, "height", height);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "element", color);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_fieldlimit_rectangle", args);
    }

    public void fieldlimit_rectangle_temporary(
        CInstance* self, CInstance* other, Position? position = null, int? width = null, int? height = null, int? color = null, int? targetMask = null, int? eraseDelay = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "height", height);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "element", color);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_fieldlimit_rectangle_temporary", args);
    }

    public void fire_aoe(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? scale = null, Position[]? positions = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "scale", scale);

        //this.utils.CreateString("type")!.Value, new RValue(type),
        if (positions != null) {
            args = args.Concat([this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length)]).ToArray();

            for (int i = 0; i < positions.Length; i++) {
                args = args.Concat([
                    this.utils.CreateString($"posX_{i}")!.Value, new RValue(positions[i].x),
                    this.utils.CreateString($"posY_{i}")!.Value, new RValue(positions[i].y)
                ]).ToArray();
            }
        }

        this.rnsReloaded.ExecuteScript("bpatt_var", self, other, args);

        // Set pattern layer (no idea what it does just copying existing scripts)
        args = [new RValue(1)];
        this.rnsReloaded.ExecuteScript("bpatt_layer", self, other, args);

        args = [new RValue(this.rnsReloaded.ScriptFindId("bp_fire_aoe"))];
        this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);

        this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
    }

    public void fire2_line(
        CInstance* self, CInstance* other, int? warningDelay = null, int? showWarning = null, int? spawnDelay = null, Position? position = null, double? angle = null, double? lineAngle = null,
        int? lineLength = null, int? numBullets = null, int? spd = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "showWarning", showWarning);
        args = this.add_if_not_null(args, "angle", angle);
        args = this.add_if_not_null(args, "lineAngle", lineAngle);
        args = this.add_if_not_null(args, "num", numBullets);
        args = this.add_if_not_null(args, "lineLength", lineLength);
        args = this.add_if_not_null(args, "spd", spd);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_fire2_line", args);
    }

    public void gravity_fall(
        CInstance* self, CInstance* other, double? mult = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "mult", mult);

        this.execute_pattern(self, other, "bp_gravity_fall", args);
    }

    public void gravity_fall_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, double? mult = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "mult", mult);

        this.execute_pattern(self, other, "bp_gravity_fall_temporary", args);
    }

    public void gravity_pull(
        CInstance* self, CInstance* other, double? mult = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "mult", mult);

        this.execute_pattern(self, other, "bp_gravity_pull", args);
    }

    public void gravity_pull_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, double? mult = null, int? targetMask = null, Position? position = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "mult", mult);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_gravity_pull_temporary", args);
    }

    public void heavy(
        CInstance* self, CInstance* other, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_heavy", args);
    }

    public void heavy_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? hbsDuration = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "hbsDuration", hbsDuration);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_heavy_temporary", args);
    }

    public void heavyextra(
        CInstance* self, CInstance* other, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_heavyextra", args);
    }

    public void heavyextra_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? hbsDuration = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "hbsDuration", hbsDuration);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_heavyextra_temporary", args);
    }

    public void invulncancel(
        CInstance* self, CInstance* other, int? delay = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "delay", delay);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_invulncancel", args);
    }

    public void knockback_circle(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? kbAmount = null, int? radius = null, int? kbDuration = null, int? targetMask = null,
        Position? position = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "kbAmount", kbAmount);
        args = this.add_if_not_null(args, "lifespan", kbDuration);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_knockback_circle", args);
    }

    public void knockback_line(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? kbAmount = null, Position? position = null, bool? horizontal = null, int? kbDuration = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "kbAmount", kbAmount);
        args = this.add_if_not_null(args, "horizontal", horizontal);
        args = this.add_if_not_null(args, "lifespan", kbDuration);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_knockback_line", args);
    }

    public void light_crosswave(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? rotation = null,
        Position? position = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "angle", rotation); // number

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_light_crosswave", args);
    }

    public void light_line(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, Position? position = null, int? lineAngle = 0, int? angle = null, int? spd = null, int? lineLength = null, int? numBullets = null, int? type = null, bool? showWarning = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "angle", angle);
        args = this.add_if_not_null(args, "spd", spd);
        args = this.add_if_not_null(args, "lineAngle", lineAngle);
        args = this.add_if_not_null(args, "lineLength", lineLength);
        args = this.add_if_not_null(args, "num", numBullets);
        args = this.add_if_not_null(args, "type", type);
        args = this.add_if_not_null(args, "showWarning", showWarning);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_light_line", args);
    }

    public void line_direction(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? width = null, PosRot[]? positions = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "width", width);

        if (positions != null) {
            args = args.Concat([this.utils.CreateString("numLines")!.Value, new RValue(positions.Length)]).ToArray();

            for (int i = 0; i < positions.Length; i++) {
                PosRot position = positions[i];
                args = args.Concat([
                    this.utils.CreateString($"posX_{i}")!.Value, new RValue(position.position.x),
                    this.utils.CreateString($"posY_{i}")!.Value, new RValue(position.position.y),
                    this.utils.CreateString($"rot_{i}")!.Value, new RValue(position.angle),
                ]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_line_direction", args);
    }

    public void line_spreads_h(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? width = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_line_spreads_h", args);
    }

    public void line_spreads_v(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? width = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_line_spreads_v", args);
    }

    public void marching_bullet(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? timeBetween = null, double? scale = null, Position[]? positions = null) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "timeBetween", timeBetween);
        args = this.add_if_not_null(args, "scale", scale);

        if (positions != null) {
            args = this.add_if_not_null(args, "numPoints", positions.Length);
            for (int i = 0; i < positions.Length; i++) {
                args = args.Concat([
                    this.utils.CreateString($"posX_{i}")!.Value, new RValue(positions[i].x),
                    this.utils.CreateString($"posY_{i}")!.Value, new RValue(positions[i].y),
                ]).ToArray();
            }
        }
        this.execute_pattern(self, other, "bp_marching_bullet", args);
    }

    public void movementcheck(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, bool? shouldMove = null, int? radius = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "shouldMove", shouldMove);
        args = this.add_if_not_null(args, "radius", radius);

        this.execute_pattern(self, other, "bp_movementcheck", args);
    }

    public void move_position_synced(
        CInstance* self, CInstance* other, int? spawnDelay = null, bool? resetAnim = null, int? duration = null, Position? position = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "resetAnim", resetAnim);
        args = this.add_if_not_null(args, "duration", duration);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }


        this.execute_pattern(self, other, "bp_move_position_synced", args);
    }

    public void painsplit(
        CInstance* self, CInstance* other, bool isPrimary
    ) {
        if (isPrimary) {
            this.execute_pattern(self, other, "bp_mouse_commander0_painsplit", []);
        } else {
            this.execute_pattern(self, other, "bp_mouse_rosemage0_painsplit", []);
        }
    }

    public void prscircle(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? radius = null, int? numBullets = null, int? speed = null, Position? position = null, int? angle = null
    ) {
        RValue[] args = [];

        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "displayNumber", displayNumber);
        args = this.add_if_not_null(args, "element", bulletType);
        args = this.add_if_not_null(args, "doubled", doubled);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "number", numBullets);
        args = this.add_if_not_null(args, "spd", speed);
        args = this.add_if_not_null(args, "angle", angle);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_prscircle", args);
    }

    public void prscircle_follow(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? radius = null, int? numBullets = null, int? speed = null, int? targetId = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "element", bulletType);
        args = this.add_if_not_null(args, "doubled", doubled);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "number", numBullets);
        args = this.add_if_not_null(args, "spd", speed);
        args = this.add_if_not_null(args, "targetId", targetId);

        this.execute_pattern(self, other, "bp_prscircle_follow", args);
    }

    public void prscircle_follow_bin(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? radius = null, int? numBullets = null, int? speed = null, int? targetMask = null
    ) {
        RValue[] args = [];

        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "element", bulletType);
        args = this.add_if_not_null(args, "doubled", doubled);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "number", numBullets);
        args = this.add_if_not_null(args, "spd", speed);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        this.execute_pattern(self, other, "bp_prscircle_follow_bin", args);
    }

    public void prsline_h(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? width = null, int? offset = null, int? speed = null, double? yPosition = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "displayNumber", displayNumber);
        args = this.add_if_not_null(args, "element", bulletType);
        args = this.add_if_not_null(args, "doubled", doubled);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "offset", offset);
        args = this.add_if_not_null(args, "spd", speed);
        args = this.add_if_not_null(args, "y", yPosition);

        this.execute_pattern(self, other, "bp_prsline_h", args);
    }

    public void prsline_h_follow(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? width = null, int? offset = null, int? speed = null, int? targetId = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "displayNumber", displayNumber);
        args = this.add_if_not_null(args, "element", bulletType);
        args = this.add_if_not_null(args, "doubled", doubled);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "offset", offset);
        args = this.add_if_not_null(args, "spd", speed);
        args = this.add_if_not_null(args, "targetId", targetId);

        this.execute_pattern(self, other, "bp_prsline_h_follow", args);
    }

    public void prsline_v(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? bulletType = null, bool? doubled = null, int? spawnDelay = null, int? width = null, int? offset = null, int? speed = null, double? xPosition = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "displayNumber", displayNumber);
        args = this.add_if_not_null(args, "element", bulletType);
        args = this.add_if_not_null(args, "doubled", doubled);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "offset", offset);
        args = this.add_if_not_null(args, "spd", speed);
        args = this.add_if_not_null(args, "x", xPosition);

        this.execute_pattern(self, other, "bp_prsline_v", args);
    }

    public void ray_multi_h(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, Position[]? positions = null) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "width", width);

        if (positions != null) {
            this.set_pattern_positions(self, other, positions);
        }

        this.execute_pattern(self, other, "bp_ray_multi_h", args);
    }

    public void ray_multi_slice(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? timeBetween = null, int? width = null, PosRot[]? positions = null) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "timeBetween", timeBetween);

        if (positions != null) {
            this.set_pattern_position_rotations(self, other, positions);
        }

        this.execute_pattern(self, other, "bp_ray_multi_slice", args);
    }

    public void ray_multi_v(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, Position[]? positions = null) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "width", width);

        if (positions != null) {
            this.set_pattern_positions(self, other, positions);
        }

        this.execute_pattern(self, other, "bp_ray_multi_v", args);
    }

    public void ray_single(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, Position? position = null, int? angle = null) {
        RValue[] args = [];

        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "angle", angle);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_ray_single", args);
    }

    public void ray_spinfast(CInstance* self, CInstance* other, int? warningDelay = null, int? warningRadius = null, int? displayNumber = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, double? angle = null, Position? position = null, double? rot = null, int? numLasers = null) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "width", width);
        args = this.add_if_not_null(args, "radius", warningRadius);
        args = this.add_if_not_null(args, "displayNumber", displayNumber);
        args = this.add_if_not_null(args, "angle", angle);
        args = this.add_if_not_null(args, "rot", rot);
        args = this.add_if_not_null(args, "num", numLasers);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_ray_spinfast", args);
    }

    public void setgamespeed(CInstance* self, CInstance* other, int? spawnDelay = null, double? timeMult = null) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "timeMult", timeMult);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);

        this.execute_pattern(self, other, "bp_setgamespeed", args);
    }

    public void showgroups(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, (int, int, int, int)? groupMasks = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);

        if (groupMasks != null) {
            args = args.Concat([this.utils.CreateString("orderBin_0")!.Value, new RValue(groupMasks.Value.Item1)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_1")!.Value, new RValue(groupMasks.Value.Item2)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_2")!.Value, new RValue(groupMasks.Value.Item3)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_3")!.Value, new RValue(groupMasks.Value.Item4)]).ToArray();
        }
        

        this.execute_pattern(self, other, "bp_showgroups", args);
    }

    public void showorder(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, int? timeBetween = null, (int, int, int, int)? orderMasks = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        args = this.add_if_not_null(args, "timeBetween", timeBetween);

        if (orderMasks != null) {
            args = args.Concat([this.utils.CreateString("orderBin_0")!.Value, new RValue(orderMasks.Value.Item1)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_1")!.Value, new RValue(orderMasks.Value.Item2)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_2")!.Value, new RValue(orderMasks.Value.Item3)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_3")!.Value, new RValue(orderMasks.Value.Item4)]).ToArray();
        }


        this.execute_pattern(self, other, "bp_showorder", args);
    }

    public void tailwind(
        CInstance* self, CInstance* other, int? eraseDelay = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
        this.execute_pattern(self, other, "bp_tailwind", args);
    }

    public void tailwind_permanent(
        CInstance* self, CInstance* other
    ) {
        RValue[] args = [];
        this.execute_pattern(self, other, "bp_tailwind_permanent", args);
    }

    public void tether(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? radius = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        if (eraseDelay != null) {
            if (eraseDelay > 0) {
                args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
            } else {
                args = args.Concat([this.utils.CreateString("permanent")!.Value, new RValue(eraseDelay.Value < 0)]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_tether", args);
    }

    public void tether_enemy(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? radius = null, Position? position = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (eraseDelay != null) {
            if (eraseDelay > 0) {
                args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
            } else {
                args = args.Concat([this.utils.CreateString("permanent")!.Value, new RValue(eraseDelay.Value < 0)]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_tether_enemy", args);
    }

    public void tether_fixed(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? radius = null, Position? position = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "trgBinary", targetMask);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        if (eraseDelay != null) {
            if (eraseDelay > 0) {
                args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
            } else {
                args = args.Concat([this.utils.CreateString("permanent")!.Value, new RValue(eraseDelay.Value < 0)]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_tether_fixed", args);
    }

    public void thorns(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, double? radius = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);

        this.execute_pattern(self, other, "bp_thorns", args);
    }

    public void thorns_bin(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, double? radius = null, (int, int, int, int)? groupMasks = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);

        if (groupMasks != null) {
            args = args.Concat([this.utils.CreateString("orderBin_0")!.Value, new RValue(groupMasks.Value.Item1)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_1")!.Value, new RValue(groupMasks.Value.Item2)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_2")!.Value, new RValue(groupMasks.Value.Item3)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_3")!.Value, new RValue(groupMasks.Value.Item4)]).ToArray();
        }
        this.execute_pattern(self, other, "bp_thorns_bin", args);
    }

    public void thorns_fixed(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? spawnDelay = null, double? radius = null, Position? position = null, int? targetMask = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "radius", radius);
        args = this.add_if_not_null(args, "trgBinary", targetMask);
        args = this.add_if_not_null(args, "warnMsg", warnMsg);
        args = this.add_if_not_null(args, "displayNumber", displayNumber);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_thorns_fixed", args);
    }

    public void water2_line(
        CInstance* self, CInstance* other, int? warningDelay = null, int? showWarning = null, int? spawnDelay = null, Position? position = null, double? angle = null, double? lineAngle = null,
        int? lineLength = null, int? numBullets = null, int? spd = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "showWarning", showWarning);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "angle", angle);
        args = this.add_if_not_null(args, "lineAngle", lineAngle);
        args = this.add_if_not_null(args, "num", numBullets);
        args = this.add_if_not_null(args, "lineLength", lineLength);
        args = this.add_if_not_null(args, "spd", spd);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }        

        this.execute_pattern(self, other, "bp_water2_line", args);
    }

    public void water_moving_ball(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, Position? position = null, double? speed = null, double? scale = null, double? angle = null
    ) {
        RValue[] args = [];
        args = this.add_if_not_null(args, "warningDelay", warningDelay);
        args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
        args = this.add_if_not_null(args, "spd", speed);
        args = this.add_if_not_null(args, "scale", scale);
        args = this.add_if_not_null(args, "angle", angle);

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_water_moving_ball", args);
    }
}
