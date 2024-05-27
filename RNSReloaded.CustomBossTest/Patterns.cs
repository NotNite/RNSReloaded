using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.CustomBossTest;

public unsafe class Patterns {
    private WeakReference<IRNSReloaded> rnsReloadedRef;
    private Util? utils;
    private ILoggerV1 logger;

    public Patterns(WeakReference<IRNSReloaded> rnsReloadedRef, Util utils, ILoggerV1 logger) {
        this.rnsReloadedRef = rnsReloadedRef;
        this.utils = utils;
        this.logger = logger;
    }

    void execute_pattern(CInstance* self, CInstance* other, string pattern, RValue[] args) {
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.ExecuteScript("bpatt_var", self, other, args);
            args = [new RValue(rnsReloaded.ScriptFindId(pattern))];
            rnsReloaded.ExecuteScript("bpatt_add", self, other, args);
            rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
        }
    }

    public void bp_fire_aoe(
        CInstance* self, CInstance* other, int spawnDelay, int eraseDelay, double scale, (double, double)[] points
    ) {
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            // Set up vars needed to spawn pattern
            RValue[] args = [
                //this.utils!.CreateString("warningDelay")!.Value, new RValue(warningDelay),
                this.utils!.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
                this.utils!.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
                //this.utils!.CreateString("trgBinary")!.Value, new RValue(trgBinary),
                this.utils!.CreateString("scale")!.Value, new RValue(scale), // 1.0 scale = 90 radius i think?
                //this.utils!.CreateString("type")!.Value, new RValue(type),
                this.utils!.CreateString("numPoints")!.Value, new RValue(points.Length),
            ];
            for (int i = 0; i < points.Length; i++) {
                (double, double) point = points[i];
                args = args.Concat([
                    this.utils!.CreateString($"posX_{i}")!.Value, new RValue(point.Item1),
                    this.utils!.CreateString($"posY_{i}")!.Value, new RValue(point.Item2)
                ]).ToArray();
            }

            rnsReloaded.ExecuteScript("bpatt_var", self, other, args);

            // Set pattern layer (no idea what it does just copying existing scripts)
            args = [new RValue(1)];
            rnsReloaded.ExecuteScript("bpatt_layer", self, other, args);

            args = [new RValue(rnsReloaded.ScriptFindId("bp_fire_aoe"))];
            rnsReloaded.ExecuteScript("bpatt_add", self, other, args);

            rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
        }
    }

    public void bp_knockback_circle(
        CInstance* self, CInstance* other, int spawnDelay, int kbAmount, int radius, int warnMsg,
        (double, double) position
    ) {
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            // Set up vars needed to spawn pattern
            RValue[] args = [
                //this.utils!.CreateString("warningDelay")!.Value, new RValue(warningDelay),
                this.utils!.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
                //this.utils!.CreateString("trgBinary")!.Value, new RValue(trgBinary),
                this.utils!.CreateString("radius")!.Value, new RValue(radius),
                this.utils!.CreateString("warnMsg")!.Value, new RValue(warnMsg),
                //this.utils!.CreateString("lifespan")!.Value, new RValue(lifespan),
                this.utils!.CreateString("kbAmount")!.Value, new RValue(kbAmount),
                this.utils!.CreateString("x")!.Value, new RValue(position.Item1),
                this.utils!.CreateString("y")!.Value, new RValue(position.Item2),
            ];

            this.execute_pattern(self, other, "bp_knockback_circle", args);
        }
    }

    public void bp_cone_direction(
        CInstance* self, CInstance* other, int spawnDelay, int fanAngle, (double, double) source, double[] rots
    ) {
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            // Set up vars needed to spawn pattern
            RValue[] args = [
                //this.utils!.CreateString("warningDelay")!.Value, new RValue(warningDelay),
                this.utils!.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
                this.utils!.CreateString("fanAngle")!.Value, new RValue(fanAngle),
                //this.utils!.CreateString("trgBinary")!.Value, new RValue(trgBinary),
                this.utils!.CreateString("numCones")!.Value, new RValue(rots.Length),
                this.utils!.CreateString("x")!.Value, new RValue(source.Item1),
                this.utils!.CreateString("y")!.Value, new RValue(source.Item2),
            ];
            for (int i = 0; i < rots.Length; i++) {
                double rot = rots[i];
                args = args.Concat([
                    this.utils!.CreateString($"rot_{i}")!.Value, new RValue(rot),
                ]).ToArray();
            }

            this.execute_pattern(self, other, "bp_cone_direction", args);
        }
    }

    public void bp_cone_spreads(
        CInstance* self, CInstance* other, int spawnDelay, int fanAngle, (double, double) source, int? targetMask
    ) {
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            // Set up vars needed to spawn pattern
            RValue[] args = [
                //this.utils!.CreateString("warningDelay")!.Value, new RValue(warningDelay),
                this.utils!.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
                this.utils!.CreateString("fanAngle")!.Value, new RValue(fanAngle),
                //this.utils!.CreateString("trgBinary")!.Value, new RValue(trgBinary),
                //this.utils!.CreateString("warnMsg")!.Value, new RValue(warnMsg),
                this.utils!.CreateString("x")!.Value, new RValue(source.Item1),
                this.utils!.CreateString("y")!.Value, new RValue(source.Item2),
                this.utils!.CreateString("trgBinary")!.Value, new RValue(targetMask == null ? 63 : targetMask.Value),
            ];

            this.execute_pattern(self, other, "bp_cone_spreads", args);
        }
    }

    public void bp_water2_line(
        CInstance* self, CInstance* other, int spawnDelay, (double, double) position, double angle, double lineAngle,
        int lineLength, int num, int spd
    ) {
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            // Set up vars needed to spawn pattern
            RValue[] args = [
                //this.utils!.CreateString("warningDelay")!.Value, new RValue(warningDelay),
                this.utils!.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
                //this.utils!.CreateString("showWarning")!.Value, new RValue(showWarning),
                this.utils!.CreateString("x")!.Value, new RValue(position.Item1),
                this.utils!.CreateString("y")!.Value, new RValue(position.Item2),
                this.utils!.CreateString("angle")!.Value, new RValue(angle),
                this.utils!.CreateString("lineAngle")!.Value, new RValue(lineAngle),
                this.utils!.CreateString("num")!.Value, new RValue(num),
                this.utils!.CreateString("lineLength")!.Value, new RValue(lineLength),
                this.utils!.CreateString("spd")!.Value, new RValue(spd),
            ];

            this.execute_pattern(self, other, "bp_water2_line", args);
        }
    }

    public void bp_fire2_line(
        CInstance* self, CInstance* other, int spawnDelay, (double, double) position, double angle, double lineAngle,
        int lineLength, int num, int spd
    ) {
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            // Set up vars needed to spawn pattern
            RValue[] args = [
                //this.utils!.CreateString("warningDelay")!.Value, new RValue(warningDelay),
                this.utils!.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
                //this.utils!.CreateString("showWarning")!.Value, new RValue(showWarning),
                this.utils!.CreateString("x")!.Value, new RValue(position.Item1),
                this.utils!.CreateString("y")!.Value, new RValue(position.Item2),
                this.utils!.CreateString("angle")!.Value, new RValue(angle),
                this.utils!.CreateString("lineAngle")!.Value, new RValue(lineAngle),
                this.utils!.CreateString("num")!.Value, new RValue(num),
                this.utils!.CreateString("lineLength")!.Value, new RValue(lineLength),
                this.utils!.CreateString("spd")!.Value, new RValue(spd),
            ];

            this.execute_pattern(self, other, "bp_fire2_line", args);
        }
    }

    // Not sure how this works yet
    //public void bp_marching_bullet(CInstance* self, CInstance* other, int spawnDelay, int timeBetween, int scale, (double, double)[] positions) {
    //    if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
    //        // Set up vars needed to spawn pattern
    //        RValue[] args = [
    //            //this.utils!.CreateString("warningDelay")!.Value, new RValue(warningDelay),
    //            this.utils!.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
    //            this.utils!.CreateString("timeBetween")!.Value, new RValue(timeBetween),
    //            this.utils!.CreateString("scale")!.Value, new RValue(scale),
    //        ];
    //        this.utils!.RunGMLScript("bpatt_var", self, other, args);

    //        args = [];
    //        for (int i = 0; i < positions.Length; i++) {
    //            (double, double) point = positions[i];
    //            args = args.Concat([
    //                new RValue(point.Item1),
    //                new RValue(point.Item2)
    //            ]).ToArray();
    //        }
    //        this.utils!.RunGMLScript("bpatt_positions", self, other, args);

    //        args = [new RValue(rnsReloaded.ScriptFindId("bp_marching_bullet"))];
    //        this.utils!.RunGMLScript("bpatt_add", self, other, args);

    //        this.utils!.RunGMLScript("bpatt_var_reset", self, other, null);
    //    }
    //}

    public void bp_enrage(CInstance* self, CInstance* other, int spawnDelay, int timeBetween) {
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            // Set up vars needed to spawn pattern
            RValue[] args = [
                //this.utils!.CreateString("warningDelay")!.Value, new RValue(warningDelay),
                this.utils!.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
                this.utils!.CreateString("timeBetween")!.Value, new RValue(timeBetween),
                //this.utils!.CreateString("resetAnim")!.Value, new RValue(resetAnim),
            ];

            this.execute_pattern(self, other, "bp_enrage", args);
        }
    }
}
