using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using static System.Formats.Asn1.AsnWriter;

namespace RNSReloaded.CustomBossTest;

public unsafe class BattlePatterns {
    private IRNSReloaded rnsReloaded;
    private Util utils;
    private ILoggerV1 logger;

    public BattlePatterns(IRNSReloaded rnsReloaded, Util utils, ILoggerV1 logger) {
        this.rnsReloaded = rnsReloaded;
        this.utils = utils;
        this.logger = logger;
    }

    void execute_pattern(CInstance* self, CInstance* other, string pattern, RValue[] args) {
        this.rnsReloaded.ExecuteScript("bpatt_var", self, other, args);
        args = [new RValue(this.rnsReloaded.ScriptFindId(pattern))];
        this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);
        this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
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

    public void cardinal_r(
        CInstance* self, CInstance* other, int warningDelay, int warningDelay2, int displayNumber, int spawnDelay, int eraseDelay, (double, double) position, double rot, int speed, int width
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warningDelay2")!.Value, new RValue(warningDelay2),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
            this.utils.CreateString("rot")!.Value, new RValue(rot),
            this.utils.CreateString("spd")!.Value, new RValue(speed),
            this.utils.CreateString("width")!.Value, new RValue(width),
        ];

        this.execute_pattern(self, other, "bp_cardinal_r", args);
    }

    public void circle_position(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int radius, (double, double)[] positions
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length),
        ];
        for (int i = 0; i < positions.Length; i++) {
            (double, double) position = positions[i];
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(position.Item1),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(position.Item2),
            ]).ToArray();
        }

        this.execute_pattern(self, other, "bp_circle_position", args);
    }

    public void cleave(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, (double rotation, int targetMask)[] cleaves
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
        ];
        for (int i = 0; i < cleaves.Length; i++) {
            (double, int) cleave = cleaves[i];
            args = args.Concat([
                this.utils.CreateString($"orderBin_{i}")!.Value, new RValue(cleave.Item1),
                this.utils.CreateString($"rot_{i}")!.Value, new RValue(cleave.Item2)
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
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, ((double, double) position, double angle)[] positions
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("exTrgId")!.Value, new RValue(0),
            this.utils.CreateString("numPoints")!.Value, new RValue(positions.Length),
        ];
        for (int i = 0; i < positions.Length; i++) {
            ((double, double), double) position = positions[i];
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(position.Item1.Item1),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(position.Item1.Item2),
                this.utils.CreateString($"rot_{i}")!.Value, new RValue(position.Item2),
            ]).ToArray();
        }

        this.execute_pattern(self, other, "bp_cleave_fixed", args);
    }

    public void clockspot(
        CInstance* self, CInstance* other, int warningDelay, int warningDelay2, int warnMsg, int displayNumber, int spawnDelay, int radius, int fanAngle, (double, double) position, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warningDelay2")!.Value, new RValue(warningDelay2),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("fanAngle")!.Value, new RValue(fanAngle),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_cone_spreads", args);
    }

    public void cone_direction(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int fanAngle, (double, double) position, double[] rots
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("fanAngle")!.Value, new RValue(fanAngle),
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("numCones")!.Value, new RValue(rots.Length),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
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
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, int fanAngle, (double, double) position, int? targetMask
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("fanAngle")!.Value, new RValue(fanAngle),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
            this.utils.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
        ];

        this.execute_pattern(self, other, "bp_cone_spreads", args);
    }

    public void dark_targeted(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double scale, (double, double)[] positions
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
            (double, double) point = positions[i];
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(point.Item1),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(point.Item2)
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
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double scale, (double, double)[] positions
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
            (double, double) point = positions[i];
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(point.Item1),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(point.Item2)
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
        CInstance* self, CInstance* other, int warningDelay, int showWarning, int spawnDelay, (double, double) position, double angle, double lineAngle,
        int lineLength, int numBullets, int spd
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("showWarning")!.Value, new RValue(showWarning),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
            this.utils.CreateString("angle")!.Value, new RValue(angle),
            this.utils.CreateString("lineAngle")!.Value, new RValue(lineAngle),
            this.utils.CreateString("num")!.Value, new RValue(numBullets),
            this.utils.CreateString("lineLength")!.Value, new RValue(lineLength),
            this.utils.CreateString("spd")!.Value, new RValue(spd),
        ];

        this.execute_pattern(self, other, "bp_fire2_line", args);
    }

    public void invulncancel(
        CInstance* self, CInstance* other, int delay, int trgBinary
    ) {
        RValue[] args = [
            this.utils.CreateString("delay")!.Value, new RValue(delay),
            this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
        ];

        this.execute_pattern(self, other, "bp_invulncancel", args);
    }

    public void knockback_circle(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int spawnDelay, int kbAmount, int radius,
        (double, double) position
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            //this.utils.CreateString("lifespan")!.Value, new RValue(lifespan),
            this.utils.CreateString("kbAmount")!.Value, new RValue(kbAmount),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
        ];

        this.execute_pattern(self, other, "bp_knockback_circle", args);
    }

    //public void light_targeted(
    //    CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double scale, int type, (double, double)[] positions
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
    //        args.Concat([new RValue(positions[i].Item1), new RValue(positions[i].Item2)]);
    //    }
    //    this.rnsReloaded.ExecuteScript("bpatt_positions", self, other, args);

    //    args = [new RValue(this.rnsReloaded.ScriptFindId("bp_light_targeted"))];
    //    this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);

    //    this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
    //}

    public void line_direction(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int width, ((double, double) position, double angle)[] positions
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("width")!.Value, new RValue(width),
            //this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("numLines")!.Value, new RValue(positions.Length),
        ];
        for (int i = 0; i < positions.Length; i++) {
            ((double, double), double) position = positions[i];
            args = args.Concat([
                this.utils.CreateString($"posX_{i}")!.Value, new RValue(position.Item1.Item1),
                this.utils.CreateString($"posY_{i}")!.Value, new RValue(position.Item1.Item2),
                this.utils.CreateString($"rot_{i}")!.Value, new RValue(position.Item2),
            ]).ToArray();
        }

        this.execute_pattern(self, other, "bp_line_direction", args);
    }

    public void marching_bullet(CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int timeBetween, double scale, (double, double)[] positions) {
            RValue[] args = [
                this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
                this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
                this.utils.CreateString("timeBetween")!.Value, new RValue(timeBetween),
                this.utils.CreateString("scale")!.Value, new RValue(scale),
            ];
            this.rnsReloaded.ExecuteScript("bpatt_var", self, other, args);

            args = [];
            for (int i = 0; i < positions.Length; i++) {
                (double, double) point = positions[i];
                args = args.Concat([
                    new RValue(point.Item1),
                    new RValue(point.Item2)
                ]).ToArray();
            }
            this.rnsReloaded.ExecuteScript("bpatt_positions", self, other, args);

            args = [new RValue(this.rnsReloaded.ScriptFindId("bp_marching_bullet"))];
            this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);

            this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
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
        CInstance* self, CInstance* other, int spawnDelay, bool resetAnim, int duration, (double, double) position
    ) {
        RValue[] args = [
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("resetAnim")!.Value, new RValue(resetAnim),
            this.utils.CreateString("duration")!.Value, new RValue(duration),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
        ];

        this.execute_pattern(self, other, "bp_move_position_synced", args);
    }

    public void prscircle(
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int displayNumber, int element, bool doubled, int spawnDelay, int radius, int numBullets, int speed, (double, double) position
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
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
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
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double radius, (double, double) position, int trgBinary
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
        ];
        if (eraseDelay > 0) {
            args.Concat([this.utils.CreateString("eraseDelay")!.Value, new RValue(eraseDelay)]);
        } else {
            args.Concat([this.utils.CreateString("permanent")!.Value, new RValue(eraseDelay < 0)]);
        }

        this.execute_pattern(self, other, "bp_tether_enemy", args);
    }

    public void tether_fixed(
        CInstance* self, CInstance* other, int warningDelay, int spawnDelay, int eraseDelay, double radius, (double, double) position, int trgBinary
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
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
        CInstance* self, CInstance* other, int warningDelay, int warnMsg, int displayNumber, int spawnDelay, double radius, (double, double) position, int trgBinary
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("warnMsg")!.Value, new RValue(warnMsg),
            this.utils.CreateString("displayNumber")!.Value, new RValue(displayNumber),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("radius")!.Value, new RValue(radius),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
            this.utils.CreateString("trgBinary")!.Value, new RValue(trgBinary),
        ];

        this.execute_pattern(self, other, "bp_thorns_fixed", args);
    }

    public void water2_line(
        CInstance* self, CInstance* other, int warningDelay, int showWarning, int spawnDelay, (double, double) position, double angle, double lineAngle,
        int lineLength, int numBullets, int spd
    ) {
        RValue[] args = [
            this.utils.CreateString("warningDelay")!.Value, new RValue(warningDelay),
            this.utils.CreateString("showWarning")!.Value, new RValue(showWarning),
            this.utils.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
            this.utils.CreateString("x")!.Value, new RValue(position.Item1),
            this.utils.CreateString("y")!.Value, new RValue(position.Item2),
            this.utils.CreateString("angle")!.Value, new RValue(angle),
            this.utils.CreateString("lineAngle")!.Value, new RValue(lineAngle),
            this.utils.CreateString("num")!.Value, new RValue(numBullets),
            this.utils.CreateString("lineLength")!.Value, new RValue(lineLength),
            this.utils.CreateString("spd")!.Value, new RValue(spd),
        ];

        this.execute_pattern(self, other, "bp_water2_line", args);
    }
}
