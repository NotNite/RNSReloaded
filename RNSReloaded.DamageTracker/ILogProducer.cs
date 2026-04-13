using RNSReloaded.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNSReloaded.DamageTracker {

    public struct LogElementDamage { public int playerId; public int enemyId; public int hbId; public int damage; public double painShare; public long gameTime;  }
    public struct LogElementDebuffDamage { public int playerId; public int enemyId; public int debuffId; public int damage; public double painShare; public long gameTime; }
    public struct LogElementNewEnemy { public string enemyKey; public int enemyId; public long gameTime; }
    public struct LogElementNewFight { public long gameTime; } // TODO: fill this out more with fight id and start time
    public struct LogElementHallwayMove { public int notchPos; public NotchType type; public long gameTime; } // TODO: add other notch data like enemy and seed?
    public struct LogElementChooseHalls { public long gameTime; }
    public struct LogElementAddBuff { public int uniqueId; public int buffId; public string buffName; public int sourceId; public int targetId; public bool targetsEnemy; public int duration; public int strength; public int sourceHbId; public long gameTime; }
    public struct LogElementRemoveBuff { public int uniqueId; public long gameTime; }
    public struct LogElementEndFight { public bool victory; public long gameTime; }
    public struct LogElementUseMove { public int gcd; public int hbId; public string itemName; public int stock; public int playerId; public long gameTime; }

    public interface ILogProducer {
        public void Subscribe(Action<LogElementDamage> action);
        public void Subscribe(Action<LogElementDebuffDamage> action);
        public void Subscribe(Action<LogElementNewEnemy> action);
        public void Subscribe(Action<LogElementNewFight> action);
        public void Subscribe(Action<LogElementHallwayMove> action);
        public void Subscribe(Action<LogElementChooseHalls> action);
        public void Subscribe(Action<LogElementAddBuff> action);
        public void Subscribe(Action<LogElementRemoveBuff> action);
        public void Subscribe(Action<LogElementEndFight> action);
        public void Subscribe(Action<LogElementUseMove> action);

        public void SubscribeAll(
            Action<LogElementDamage> damage,
            Action<LogElementDebuffDamage> debuff,
            Action<LogElementNewEnemy> newEnemy,
            Action<LogElementNewFight> newFight,
            Action<LogElementHallwayMove> hallwayMove,
            Action<LogElementChooseHalls> chooseHalls,
            Action<LogElementAddBuff> addBuff,
            Action<LogElementRemoveBuff> removeBuff,
            Action<LogElementEndFight> endFight,
            Action<LogElementUseMove> useMove
        ) {
            this.Subscribe(damage);
            this.Subscribe(debuff);
            this.Subscribe(newEnemy);
            this.Subscribe(newFight);
            this.Subscribe(hallwayMove);
            this.Subscribe(chooseHalls);
            this.Subscribe(addBuff);
            this.Subscribe(removeBuff);
            this.Subscribe(endFight);
            this.Subscribe(useMove);
        }
    }
}
