# Damage tracker

FFXIV ACT but bad

Use the player buttons (labeled with their in-game names) to select which player you want to see damage stats on.

Expand the "Enemy Select" dropdown to select which enemy you want to see damage stats on. The default is either just damage (including painshared damage) if the main enemy has painshare, otherwise it includes total damage to all enemies (for multi enemy fights)

Future TODO list:
- Add a buff/debuff uptime tracker
- Calculate how much buffs/debuffs are contributing to total damage and display rDPS instead, including buffs/debuffs
- Add a "Damage Breakdown" section that shows how much damage each skill is contributing to the total damage, and how much of that damage is coming from buffs/debuffs. Or maybe keep that for uploaded logs or something.
  - (might have issues with snapshotting, since long DOT effects will use the same buff % as the original hit, but will count as separate damage instances. Unsure about if debuffs are snapshot)
