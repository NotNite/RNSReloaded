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
        CInstance* self, CInstance* other, int delay, int hbsHitDelay, string hbs, int hbsDuration, int hbsStrength, int trgBinary
    ) {
        var hbsInfo = this.utils.GetGlobalVar("hbsInfo");
        for (var i = 0; i < this.rnsReloaded.ArrayGetLength(hbsInfo)!.Value.Real; i++) {
            if (hbsInfo->Get(i)->Get(0)->ToString() == hbs) {
                RValue[] args = [
                    this.utils.CreateString("delay")!.Value, new RValue(delay),
                    this.utils.CreateString("hbsHitDelay")!.Value, new RValue(hbsHitDelay),
                    this.utils.CreateString("hbsIndex")!.Value, new RValue(i),
                    this.utils.CreateString("hbsDuration")!.Value, new RValue(hbsDuration),
                    this.utils.CreateString("hbsStrength")!.Value, new RValue(hbsStrength),
                    this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
                ];

                this.execute_pattern(self, other, "bp_apply_hbs_synced", args);
                break;
            }
        }
    }

    public void bind_h(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_bind_h", args);
    }

    public void bind_v(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_bind_v", args);
    }

    public void bullet_enlarge(CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int timeBetween, double scale, double scaleInc, int num, Position[] positions) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween),
            this.utils.CreateString("scale")!.Value, new RValue(scale),
            this.utils.CreateString("scaleInc")!.Value, new RValue(scaleInc),
            //this.utils.CreateString("type")!.Value, new RValue(type),
            this.utils.CreateString("num")!.Value, new RValue(num)
        ];

        this.set_pattern_positions(self, other, positions);
        this.execute_pattern(self, other, "bp_bullet_enlarge", args);
    }

    public void cardinal_r(
        CInstance* self, CInstance* other, int warningDelay, int warningDelay2, int displayNumber, int spawnDelay, int eraseDelay, Position position, double rot, int speed, int width
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warningDelay2")!.Value, new RValue(warningDelay2),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
            this.utils.CreateString("rot")!.Value, new RValue(rot),
            this.utils.CreateString("spd")!.Value, new RValue(speed),
            this.utils.CreateString("width")!.Value, new RValue(width),
        ];

        this.execute_pattern(self, other, "bp_cardinal_r", args);
    }

    public void circle_position(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int radius, Position[] positions
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length),
        ];
        for (int i = 0; i < positions.Length; i++) {
            Position position = positions[i];
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(position.x),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(position.y),
            ]).ToArray();
        }

        this.execute_pattern(self, other, "bp_circle_position", args);
    }

    public void circle_spreads(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, int radius, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_circle_spreads", args);
    }

    public void cleave(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, (double rotation, int? targetMask)[] cleaves
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
        ];
        for (int i = 0; i < cleaves.Length; i++) {
            (double rotation, int? targetMask) cleave = cleaves[i];
            args = args.Concat([
                this.utils.CreateString($"orderBin_{i}")!.Value, new RValue(cleave.rotation),
                this.utils.CreateString($"rot_{i}")!.Value, new RValue(cleave.targetMask == null ? 63 : cleave.targetMask.Value)
            ]).ToArray();
        }

        this.execute_pattern(self, other, "bp_cleave", args);
    }

    public void cleave_enemy(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, int angle
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString($"rot")!.Value, new RValue(angle)
        ];

        this.execute_pattern(self, other, "bp_cleave_enemy", args);
    }

    public void cleave_fixed(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, PosRot[] positions
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("exTrgId")!.Value, new RValue(0),
            this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length),
        ];
        for (int i = 0; i < positions.Length; i++) {
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(positions[i].position.x),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(positions[i].position.y),
                this.utils.CreateString($"rot_{i}")!.Value, new RValue(positions[i].angle),
            ]).ToArray();
        }

        this.execute_pattern(self, other, "bp_cleave_fixed", args);
    }

    public void clockspot(
        CInstance* self, CInstance* other, int warningDelay, int warningDelay2, int warnMsg, int displayNumber, int spawnDelay, int radius, int fanAngle, Position position, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warningDelay2")!.Value, new RValue(warningDelay2),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("fanAngle")!.Value, new RValue(fanAngle),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_cone_spreads", args);
    }

    public void cone_direction(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int fanAngle, Position position, double[] rots
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("fanAngle")!.Value, new RValue(fanAngle),
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("numCones")!.Value, new RValue(rots.Length),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
        ];
        for (int i = 0; i < rots.Length; i++) {
            double rot = rots[i];
            args = args.Concat([
                this.utils.CreateString($"rot_{i}")!.Value, new RValue(rot),
            ]).ToArray();
        }

        this.execute_pattern(self, other, "bp_cone_direction", args);
    }

    public void cone_spreads(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, int fanAngle, Position position, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("fanAngle")!.Value, new RValue(fanAngle),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_cone_spreads", args);
    }

    public void dark_targeted(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double scale, Position[] positions
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("scale")!.Value, new RValue(scale), // 1.0 scale = 180 radius
            //this.utils.CreateString("type")!.Value, new RValue(type),
            this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length),
        ];
        for (int i = 0; i < positions.Length; i++) {
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(positions[i].x),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(positions[i].y)
            ]).ToArray();
        }

        this.execute_pattern(self, other, "bp_dark_targeted", args);
    }

    public void enrage(CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int timeBetween, bool resetAnim) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween),
            this.utils.CreateString("resetAnim")!.Value, new RValue(resetAnim ? 1.0 : 0.0), // Using a bool here doesnt work, no idea why
        ];

        this.execute_pattern(self, other, "bp_enrage", args);
    }

    public void fire_aoe(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double scale, Position[] positions
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("scale")!.Value, new RValue(scale), // 1.0 scale = 180 radius
            //this.utils.CreateString("type")!.Value, new RValue(type),
            this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length),
        ];
        for (int i = 0; i < positions.Length; i++) {
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(positions[i].x),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(positions[i].y)
            ]).ToArray();
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
        CInstance* self, CInstance* other, int warningDelay, int showWarning, int spawnDelay, Position position, double angle, double lineAngle,
        int lineLength, int numBullets, int spd
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("showWarning")!.Value, new RValue(showWarning),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
            this.utils.CreateString("angle")!.Value, new RValue(angle),
            this.utils.CreateString("lineAngle")!.Value, new RValue(lineAngle),
            this.utils.CreateString("num")!.Value, new RValue(numBullets),
            this.utils.CreateString("lineLength")!.Value, new RValue(lineLength),
            this.utils.CreateString("spd")!.Value, new RValue(spd),
        ];

        this.execute_pattern(self, other, "bp_fire2_line", args);
    }

    public void gravity_fall(
        CInstance* self, CInstance* other, double mult
    ) {
        RValue[] args = [
            this.utils.CreateString("mult")!.Value, new RValue(mult),
        ];

        this.execute_pattern(self, other, "bp_gravity_fall", args);
    }

    public void gravity_fall_temporary(
        CInstance* self, CInstance* other, int spawnDelay, int eraseDelay, double mult, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("mult")!.Value, new RValue(mult),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_gravity_fall_temporary", args);
    }

    public void gravity_pull(
        CInstance* self, CInstance* other, double mult
    ) {
        RValue[] args = [
            this.utils.CreateString("mult")!.Value, new RValue(mult),
        ];

        this.execute_pattern(self, other, "bp_gravity_pull", args);
    }

    public void gravity_pull_temporary(
        CInstance* self, CInstance* other, int spawnDelay, int eraseDelay, double mult, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("mult")!.Value, new RValue(mult),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_gravity_pull_temporary", args);
    }

    public void heavy(
        CInstance* self, CInstance* other, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_heavy", args);
    }

    public void heavy_temporary(
        CInstance* self, CInstance* other, int spawnDelay, int hbsDuration, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("hbsDuration")!.Value, new RValue(hbsDuration),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_heavy_temporary", args);
    }

    public void heavyextra(
        CInstance* self, CInstance* other, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_heavyextra", args);
    }

    public void heavyextra_temporary(
        CInstance* self, CInstance* other, int spawnDelay, int hbsDuration, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("hbsDuration")!.Value, new RValue(hbsDuration),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_heavyextra_temporary", args);
    }

    public void invulncancel(
        CInstance* self, CInstance* other, int delay, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("delay")!.Value, new RValue(delay),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_invulncancel", args);
    }

    public void knockback_circle(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, int kbAmount, int radius,
        Position position
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            //this.utils.CreateString("lifespan")!.Value, new RValue(lifespan),
            this.utils.CreateString("kbAmount")!.Value, new RValue(kbAmount),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
        ];

        this.execute_pattern(self, other, "bp_knockback_circle", args);
    }

    //public void light_targeted(
    //    CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double scale, int type, Position[] positions
    //) {
    //    RValue[] args = [
    //        this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
    //        this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
    //        this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
    //        this.utils.CreateString("scale")!.Value, new RValue(scale),
    //        this.utils.CreateString("type")!.Value, new RValue(type),
    //    ];

    //    this.rnsReloaded.ExecuteScript("bpatt_var", self, other, args);

    //    args = [];
    //    for ( int i = 0; i < positions.Length; i++ ) {
    //        args.Concat([new RValue(positions[i].x), new RValue(positions[i].y)]);
    //    }
    //    this.rnsReloaded.ExecuteScript("bpatt_positions", self, other, args);

    //    args = [new RValue(this.rnsReloaded.ScriptFindId("bp_light_targeted"))];
    //    this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);

    //    this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
    //}

    public void line_direction(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int width, PosRot[] positions
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("width")!.Value, new RValue(width),
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("numLines")!.Value, new RValue(positions.Length),
        ];
        for (int i = 0; i < positions.Length; i++) {
            PosRot position = positions[i];
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(position.position.x),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(position.position.y),
                this.utils.CreateString($"rot_{i}")!.Value, new RValue(position.angle),
            ]).ToArray();
        }

        this.execute_pattern(self, other, "bp_line_direction", args);
    }

    public void line_spreads_h(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, int width, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("width")!.Value, new RValue(width),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_line_spreads_h", args);
    }

    public void line_spreads_v(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, int width, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("width")!.Value, new RValue(width),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_line_spreads_v", args);
    }

    public void marching_bullet(CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int timeBetween, double scale, Position[] positions) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween),
            this.utils.CreateString("scale")!.Value, new RValue(scale),
        ];

        this.set_pattern_positions(self, other, positions);
        this.execute_pattern(self, other, "bp_marching_bullet", args);
    }

    public void movementcheck(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, bool shouldMove, int radius, int targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("shouldMove")!.Value, new RValue(shouldMove),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask),
        ];

        this.execute_pattern(self, other, "bp_movementcheck", args);
    }

    public void move_position_synced(
        CInstance* self, CInstance* other, int spawnDelay, bool resetAnim, int duration, Position position
    ) {
        RValue[] args = [
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("resetAnim")!.Value, new RValue(resetAnim),
            this.utils.CreateString("duration")!.Value, new RValue(duration),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
        ];

        this.execute_pattern(self, other, "bp_move_position_synced", args);
    }

    public void prscircle(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int displayNumber, int element, bool doubled, int spawnDelay, int radius, int numBullets, int speed, Position position
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("element")!.Value, new RValue(element),
            this.utils.CreateString("doubled")!.Value, new RValue(doubled),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            //this.utils.CreateString("angle")!.Value, new RValue(angle),
            this.utils.CreateString("number")!.Value, new RValue(numBullets),
            this.utils.CreateString("spd")!.Value, new RValue(speed),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
        ];

        this.execute_pattern(self, other, "bp_prscircle", args);
    }

    public void prscircle_follow(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int element, bool doubled, int spawnDelay, int radius, int numBullets, int speed, int targetId
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("element")!.Value, new RValue(element),
            this.utils.CreateString("doubled")!.Value, new RValue(doubled),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            //this.utils.CreateString("angle")!.Value, new RValue(angle),
            this.utils.CreateString("number")!.Value, new RValue(numBullets),
            this.utils.CreateString("spd")!.Value, new RValue(speed),
            this.utils.CreateString("targetId")!.Value, new RValue(targetId),
        ];

        this.execute_pattern(self, other, "bp_prscircle_follow", args);
    }

    public void prscircle_follow_bin(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int element, bool doubled, int spawnDelay, int radius, int numBullets, int speed, int targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("element")!.Value, new RValue(element),
            this.utils.CreateString("doubled")!.Value, new RValue(doubled),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            //this.utils.CreateString("angle")!.Value, new RValue(angle),
            this.utils.CreateString("number")!.Value, new RValue(numBullets),
            this.utils.CreateString("spd")!.Value, new RValue(speed),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask),
        ];

        this.execute_pattern(self, other, "bp_prscircle_follow_bin", args);
    }

    public void prsline_h(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int displayNumber, int element, bool doubled, int spawnDelay, int width, int offset, int speed, double yPosition
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("element")!.Value, new RValue(element),
            this.utils.CreateString("doubled")!.Value, new RValue(doubled),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("width")!.Value, new RValue(width),
            this.utils.CreateString("offset")!.Value, new RValue(offset),
            this.utils.CreateString("spd")!.Value, new RValue(speed),
            this.utils.CreateString("y")!.Value, new RValue(yPosition),
        ];

        this.execute_pattern(self, other, "bp_prsline_h", args);
    }

    public void prsline_v(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int displayNumber, int element, bool doubled, int spawnDelay, int width, int offset, int speed, double xPosition
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("element")!.Value, new RValue(element),
            this.utils.CreateString("doubled")!.Value, new RValue(doubled),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("width")!.Value, new RValue(width),
            this.utils.CreateString("offset")!.Value, new RValue(offset),
            this.utils.CreateString("spd")!.Value, new RValue(speed),
            this.utils.CreateString("x")!.Value, new RValue(xPosition),
        ];

        this.execute_pattern(self, other, "bp_prsline_v", args);
    }

    public void ray_multi_h(CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, int width, Position[] positions) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("width")!.Value, new RValue(width),
        ];

        this.set_pattern_positions(self, other, positions);
        this.execute_pattern(self, other, "bp_ray_multi_h", args);
    }

    public void ray_multi_slice(CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, int timeBetween, int width, PosRot[] positions) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween),
            this.utils.CreateString("width")!.Value, new RValue(width),
        ];

        this.set_pattern_position_rotations(self, other, positions);
        this.execute_pattern(self, other, "bp_ray_multi_slice", args);
    }

    public void ray_multi_v(CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, int width, Position[] positions) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("width")!.Value, new RValue(width),
        ];

        this.set_pattern_positions(self, other, positions);
        this.execute_pattern(self, other, "bp_ray_multi_v", args);
    }

    public void ray_spinfast(CInstance* self, CInstance* other, int warningDelay, int warningRadius, int displayNumber, int spawnDelay, int eraseDelay, int width, double angle, Position position, double rot) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("radius")!.Value, new RValue(warningRadius),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("width")!.Value, new RValue(width),
            this.utils.CreateString("angle")!.Value, new RValue(angle),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
            this.utils.CreateString("rot")!.Value, new RValue(rot),
        ];

        this.execute_pattern(self, other, "bp_ray_spinfast", args);
    }

    public void showgroups(
        CInstance* self, CInstance* other, int spawnDelay, int eraseDelay, (int, int, int, int) groupMasks
    ) {
        RValue[] args = [
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("orderBin_0")!.Value, new RValue(groupMasks.Item1),
            this.utils.CreateString("orderBin_1")!.Value, new RValue(groupMasks.Item2),
            this.utils.CreateString("orderBin_2")!.Value, new RValue(groupMasks.Item3),
            this.utils.CreateString("orderBin_3")!.Value, new RValue(groupMasks.Item4),
        ];

        this.execute_pattern(self, other, "bp_showgroups", args);
    }

    public void showorder(
        CInstance* self, CInstance* other, int spawnDelay, int eraseDelay, int timeBetween, (int, int, int, int) orderMasks
    ) {
        RValue[] args = [
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween),
            this.utils.CreateString("orderBin_0")!.Value, new RValue(orderMasks.Item1),
            this.utils.CreateString("orderBin_1")!.Value, new RValue(orderMasks.Item2),
            this.utils.CreateString("orderBin_2")!.Value, new RValue(orderMasks.Item3),
            this.utils.CreateString("orderBin_3")!.Value, new RValue(orderMasks.Item4),
        ];

        this.execute_pattern(self, other, "bp_showorder", args);
    }

    public void tether(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double radius, int trgBinary
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
        ];
        if(eraseDelay > 0) {
            args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay)]);
        } else {
            args.Concat([this.utils.CreateString("permanent")!.Value, new RValue(eraseDelay < 0)]);
        }

        this.execute_pattern(self, other, "bp_tether", args);
    }

    public void tether_enemy(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double radius, Position position, int trgBinary
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
        ];
        if (eraseDelay > 0) {
            args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay)]);
        } else {
            args.Concat([this.utils.CreateString("permanent")!.Value, new RValue(eraseDelay < 0)]);
        }

        this.execute_pattern(self, other, "bp_tether_enemy", args);
    }

    public void tether_fixed(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double radius, Position position, int trgBinary
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
        ];
        if (eraseDelay > 0) {
            args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay)]);
        } else {
            args.Concat([this.utils.CreateString("permanent")!.Value, new RValue(eraseDelay < 0)]);
        }

        this.execute_pattern(self, other, "bp_tether_fixed", args);
    }

    public void thorns(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, double radius, int trgBinary
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
        ];

        this.execute_pattern(self, other, "bp_thorns", args);
    }

    public void thorns_fixed(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int displayNumber, int spawnDelay, double radius, Position position, int trgBinary
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
            this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
        ];

        this.execute_pattern(self, other, "bp_thorns_fixed", args);
    }

    public void water2_line(
        CInstance* self, CInstance* other, int warningDelay, int showWarning, int spawnDelay, Position position, double angle, double lineAngle,
        int lineLength, int numBullets, int spd
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("showWarning")!.Value, new RValue(showWarning),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("x")!.Value, new RValue(position.x),
            this.utils.CreateString("y")!.Value, new RValue(position.y),
            this.utils.CreateString("angle")!.Value, new RValue(angle),
            this.utils.CreateString("lineAngle")!.Value, new RValue(lineAngle),
            this.utils.CreateString("num")!.Value, new RValue(numBullets),
            this.utils.CreateString("lineLength")!.Value, new RValue(lineLength),
            this.utils.CreateString("spd")!.Value, new RValue(spd),
        ];

        this.execute_pattern(self, other, "bp_water2_line", args);
    }
}
