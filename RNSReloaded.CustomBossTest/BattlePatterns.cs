using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.CustomBossTest;

using Position = (double x, double y);
using PosRot = ((double x, double y) position, double angle);

public unsafe class BattlePatterns {
    private IRNSReloaded rnsReloaded;
    private Util utils;
    private ILoggerV1 logger;

    public BattlePatterns(IRNSReloaded rnsReloaded, Util utils, ILoggerV1 logger) {
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

                if (delay != null) {
                    args = args.Concat([this.utils.CreateString("delay")!.Value, new RValue(delay.Value)]).ToArray();
                }
                if (hbsHitDelay != null) {
                    args = args.Concat([this.utils.CreateString("hbsHitDelay")!.Value, new RValue(hbsHitDelay.Value)]).ToArray();
                }
                args = args.Concat([this.utils.CreateString("hbsIndex")!.Value, new RValue(i)]).ToArray();
                if (hbsDuration != null) {
                    args = args.Concat([this.utils.CreateString("hbsDuration")!.Value, new RValue(hbsDuration.Value)]).ToArray();
                }
                if (hbsStrength != null) {
                    args = args.Concat([this.utils.CreateString("hbsStrength")!.Value, new RValue(hbsStrength.Value)]).ToArray();
                }
                if (targetMask != null) {
                    args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
                }

                this.execute_pattern(self, other, "bp_apply_hbs_synced", args);
                break;
            }
        }
    }

    public void bind_h(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? targetMask = null
    ) {
        RValue[] args = [];
        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_bind_h", args);
    }

    public void bind_v(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? targetMask = null
    ) {
        RValue[] args = [];
        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_bind_v", args);
    }

    public void bullet_enlarge(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? timeBetween = null, double? scale = null, double? scaleInc = null, int? num = null, Position[]? positions = null) {
        RValue[] args = [];
        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (timeBetween != null) {
            args = args.Concat([this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween.Value)]).ToArray();
        }
        if (scale != null) {
            args = args.Concat([this.utils.CreateString("scale")!.Value, new RValue(scale.Value)]).ToArray();
        }
        if (scaleInc != null) {
            args = args.Concat([this.utils.CreateString("scaleInc")!.Value, new RValue(scaleInc.Value)]).ToArray();
        }
        //if (type != null) {
        //    args = args.Concat([this.utils.CreateString("type")!.Value, new RValue(type)]).ToArray();
        //}
        if (num != null) {
            args = args.Concat([this.utils.CreateString("num")!.Value, new RValue(num.Value)]).ToArray();
        }
        if (positions != null) {
            this.set_pattern_positions(self, other, positions);
        }

        this.execute_pattern(self, other, "bp_bullet_enlarge", args);
    }

    public void cardinal_r(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warningDelay2 = null, int? displayNumber = null, int? spawnDelay = null, int? eraseDelay = null, Position? position = null, double? rot = null, int? speed = null, int? width = null
    ) {
        RValue[] args = [];
        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warningDelay2 != null) {
            args = args.Concat([this.utils.CreateString("warningDelay2")!.Value, new RValue(warningDelay2.Value)]).ToArray();
        }
        if (displayNumber != null) {
            args = args.Concat([this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (rot != null) {
            args = args.Concat([this.utils.CreateString("rot")!.Value, new RValue(rot.Value)]).ToArray();
        }
        if (speed != null) {
            args = args.Concat([this.utils.CreateString("spd")!.Value, new RValue(speed.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_cardinal_r", args);
    }

    public void circle_position(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? radius = null, Position[]? positions = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
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

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
        

        this.execute_pattern(self, other, "bp_circle_spreads", args);
    }

    public void cleave(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, (double rotation, int? targetMask)[]? cleaves = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }

        if (cleaves != null) {
            for (int i = 0; i < cleaves.Length; i++) {
                (double rotation, int? targetMask) cleave = cleaves[i];
                args = args.Concat([
                    this.utils.CreateString($"orderBin_{i}")!.Value, new RValue(cleave.rotation),
                    this.utils.CreateString($"rot_{i}")!.Value, new RValue(cleave.targetMask == null ? 63 : cleave.targetMask.Value)
                ]).ToArray();
            }
        }

        this.execute_pattern(self, other, "bp_cleave", args);
    }

    public void cleave_enemy(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? angle = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (angle != null) {
            args = args.Concat([this.utils.CreateString($"rot")!.Value, new RValue(angle.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_cleave_enemy", args);
    }

    public void cleave_fixed(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, PosRot[]? positions = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
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

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warningDelay2 != null) {
            args = args.Concat([this.utils.CreateString("warningDelay2")!.Value, new RValue(warningDelay2.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (displayNumber != null) {
            args = args.Concat([this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        if (fanAngle != null) {
            args = args.Concat([this.utils.CreateString("fanAngle")!.Value, new RValue(fanAngle.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
        
        this.execute_pattern(self, other, "bp_cone_spreads", args);
    }

    public void colormatch(
    CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? radius = null, int? targetMask = null, int? color = null
) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (color != null) {
            args = args.Concat([this.utils.CreateString("element")!.Value, new RValue(color.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_colormatch", args);
    }

    public void cone_direction(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? fanAngle = null, Position? position = null, double[]? rots = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (fanAngle != null) {
            args = args.Concat([this.utils.CreateString("fanAngle")!.Value, new RValue(fanAngle.Value)]).ToArray();
        }
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
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

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (fanAngle != null) {
            args = args.Concat([this.utils.CreateString("fanAngle")!.Value, new RValue(fanAngle.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
        

        this.execute_pattern(self, other, "bp_cone_spreads", args);
    }

    public void dark_targeted(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? scale = null, Position[]? positions = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (scale != null) {
            args = args.Concat([this.utils.CreateString("scale")!.Value, new RValue(scale.Value)]).ToArray(); // 1.0 scale = 180 radius
        }
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
        
        this.execute_pattern(self, other, "bp_dark_targeted", args);
    }

    public void enrage(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? timeBetween = null, bool? resetAnim = null) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (timeBetween != null) {
            args = args.Concat([this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween.Value)]).ToArray();
        }
        if (resetAnim != null) {
            args = args.Concat([this.utils.CreateString("resetAnim")!.Value, new RValue(resetAnim == true ? 1.0 : 0.0)]).ToArray(); // Using a bool here doesnt work, no idea why
        }

        this.execute_pattern(self, other, "bp_enrage", args);
    }

    // (x, y) refers to the center of the field. Element is which color (purple, yellow, red, blue)
    public void fieldlimit_rectangle(
        CInstance* self, CInstance* other, Position? position = null, int? width = null, int? height = null, int? element = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (height != null) {
            args = args.Concat([this.utils.CreateString("height")!.Value, new RValue(height.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (element != null) {
            args = args.Concat([this.utils.CreateString("element")!.Value, new RValue(element.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
        
        this.execute_pattern(self, other, "bp_fieldlimit_rectangle", args);
    }

    public void fieldlimit_rectangle_temporary(
        CInstance* self, CInstance* other, Position? position = null, int? width = null, int? height = null, int? element = null, int? targetMask = null, int? eraseDelay = null
    ) {
        RValue[] args = [];

        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (height != null) {
            args = args.Concat([this.utils.CreateString("height")!.Value, new RValue(height.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (element != null) {
            args = args.Concat([this.utils.CreateString("element")!.Value, new RValue(element.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        
        this.execute_pattern(self, other, "bp_fieldlimit_rectangle_temporary", args);
    }

    public void fire_aoe(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? scale = null, Position[]? positions = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
        if (scale != null) {
            args = args.Concat([this.utils.CreateString("scale")!.Value, new RValue(scale.Value)]).ToArray(); // 1.0 scale = 180 radius
        }
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

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (showWarning != null) {
            args = args.Concat([this.utils.CreateString("showWarning")!.Value, new RValue(showWarning.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (angle != null) {
            args = args.Concat([this.utils.CreateString("angle")!.Value, new RValue(angle.Value)]).ToArray();
        }
        if (lineAngle != null) {
            args = args.Concat([this.utils.CreateString("lineAngle")!.Value, new RValue(lineAngle.Value)]).ToArray();
        }
        if (numBullets != null) {
            args = args.Concat([this.utils.CreateString("num")!.Value, new RValue(numBullets.Value)]).ToArray();
        }
        if (lineLength != null) {
            args = args.Concat([this.utils.CreateString("lineLength")!.Value, new RValue(lineLength.Value)]).ToArray();
        }
        if (spd != null) {
            args = args.Concat([this.utils.CreateString("spd")!.Value, new RValue(spd.Value)]).ToArray();
        }
        
        this.execute_pattern(self, other, "bp_fire2_line", args);
    }

    public void gravity_fall(
        CInstance* self, CInstance* other, double? mult = null
    ) {
        RValue[] args = [];

        if (mult != null) {
            args = args.Concat([this.utils.CreateString("mult")!.Value, new RValue(mult.Value)]).ToArray();
        }
        
        this.execute_pattern(self, other, "bp_gravity_fall", args);
    }

    public void gravity_fall_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, double? mult = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (mult != null) {
            args = args.Concat([this.utils.CreateString("mult")!.Value, new RValue(mult.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_gravity_fall_temporary", args);
    }

    public void gravity_pull(
        CInstance* self, CInstance* other, double? mult = null
    ) {
        RValue[] args = [];

        if (mult != null) {
            args = args.Concat([this.utils.CreateString("mult")!.Value, new RValue(mult.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_gravity_pull", args);
    }

    public void gravity_pull_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, double? mult = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (mult != null) {
            args = args.Concat([this.utils.CreateString("mult")!.Value, new RValue(mult.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_gravity_pull_temporary", args);
    }

    public void heavy(
        CInstance* self, CInstance* other, int? targetMask = null
    ) {
        RValue[] args = [];

        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_heavy", args);
    }

    public void heavy_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? hbsDuration = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (hbsDuration != null) {
            args = args.Concat([this.utils.CreateString("hbsDuration")!.Value, new RValue(hbsDuration.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_heavy_temporary", args);
    }

    public void heavyextra(
        CInstance* self, CInstance* other, int? targetMask = null
    ) {
        RValue[] args = [];

        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }   

        this.execute_pattern(self, other, "bp_heavyextra", args);
    }

    public void heavyextra_temporary(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? hbsDuration = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (hbsDuration != null) {
            args = args.Concat([this.utils.CreateString("hbsDuration")!.Value, new RValue(hbsDuration.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_heavyextra_temporary", args);
    }

    public void invulncancel(
        CInstance* self, CInstance* other, int? delay = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (delay != null) {
            args = args.Concat([this.utils.CreateString("delay")!.Value, new RValue(delay.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_invulncancel", args);
    }

    public void knockback_circle(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? kbAmount = null, int? radius = null,
        Position? position = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
            //this.utils.CreateString("lifespan")!.Value, new RValue(lifespan),
        if (kbAmount != null) {
            args = args.Concat([this.utils.CreateString("kbAmount")!.Value, new RValue(kbAmount.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_knockback_circle", args);
    }

    //public void light_targeted(
    //    CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? scale = null, int? type = null, Position[] positions = null
    //) {
    //    RValue[] args = [];

    //    if (warningDelay != null) {
    //        args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
    //    }
    //    if (spawnDelay != null) {
    //        args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
    //    }
    //    if (eraseDelay != null) {
    //        args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
    //    }
    //    if (scale != null) {
    //        args = args.Concat([this.utils.CreateString("scale")!.Value, new RValue(scale.Value)]).ToArray();
    //    }
    //    if (type != null) {
    //        args = args.Concat([this.utils.CreateString("type")!.Value, new RValue(type.Value)]).ToArray();
    //    }
    //    

    //    this.rnsReloaded.ExecuteScript("bpatt_var", self, other, args);

    //    args = [];
    //    for ( int i = 0; i < positions.Length; i++ ) {
    //        args = args.Concat([new RValue(positions[i].x), new RValue(positions[i].y)]).ToArray();
    //    }
    //    this.rnsReloaded.ExecuteScript("bpatt_positions", self, other, args);

    //    args = [new RValue(this.rnsReloaded.ScriptFindId("bp_light_targeted"))];
    //    this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);

    //    this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
    //}

    public void line_direction(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? width = null, PosRot[]? positions = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
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

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_line_spreads_h", args);
    }

    public void line_spreads_v(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, int? width = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }


        this.execute_pattern(self, other, "bp_line_spreads_v", args);
    }

    public void marching_bullet(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? timeBetween = null, double? scale = null, Position[]? positions = null) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (timeBetween != null) {
            args = args.Concat([this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween.Value)]).ToArray();
        }
        if (scale != null) {
            args = args.Concat([this.utils.CreateString("scale")!.Value, new RValue(scale.Value)]).ToArray();
        }
        if (positions != null) {
            this.set_pattern_positions(self, other, positions);
        }
        this.execute_pattern(self, other, "bp_marching_bullet", args);
    }

    public void movementcheck(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? spawnDelay = null, bool? shouldMove = null, int? radius = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (shouldMove != null) {
            args = args.Concat([this.utils.CreateString("shouldMove")!.Value, new RValue(shouldMove.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_movementcheck", args);
    }

    public void move_position_synced(
        CInstance* self, CInstance* other, int? spawnDelay = null, bool? resetAnim = null, int? duration = null, Position? position = null
    ) {
        RValue[] args = [];

        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (resetAnim != null) {
            args = args.Concat([this.utils.CreateString("resetAnim")!.Value, new RValue(resetAnim.Value)]).ToArray();
        }
        if (duration != null) {
            args = args.Concat([this.utils.CreateString("duration")!.Value, new RValue(duration.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }


        this.execute_pattern(self, other, "bp_move_position_synced", args);
    }

    public void prscircle(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? element = null, bool? doubled = null, int? spawnDelay = null, int? radius = null, int? numBullets = null, int? speed = null, Position? position = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (displayNumber != null) {
            args = args.Concat([this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber.Value)]).ToArray();
        }
        if (element != null) {
            args = args.Concat([this.utils.CreateString("element")!.Value, new RValue(element.Value)]).ToArray();
        }
        if (doubled != null) {
            args = args.Concat([this.utils.CreateString("doubled")!.Value, new RValue(doubled.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
            //this.utils.CreateString("angle")!.Value, new RValue(angle),
        if (numBullets != null) {
            args = args.Concat([this.utils.CreateString("number")!.Value, new RValue(numBullets.Value)]).ToArray();
        }
        if (speed != null) {
            args = args.Concat([this.utils.CreateString("spd")!.Value, new RValue(speed.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_prscircle", args);
    }

    public void prscircle_follow(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? element = null, bool? doubled = null, int? spawnDelay = null, int? radius = null, int? numBullets = null, int? speed = null, int? targetId = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (element != null) {
            args = args.Concat([this.utils.CreateString("element")!.Value, new RValue(element.Value)]).ToArray();
        }
        if (doubled != null) {
            args = args.Concat([this.utils.CreateString("doubled")!.Value, new RValue(doubled.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        //this.utils.CreateString("angle")!.Value, new RValue(angle),
        if (numBullets != null) {
            args = args.Concat([this.utils.CreateString("number")!.Value, new RValue(numBullets.Value)]).ToArray();
        }
        if (speed != null) {
            args = args.Concat([this.utils.CreateString("spd")!.Value, new RValue(speed.Value)]).ToArray();
        }
        if (targetId != null) {
            args = args.Concat([this.utils.CreateString("targetId")!.Value, new RValue(targetId.Value)]).ToArray();
        }
        

        this.execute_pattern(self, other, "bp_prscircle_follow", args);
    }

    public void prscircle_follow_bin(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? element = null, bool? doubled = null, int? spawnDelay = null, int? radius = null, int? numBullets = null, int? speed = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (element != null) {
            args = args.Concat([this.utils.CreateString("element")!.Value, new RValue(element.Value)]).ToArray();
        }
        if (doubled != null) {
            args = args.Concat([this.utils.CreateString("doubled")!.Value, new RValue(doubled.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        //this.utils.CreateString("angle")!.Value, new RValue(angle),
        if (numBullets != null) {
            args = args.Concat([this.utils.CreateString("number")!.Value, new RValue(numBullets.Value)]).ToArray();
        }
        if (speed != null) {
            args = args.Concat([this.utils.CreateString("spd")!.Value, new RValue(speed.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }


        this.execute_pattern(self, other, "bp_prscircle_follow_bin", args);
    }

    public void prsline_h(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? element = null, bool? doubled = null, int? spawnDelay = null, int? width = null, int? offset = null, int? speed = null, double? yPosition = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (displayNumber != null) {
            args = args.Concat([this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber.Value)]).ToArray();
        }
        if (element != null) {
            args = args.Concat([this.utils.CreateString("element")!.Value, new RValue(element.Value)]).ToArray();
        }
        if (doubled != null) {
            args = args.Concat([this.utils.CreateString("doubled")!.Value, new RValue(doubled.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (offset != null) {
            args = args.Concat([this.utils.CreateString("offset")!.Value, new RValue(offset.Value)]).ToArray();
        }
        if (speed != null) {
            args = args.Concat([this.utils.CreateString("spd")!.Value, new RValue(speed.Value)]).ToArray();
        }
        if (yPosition != null) {
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(yPosition.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_prsline_h", args);
    }

    public void prsline_v(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? element = null, bool? doubled = null, int? spawnDelay = null, int? width = null, int? offset = null, int? speed = null, double? xPosition = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (displayNumber != null) {
            args = args.Concat([this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber.Value)]).ToArray();
        }
        if (element != null) {
            args = args.Concat([this.utils.CreateString("element")!.Value, new RValue(element.Value)]).ToArray();
        }
        if (doubled != null) {
            args = args.Concat([this.utils.CreateString("doubled")!.Value, new RValue(doubled.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (offset != null) {
            args = args.Concat([this.utils.CreateString("offset")!.Value, new RValue(offset.Value)]).ToArray();
        }
        if (speed != null) {
            args = args.Concat([this.utils.CreateString("spd")!.Value, new RValue(speed.Value)]).ToArray();
        }
        if (xPosition != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(xPosition.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_prsline_v", args);
    }

    public void ray_multi_h(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, Position[]? positions = null) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (positions != null) {
            this.set_pattern_positions(self, other, positions);
        }

        this.execute_pattern(self, other, "bp_ray_multi_h", args);
    }

    public void ray_multi_slice(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? timeBetween = null, int? width = null, PosRot[]? positions = null) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (timeBetween != null) {
            args = args.Concat([this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (positions != null) {
            this.set_pattern_position_rotations(self, other, positions);
        }

        this.execute_pattern(self, other, "bp_ray_multi_slice", args);
    }

    public void ray_multi_v(CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, Position[]? positions = null) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (positions != null) {
            this.set_pattern_positions(self, other, positions);
        }

        this.execute_pattern(self, other, "bp_ray_multi_v", args);
    }

    public void ray_spinfast(CInstance* self, CInstance* other, int? warningDelay = null, int? warningRadius = null, int? displayNumber = null, int? spawnDelay = null, int? eraseDelay = null, int? width = null, double? angle = null, Position? position = null, double? rot = null) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warningRadius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(warningRadius.Value)]).ToArray();
        }
        if (displayNumber != null) {
            args = args.Concat([this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (width != null) {
            args = args.Concat([this.utils.CreateString("width")!.Value, new RValue(width.Value)]).ToArray();
        }
        if (angle != null) {
            args = args.Concat([this.utils.CreateString("angle")!.Value, new RValue(angle.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (rot != null) {
            args = args.Concat([this.utils.CreateString("rot")!.Value, new RValue(rot.Value)]).ToArray();
        }
        
        this.execute_pattern(self, other, "bp_ray_spinfast", args);
    }

    public void showgroups(
        CInstance* self, CInstance* other, int? spawnDelay = null, int? eraseDelay = null, (int, int, int, int)? groupMasks = null
    ) {
        RValue[] args = [];

        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
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

        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (eraseDelay != null) {
            args = args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay.Value)]).ToArray();
        }
        if (timeBetween != null) {
            args = args.Concat([this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween.Value)]).ToArray();
        }
        if (orderMasks != null) {
            args = args.Concat([this.utils.CreateString("orderBin_0")!.Value, new RValue(orderMasks.Value.Item1)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_1")!.Value, new RValue(orderMasks.Value.Item2)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_2")!.Value, new RValue(orderMasks.Value.Item3)]).ToArray();
            args = args.Concat([this.utils.CreateString("orderBin_3")!.Value, new RValue(orderMasks.Value.Item4)]).ToArray();
        }


        this.execute_pattern(self, other, "bp_showorder", args);
    }

    public void tether(
        CInstance* self, CInstance* other, int? warningDelay = null, int? spawnDelay = null, int? eraseDelay = null, double? radius = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
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

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
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

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
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

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_thorns", args);
    }

    public void thorns_fixed(
        CInstance* self, CInstance* other, int? warningDelay = null, int? warnMsg = null, int? displayNumber = null, int? spawnDelay = null, double? radius = null, Position? position = null, int? targetMask = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (warnMsg != null) {
            args = args.Concat([this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg.Value)]).ToArray();
        }
        if (displayNumber != null) {
            args = args.Concat([this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (radius != null) {
            args = args.Concat([this.utils.CreateString("radius")!.Value, new RValue(radius.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (targetMask != null) {
            args = args.Concat([this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask.Value)]).ToArray();
        }

        this.execute_pattern(self, other, "bp_thorns_fixed", args);
    }

    public void water2_line(
        CInstance* self, CInstance* other, int? warningDelay = null, int? showWarning = null, int? spawnDelay = null, Position? position = null, double? angle = null, double? lineAngle = null,
        int? lineLength = null, int? numBullets = null, int? spd = null
    ) {
        RValue[] args = [];

        if (warningDelay != null) {
            args = args.Concat([this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay.Value)]).ToArray();
        }
        if (showWarning != null) {
            args = args.Concat([this.utils.CreateString("showWarning")!.Value, new RValue(showWarning.Value)]).ToArray();
        }
        if (spawnDelay != null) {
            args = args.Concat([this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay.Value)]).ToArray();
        }
        if (position != null) {
            args = args.Concat([this.utils.CreateString("x")!.Value, new RValue(position.Value.x)]).ToArray();
            args = args.Concat([this.utils.CreateString("y")!.Value, new RValue(position.Value.y)]).ToArray();
        }
        if (angle != null) {
            args = args.Concat([this.utils.CreateString("angle")!.Value, new RValue(angle.Value)]).ToArray();
        }
        if (lineAngle != null) {
            args = args.Concat([this.utils.CreateString("lineAngle")!.Value, new RValue(lineAngle.Value)]).ToArray();
        }
        if (numBullets != null) {
            args = args.Concat([this.utils.CreateString("num")!.Value, new RValue(numBullets.Value)]).ToArray();
        }
        if (lineLength != null) {
            args = args.Concat([this.utils.CreateString("lineLength")!.Value, new RValue(lineLength.Value)]).ToArray();
        }
        if (spd != null) {
            args = args.Concat([this.utils.CreateString("spd")!.Value, new RValue(spd.Value)]).ToArray();
        }
        

        this.execute_pattern(self, other, "bp_water2_line", args);
    }
}
