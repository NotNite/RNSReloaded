using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System.Runtime.InteropServices;

namespace RNSReloaded.JadeLakeside {
    internal unsafe class Maxi2Fight : CustomFight {

        private IHook<ScriptDelegate> bulletHook;

        public Maxi2Fight(IRNSReloaded rnsReloaded, IFuzzyMechPack fzbp, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, fzbp, logger, hooks, "bp_frog_tinkerer2") {

            var bulletScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_pattern_update_bullet_pos") - 100000);
            this.bulletHook =
                hooks.CreateHook<ScriptDelegate>((CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) => {
                    RValue* projectiles = this.rnsReloaded.FindValue(self, "projectiles");
                    Console.WriteLine("About to call, " + projectiles->Real + " " + projectiles->Int64);
                    RValue numProjectiles = this.rnsReloaded.ExecuteCodeFunction("ds_list_size", self, other, [*projectiles])!.Value;
                    Console.WriteLine("Num projectiles: " + numProjectiles.ToString() + numProjectiles.Type);
                    
                    if (rnsReloaded.utils.RValueToLong(numProjectiles) > 0) {
                        RValue? projectile = this.rnsReloaded.ExecuteCodeFunction("ds_list_find_value", self, other, [
                            *projectiles,
                            new RValue(0)
                        ]);
                        Console.WriteLine("Projectile 0: " + projectile.ToString());
                    }
                    returnValue = this.bulletHook!.OriginalFunction(self, other, returnValue, argc, argv);
                    return returnValue;

                }, bulletScript->Functions->Function);
            this.bulletHook.Activate();
            this.bulletHook.Enable();

            
        }

        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            if (this.scrbp.time(self, other, 0)) {
                this.bp.move_position_synced(self, other, duration: 1000, position: (1920 / 2, 1080 / 2));

                // 4x zoom makes the field now 480 x 270, centered around (960, 540)
                // Meaning it goes from (720, 405) to (1200, 675) (with 20 px on each side being visible buffer)
                RValue[] args = [
                    new RValue(4),
                    new RValue(1)
                ];

                //this.rnsReloaded.ExecuteScript("scrbp_zoom", self, other, args);
                //this.bp.heavyextra(self, other);
            }

            if (this.scrbp.time_repeating(self, other, 1000, 5000)) {
                this.bp.water2_line(self, other, position: (740, 425), angle: 90, lineLength: 480, numBullets: 5, spd: 1);

            }
            if (this.scrbp.time(self, other, 90000)) {
                //this.bp.enrage(self, other, 0);
            }
            
            return returnValue;
        }
    }
}
