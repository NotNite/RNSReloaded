using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.JadeLakeside {
    internal unsafe class Maxi1Fight : CustomFight {

        private IHook<ScriptDelegate> langStringHook;

        struct StringReplaceInfo {
            public readonly int stringId;
            public readonly string key;
            public readonly string replace;

            public StringReplaceInfo(int stringId, string key, string replace) {
                this.stringId = stringId;
                this.key = key;
                this.replace = replace;
            }
        }
        static readonly StringReplaceInfo[] DoMoveStrings = [
            new StringReplaceInfo(1, "d_cat_shopkeeper_0_0", "Maxi Says: Move!"),
            new StringReplaceInfo(2, "d_cat_shopkeeper_0_1", "Don't Move!"),
            new StringReplaceInfo(3, "d_cat_shopkeeper_0_2", "Maxi Says: Don't Not Move!"),
            new StringReplaceInfo(4, "d_cat_shopkeeper_0_3", "Don't Not Stand Still!"),
            new StringReplaceInfo(5, "d_cat_shopkeeper_0_4", "Maxi Says: Don't Not Keep Moving!")
        ];

        static readonly StringReplaceInfo[] DontMoveStrings = [
            new StringReplaceInfo(6, "d_cat_shopkeeper_0_5", "Maxi Says: Don't Move!"),
            new StringReplaceInfo(7, "d_cat_shopkeeper_1_0", "Stay Moving!"),
            new StringReplaceInfo(8, "d_cat_shopkeeper_1_1", "Maxi Says: Stay Stopped!"),
            new StringReplaceInfo(9, "d_cat_shopkeeper_1_2", "Don't Not Move!"),
            new StringReplaceInfo(10, "d_cat_shopkeeper_1_3", "Maxi Says: Don't Not Stand Still!")
        ];


        public Maxi1Fight(IRNSReloaded rnsReloaded, IFuzzyMechPack fzbp, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, fzbp, logger, hooks, "bp_frog_tinkerer1") {

            var langStringScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_lang_string") - 100000);
            this.langStringHook =
                hooks.CreateHook<ScriptDelegate>((CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) => {
                    string key = argv[0]->ToString();
                    // Replace base strings with our custom stop/keep moving ones
                    // Theoretically this fight could be fully vanilla but ehhhhh I don't want to
                    for (int i = 0; i < DoMoveStrings.Length; i++) {
                        if (key == DoMoveStrings[i].key) {
                            this.rnsReloaded.CreateString(returnValue, DoMoveStrings[i].replace);
                            return returnValue;
                        }
                    }
                    for (int i = 0; i < DontMoveStrings.Length; i++) {
                        if (key == DontMoveStrings[i].key) {
                            this.rnsReloaded.CreateString(returnValue, DontMoveStrings[i].replace);
                            return returnValue;
                        }
                    }
                    if (key == "d_cat_shopkeeper_1_4") {
                        this.rnsReloaded.CreateString(returnValue, "Let's play a game! I call it \"Maxi Says\". No cheating :3");
                        return returnValue;
                    }
                    returnValue = this.langStringHook!.OriginalFunction(self, other, returnValue, argc, argv);

                    return returnValue;

                }, langStringScript->Functions->Function);
            this.langStringHook.Activate();
            this.langStringHook.Enable();

        }

        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            if (this.scrbp.time(self, other, 0)) {
                // Intro dialogue
                this.bp.dialog(self, other, time: 3000, dialogIndex0: 11);
                // No invuln.
                this.bp.apply_hbs_synced(self, other, hbs: "hbs_nodefense", hbsDuration: 60000);
                this.bp.move_position_synced(self, other, duration: 1000, position: (1920 / 2, 1080 / 2));
            }
            if (this.scrbp.time_repeating(self, other, 3000, 5000)) {
                bool shouldMove = this.rng.Next(2) == 0;
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;

                // Slowly increase difficulty as time goes on
                int index = this.rng.Next(Math.Min(2, thisBattleTime / 9000), Math.Min(1 + (thisBattleTime / 4500), DoMoveStrings.Length));
                this.bp.dialog(self, other, time: 4000, dialogIndex0: shouldMove ? DoMoveStrings[index].stringId : DontMoveStrings[index].stringId);
                this.bp.movementcheck(self, other, warningDelay: 4500, spawnDelay: 4500, shouldMove: shouldMove);

                // Basic spreads, also helps time the move check
                this.bp.line_spreads_h(self, other, warnMsg: 1, spawnDelay: 4500, width: 100);
                this.bp.line_spreads_v(self, other, warnMsg: 0, spawnDelay: 4500, width: 100);
                // Some balls to fill the map I guess
                if (this.rng.Next(2) == 0) {
                    this.bp.water_moving_ball(self, other, spawnDelay: 2000, position: (0, 1* 1080 / 4 - 50), speed: 8, scale: 1.2, angle: 0);
                    this.bp.water_moving_ball(self, other, spawnDelay: 2000, position: (1920, 2 * 1080 / 4), speed: 10, scale: 1.2, angle: 180);
                    this.bp.water_moving_ball(self, other, spawnDelay: 2000, position: (0, 3 * 1080 / 4 + 50), speed: 8, scale: 1.2, angle: 0);
                } else {
                    this.bp.water_moving_ball(self, other, spawnDelay: 2000, position: (1920, 1 * 1080 / 4 - 50), speed: 8, scale: 1.2, angle: 180);
                    this.bp.water_moving_ball(self, other, spawnDelay: 2000, position: (0, 2 * 1080 / 4), speed: 10, scale: 1.2, angle: 0);
                    this.bp.water_moving_ball(self, other, spawnDelay: 2000, position: (1920, 3 * 1080 / 4 + 50), speed: 8, scale: 1.2, angle: 180);

                }
            }


            if (this.scrbp.time(self, other, 45000)) {
                this.bp.enrage(self, other, 0);
            }
            
            return returnValue;
        }
    }
}
