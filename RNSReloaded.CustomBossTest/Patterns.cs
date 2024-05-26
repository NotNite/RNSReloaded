using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNSReloaded.CustomBossTest {
    public unsafe class Patterns {

        private WeakReference<IRNSReloaded> rnsReloadedRef;
        private Util? utils;
        private ILoggerV1 logger;

        public Patterns(WeakReference<IRNSReloaded> rnsReloadedRef, Util utils, ILoggerV1 logger) {
            this.rnsReloadedRef = rnsReloadedRef;
            this.utils = utils;
            this.logger = logger;
        }

        public void bp_fire_aoe(CInstance* self, CInstance* other, int spawnDelay, int eraseDelay, double scale, int numPoints, double posX, double posY) {
            if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
                // Set up vars needed to spawn pattern
                RValue[] args = [
                    this.utils!.CreateString("spawnDelay")!.Value, new RValue(spawnDelay),
                    this.utils!.CreateString("eraseDelay")!.Value, new RValue(eraseDelay),
                    this.utils!.CreateString("scale")!.Value, new RValue(scale),
                    this.utils!.CreateString("numPoints")!.Value, new RValue(numPoints),
                    this.utils!.CreateString("posX_0")!.Value, new RValue(posX),
                    this.utils!.CreateString("posY_0")!.Value, new RValue(posY)
                ];
                this.utils!.RunGMLScript("bpatt_var", self, other, args);

                // Set pattern layer (no idea what it does just copying existing script)
                args = [new RValue(1)];
                this.utils!.RunGMLScript("bpatt_layer", self, other, args);

                // Spawn pattern
                args = [new RValue(rnsReloaded.ScriptFindId("bp_fire_aoe"))];
                this.utils!.RunGMLScript("bpatt_add", self, other, args);

                // Reset pattern vars
                this.utils!.RunGMLScript("bpatt_var_reset", self, other, null);
            }
        }
    }
}
