using RNSReloaded.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNSReloaded.DamageTracker {

    public struct LogDamageElement { public int playerId; public int enemyId; public int hbId; public int damageAmount; public double painShare; public long gameTime;  }
    public struct LogDebuffDamageElement { public int playerId; public int enemyId; public int debuffId; public int damageAmount; public double painShare; public long gameTime; }
    public struct LogNewEnemyElement { public string enemyKey; public int enemyId; public long gameTime; }
    public struct LogNewFightElement { public long gameTime; } // TODO: fill this out more with fight id and start time
    public struct LogHallwayMoveElement { public int notchPos; public NotchType type; public long gameTime; } // TODO: add other notch data like enemy and seed?
    public struct LogChooseHallsElement { public long gameTime; }

    public interface ILogProducer {
        public void Subscribe(Action<LogDamageElement> action);
        public void Subscribe(Action<LogDebuffDamageElement> action);
        public void Subscribe(Action<LogNewEnemyElement> action);
        public void Subscribe(Action<LogNewFightElement> action);
        public void Subscribe(Action<LogHallwayMoveElement> action);
        public void Subscribe(Action<LogChooseHallsElement> action);

        public void SubscribeAll(
            Action<LogDamageElement> damage,
            Action<LogDebuffDamageElement> debuff,
            Action<LogNewEnemyElement> newEnemy,
            Action<LogNewFightElement> newFight,
            Action<LogHallwayMoveElement> hallwayMove,
            Action<LogChooseHallsElement> chooseHalls
        ) {
            this.Subscribe(damage);
            this.Subscribe(debuff);
            this.Subscribe(newEnemy);
            this.Subscribe(newFight);
            this.Subscribe(hallwayMove);
            this.Subscribe(chooseHalls);
        }
    }
}
