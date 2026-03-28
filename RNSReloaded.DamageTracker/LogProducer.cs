using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNSReloaded.DamageTracker {
    unsafe class LogProducer : ILogProducer {
        private IRNSReloaded rnsReloaded;
        private IReloadedHooks hooks;
        private ILoggerV1 reloadedLogger;

        private IHook<ScriptDelegate> damageHook;
        private IHook<ScriptDelegate> newFightHook;
        private IHook<ScriptDelegate> addEnemyHook;
        private IHook<ScriptDelegate> hallwayMoveHook;
        private IHook<ScriptDelegate> chooseHallsHook;

        public LogProducer(IRNSReloaded rnsReloaded, IReloadedHooks hooks, ILoggerV1 reloadedLogger) {
            this.rnsReloaded = rnsReloaded;
            this.hooks = hooks;
            this.reloadedLogger = reloadedLogger;

            var damageScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_pattern_deal_damage_enemy_subtract") - 100000);
            this.damageHook =
                hooks.CreateHook<ScriptDelegate>(this.EnemyDamageDetour, damageScript->Functions->Function);
            this.damageHook.Activate();
            this.damageHook.Enable();

            var newFightScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrdt_encounter") - 100000);
            this.newFightHook =
                hooks.CreateHook<ScriptDelegate>(this.NewFightDetour, newFightScript->Functions->Function);
            this.newFightHook.Activate();
            this.newFightHook.Enable();

            var addEnemyScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrdt_enemy") - 100000);
            this.addEnemyHook =
                hooks.CreateHook<ScriptDelegate>(this.AddEnemyDetour, addEnemyScript->Functions->Function);
            this.addEnemyHook.Activate();
            this.addEnemyHook.Enable();

            var hallwayMoveScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwayprogress_move_next") - 100000);
            this.hallwayMoveHook =
                hooks.CreateHook<ScriptDelegate>(this.HallwayMoveDetour, hallwayMoveScript->Functions->Function);
            this.hallwayMoveHook.Activate();
            this.hallwayMoveHook.Enable();

            var chooseHallsScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwayprogress_choose_halls") - 100000);
            this.chooseHallsHook =
                hooks.CreateHook<ScriptDelegate>(this.ChooseHallsDetour, chooseHallsScript->Functions->Function);
            this.chooseHallsHook.Activate();
            this.chooseHallsHook.Enable();
        }

        private List<Action<LogDamageElement>> consumersDamage = new List<Action<LogDamageElement>>();
        private List<Action<LogNewEnemyElement>> consumersNewEnemy = new List<Action<LogNewEnemyElement>>();
        private List<Action<LogDebuffDamageElement>> consumersDebuffDmg = new List<Action<LogDebuffDamageElement>>();
        private List<Action<LogNewFightElement>> consumersNewFight = new List<Action<LogNewFightElement>>();
        private List<Action<LogHallwayMoveElement>> consumersHallwayMove = new List<Action<LogHallwayMoveElement>>();
        private List<Action<LogChooseHallsElement>> consumersChooseHalls = new List<Action<LogChooseHallsElement>>();

        public void Subscribe(Action<LogDamageElement> consumer) {
            this.consumersDamage.Add(consumer);
        }
        public void Subscribe(Action<LogNewEnemyElement> consumer) {
            this.consumersNewEnemy.Add(consumer);
        }
        public void Subscribe(Action<LogDebuffDamageElement> consumer) {
            this.consumersDebuffDmg.Add(consumer);
        }
        public void Subscribe(Action<LogNewFightElement> consumer) {
            this.consumersNewFight.Add(consumer);
        }
        public void Subscribe(Action<LogHallwayMoveElement> consumer) {
            this.consumersHallwayMove.Add(consumer);
        }
        public void Subscribe(Action<LogChooseHallsElement> consumer) {
            this.consumersChooseHalls.Add(consumer);
        }

        private RValue* EnemyDamageDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            var hbId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "hbId"));
            var damage = this.rnsReloaded.utils.RValueToLong(argv[2]);
            var playerId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));
            var enemyId = this.rnsReloaded.utils.RValueToLong(argv[1]);
            var painShare = this.rnsReloaded.utils.RValueToDouble(this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "playerPainshareRatio")->Get(1)->Get(0));
            var gameTime = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "gametime"));

            // hbId of -1 means it's a debuff
            if (hbId != -1) {
                LogDamageElement elem = new LogDamageElement() {
                    playerId = (int) playerId,
                    enemyId = (int) enemyId,
                    hbId = (int) hbId,
                    damageAmount = (int) damage,
                    painShare = painShare,
                    gameTime = gameTime
                };
                this.consumersDamage.ForEach(consumer => consumer.Invoke(elem));
            } else {
                var debuffId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "statusId"));
                LogDebuffDamageElement elem = new LogDebuffDamageElement() {
                    playerId = (int) playerId,
                    enemyId = (int) enemyId,
                    debuffId = (int) debuffId,
                    damageAmount = (int) damage,
                    painShare = painShare,
                    gameTime = gameTime
                };
                this.consumersDebuffDmg.ForEach(consumer => consumer.Invoke(elem));
            }
            returnValue = this.damageHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        private RValue* NewFightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            // TODO: fill this out
            var gameTime = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "gametime"));

            LogNewFightElement elem = new LogNewFightElement() {
                gameTime = gameTime,
            };
            this.consumersNewFight.ForEach(consumer => consumer.Invoke(elem));

            returnValue = this.newFightHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        private RValue* AddEnemyDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            var gameTime = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "gametime"));
            var enemyRealId = this.rnsReloaded.utils.RValueToLong(argv[0]);
            // index 0 is key. We use this instead of index 2 (name w/o title) because it's always in english and CN/JP chars don't display in ImGui
            var enemyName = this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "enemyData")->Get((int) enemyRealId)->Get(0)->ToString();
            var enemyListId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));

            LogNewEnemyElement elem = new LogNewEnemyElement() {
                enemyKey = enemyName,
                enemyId = (int) enemyListId,
                gameTime = gameTime,
            };
            this.consumersNewEnemy.ForEach(consumer => consumer.Invoke(elem));

            returnValue = this.addEnemyHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        private RValue* HallwayMoveDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            var gameTime = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "gametime"));

            var currentPos = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "currentPos")) + 1;
            NotchType thisNotchType = (NotchType) this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "notches")->Get((int) currentPos)->Get(0));

            LogHallwayMoveElement elem = new LogHallwayMoveElement() {
                notchPos = (int) currentPos,
                type = thisNotchType,
                gameTime = gameTime,
            };
            this.consumersHallwayMove.ForEach(consumer => consumer.Invoke(elem));

            returnValue = this.hallwayMoveHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;

        }

        private RValue* ChooseHallsDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            var gameTime = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "gametime"));

            LogChooseHallsElement elem = new LogChooseHallsElement() {
                gameTime = gameTime,
            };
            this.consumersChooseHalls.ForEach(consumer => consumer.Invoke(elem));

            returnValue = this.chooseHallsHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }
    }
}
