using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
        private IHook<ScriptDelegate> triggerCallHook;
        private IHook<ScriptDelegate> gameOverHook;
        private IHook<ScriptDelegate> finishedFightHook;

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

            var triggerCallScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_trigger_call") - 100000);
            this.triggerCallHook =
                hooks.CreateHook<ScriptDelegate>(this.TriggerCallDetour, triggerCallScript->Functions->Function);
            this.triggerCallHook.Activate();
            this.triggerCallHook.Enable();

            var gameOverScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_gamecontrol_do_gameover") - 100000);
            this.gameOverHook =
                hooks.CreateHook<ScriptDelegate>(this.GameOverDetour, gameOverScript->Functions->Function);
            this.gameOverHook.Activate();
            this.gameOverHook.Enable();

            var finishedFightScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_battlecontroller_end_round") - 100000);
            this.finishedFightHook =
                hooks.CreateHook<ScriptDelegate>(this.FinishedFightDetour, finishedFightScript->Functions->Function);
            this.finishedFightHook.Activate();
            this.finishedFightHook.Enable();
        }

        private List<Action<LogElementDamage>> consumersDamage = new List<Action<LogElementDamage>>();
        private List<Action<LogElementNewEnemy>> consumersNewEnemy = new List<Action<LogElementNewEnemy>>();
        private List<Action<LogElementDebuffDamage>> consumersDebuffDmg = new List<Action<LogElementDebuffDamage>>();
        private List<Action<LogElementNewFight>> consumersNewFight = new List<Action<LogElementNewFight>>();
        private List<Action<LogElementHallwayMove>> consumersHallwayMove = new List<Action<LogElementHallwayMove>>();
        private List<Action<LogElementChooseHalls>> consumersChooseHalls = new List<Action<LogElementChooseHalls>>();
        private List<Action<LogElementAddBuff>> consumersAddBuff = new List<Action<LogElementAddBuff>>();
        private List<Action<LogElementRemoveBuff>> consumersRemoveBuff = new List<Action<LogElementRemoveBuff>>();
        private List<Action<LogElementEndFight>> consumersEndFight = new List<Action<LogElementEndFight>>();
        private List<Action<LogElementUseMove>> consumersUseMove = new List<Action<LogElementUseMove>>();


        public void Subscribe(Action<LogElementDamage> consumer) {
            this.consumersDamage.Add(consumer);
        }
        public void Subscribe(Action<LogElementNewEnemy> consumer) {
            this.consumersNewEnemy.Add(consumer);
        }
        public void Subscribe(Action<LogElementDebuffDamage> consumer) {
            this.consumersDebuffDmg.Add(consumer);
        }
        public void Subscribe(Action<LogElementNewFight> consumer) {
            this.consumersNewFight.Add(consumer);
        }
        public void Subscribe(Action<LogElementHallwayMove> consumer) {
            this.consumersHallwayMove.Add(consumer);
        }
        public void Subscribe(Action<LogElementChooseHalls> consumer) {
            this.consumersChooseHalls.Add(consumer);
        }
        public void Subscribe(Action<LogElementAddBuff> consumer) {
            this.consumersAddBuff.Add(consumer);
        }
        public void Subscribe(Action<LogElementRemoveBuff> consumer) {
            this.consumersRemoveBuff.Add(consumer);
        }
        public void Subscribe(Action<LogElementEndFight> consumer) {
            this.consumersEndFight.Add(consumer);
        }

        public void Subscribe(Action<LogElementUseMove> consumer) {
            this.consumersUseMove.Add(consumer);
        }

        private long gameTime() {
            return this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "gametime"));
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
                LogElementDamage elem = new LogElementDamage() {
                    playerId = (int) playerId,
                    enemyId = (int) enemyId,
                    hbId = (int) hbId,
                    damage = (int) damage,
                    painShare = painShare,
                    gameTime = gameTime
                };
                this.consumersDamage.ForEach(consumer => consumer.Invoke(elem));
            } else {
                var debuffId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "statusId"));
                LogElementDebuffDamage elem = new LogElementDebuffDamage() {
                    playerId = (int) playerId,
                    enemyId = (int) enemyId,
                    debuffId = (int) debuffId,
                    damage = (int) damage,
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

            LogElementNewFight elem = new LogElementNewFight() {
                gameTime = this.gameTime(),
            };
            this.consumersNewFight.ForEach(consumer => consumer.Invoke(elem));

            returnValue = this.newFightHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        private RValue* AddEnemyDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            var enemyRealId = this.rnsReloaded.utils.RValueToLong(argv[0]);
            // index 0 is key. We use this instead of index 2 (name w/o title) because it's always in english and CN/JP chars don't display in ImGui
            var enemyName = this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "enemyData")->Get((int) enemyRealId)->Get(0)->ToString();
            var enemyListId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));

            LogElementNewEnemy elem = new LogElementNewEnemy() {
                enemyKey = enemyName,
                enemyId = (int) enemyListId,
                gameTime = this.gameTime(),
            };
            this.consumersNewEnemy.ForEach(consumer => consumer.Invoke(elem));

            returnValue = this.addEnemyHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        private RValue* HallwayMoveDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            var currentPos = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "currentPos")) + 1;
            NotchType thisNotchType = (NotchType) this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "notches")->Get((int) currentPos)->Get(0));

            LogElementHallwayMove elem = new LogElementHallwayMove() {
                notchPos = (int) currentPos,
                type = thisNotchType,
                gameTime = this.gameTime(),
            };
            this.consumersHallwayMove.ForEach(consumer => consumer.Invoke(elem));

            returnValue = this.hallwayMoveHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;

        }

        private RValue* ChooseHallsDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            LogElementChooseHalls elem = new LogElementChooseHalls() {
                gameTime = this.gameTime(),
            };
            this.consumersChooseHalls.ForEach(consumer => consumer.Invoke(elem));

            returnValue = this.chooseHallsHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        private RValue* GameOverDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            LogElementEndFight elem = new LogElementEndFight() {
                victory = false,
                gameTime = this.gameTime()
            };
            this.consumersEndFight.ForEach(consumer => consumer.Invoke(elem));

            returnValue = this.gameOverHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        private RValue* FinishedFightDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            LogElementEndFight elem = new LogElementEndFight() {
                victory = true,
                gameTime = this.gameTime()
            };
            this.consumersEndFight.ForEach(consumer => consumer.Invoke(elem));
            returnValue = this.finishedFightHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        private const long HBS_CREATED = 33, HBS_DESTROYED = 36, HOTBAR_USED = 44;
        private RValue* TriggerCallDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            var triggerType = argc > 0 ? this.rnsReloaded.utils.RValueToLong(argv[0]) : 0;
            long teamId() => this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "teamId"));

            // Filter by team id being 0 as we don't care about debuffs enemies apply
            if (triggerType == HBS_CREATED && teamId() == 0) {
                // Called when refreshed
                var statusId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "statusId"));
                var hbName = this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "hbsInfo")->Get((int) statusId)->Get(0)->ToString();

                LogElementAddBuff elem = new LogElementAddBuff() {
                    uniqueId = (int) this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "hbsUniqueId")), // Increments with each new hbs applied
                    buffId = (int) statusId,
                    buffName = hbName,
                    sourceId = (int) this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId")),
                    targetId = (int) this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "aflPlayerId")),
                    targetsEnemy = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "aflTeamId")) == 1, // (0 for player, 1 for enemy)
                    sourceHbId = (int) this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "originHbId")),
                    duration = (int) this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "initLength")),
                    strength = (int) this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "strength")),
                    gameTime = this.gameTime()
                };
                this.consumersAddBuff.ForEach(consumer => consumer.Invoke(elem));
            } else if (triggerType == HBS_DESTROYED && teamId() == 0) {
                LogElementRemoveBuff elem = new LogElementRemoveBuff() {
                    uniqueId = (int) this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "hbsUniqueId")), // Increments with each new hbs applied
                    gameTime = this.gameTime()
                };
                this.consumersRemoveBuff.ForEach(consumer => consumer.Invoke(elem));
            } else if (triggerType == HOTBAR_USED) {
                var gcd = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "gcdSecLeft"));
                // hbId, slotId seems to be 0-indexed and both first ability (primary) and first item are slotId 0
                var slotId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "uniqueId"));
                var playerId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));
                var stock = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "stock"));

                var dataId = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(self, "dataId"));
                var itemName = this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "itemData")->Get((int) dataId)->Get(0)->Get(0)->ToString();

                LogElementUseMove elem = new LogElementUseMove() {
                    gcd = (int) gcd,
                    hbId = (int) slotId,
                    itemName = itemName,
                    playerId = (int) playerId,
                    stock = (int) stock,
                    gameTime = this.gameTime()
                };
                this.consumersUseMove.ForEach(item => item.Invoke(elem));
            }

            returnValue = this.triggerCallHook!.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }
    }
}
