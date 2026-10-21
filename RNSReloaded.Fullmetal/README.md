# Fullmetal

**Limits online play.**

*For the truly dedicated.*
Every encounter and area will now be visited. _Gold_ and _EXP_ are rebalanced.

## Important Notes
- This is meant to be an accompaniment to **Steelheart**
- Do not use _toybox_ with this mod. If the game crashes on start, try starting a regular game in vanilla, then relaunching **Fullmetal**
- Overall _gold_ and _EXP_ will be more than a normal run. This is inevitable due to rounding
- _Topaz Charm_, _Gold Potions_, and _Tanzakus_ are more useful
- Health bars in encounters with multiple enemies are known to desync. This is mainly a visual thing and shouldn't affect gameplay too much

# Details
## Hallways
"Areas" refer to any of the destinations that can be chosen from the map. For example: _Scholar's Nest_ or _Atelier Aurum_

Because of hallway limitations, there are still 6 hallways in **Fullmetal**. However, all areas are included.
Where Hallway 2 would usually contain one area, it may contain 2 or more and partial areas. This is also true for all hallways, varying from route to route.

In the final route display of the game, only the first areas of each hallway is shown. That would be Outskirts, Area 1, Area 3, Area 5, and Pale Keep.

## Routes
In config, you are allowed to select routes as you could from the map in the regular game.
All _Kingdom_ routes bring you through all five main _Kingdom_ areas,
and all _Underground_ routes bring you through all three main _Underground_ areas

Only Chaotic and True Random bring you through all eight main areas of the game,
as well as Outskirts, Pale Keep, Geode, Looping Hallway, Pinnacle, and Reflecting Pool.
Paired with Steelheart and Infernal BBQ, this is the ultimate challenge of consistency

In order to change route, restart after a run has begun

Below are details for how hallways are structured in a given route

### Kingdom Routes
**Hallway 1**: Kingdom Outskirts\
**Hallway 2**: Areas 1, 2\
**Hallway 3**: Areas 3, 4\
**Hallway 4**: Area 5\
**Hallway 5**: The Pale Keep\
**Hallway 6**: Moonlit Pinnacle

### Underground Routes
**Hallway 1**: Crack in the Geode\
**Hallway 2**: Area 1\
**Hallway 3**: Area 2\
**Hallway 4**: Area 3\
**Hallway 5**: Looping Hallway\
**Hallway 6**: Reflecting Pool

### Random Routes
**Hallway 1**: Kingdom Outskirts+Crack in the Geode\
**Hallway 2**: Outskirts+Geode (cont.) & Areas 1, 2, 3\
**Hallway 3**: Areas 3 (cont.) & 4, 5\
**Hallway 4**: Area 6, 7, 8\
**Hallway 5**: Area 8 (cont.) & The Pale Keep+Looping Hallway\
**Hallway 6**: Moonlit Pinnacle+Reflecting Pool

## Transitions
When transitioning between the two areas in Hallways 2 and 3, the animation can be startling.
The game will not play a long title show, but only move you as if entering another battle.
Fullmetal will transition the background and music,
but the name of the hallway (top left) will remain the same as the first area in the hallway.

## Shops and Treasurespheres
The amount of _Treasurespheres_ is the same as a regular run.
There are an increased amount of _Shops_, one at the beginning of each area, including _Pale Keep_.
In order to ensure that **Fullmetal**'s DPS checks are not steeply more difficult than a regular game,
the treasurespheres are placed in relatively similar locations in terms of enemy levels.
To see the exact placement of _Treasurespheres_, see the **Enemy Levels** section.

## Rewards
In order to balance the _Gold_ and _EXP_ rewards,
they are scaled down so the final total amount will be closer to that of a regular run.
For fairness, numbers are rounded up.
Overall _Gold_ and _EXP_ will be more than a normal run. This is inevitable due to the rounding.

Because of increased number of encounters, _Topaz Charm_ and _Gold Potions_ will have more opportunities to give gold.
_Tanzakus_, _Butterfly Ocarina_, and _Blue Rose_ are more useful as they also have more opportunities to process.

The exact scaling is based on the ratio of enemies in **Fullmetal** compared to that of a regular run.
This is determined per hallway, so Hallway 3 will have different balancing than Hallways 1 and 2.

### Reward Scaling Based on Route

| Hallway | Base | DLC  | Rand |
|---------|------|------|------|
|       1 | 3/10 | 1/3  | 3/19 |
|       2 | 1/3  | 2/3  | 1/4  |
|       3 | 1/3  | 2/3  | 1/4  |
|       4 | 2/3  | 2/3  | 1/4  |
|       5 | 3/5  | 3/5  | 3/10 |
|       6 | 1    | 1    | 1    |

# Enemy Levels
Enemy levels in certain areas are made to match the same levels in a normal game.
Kingdom **Fullmetal** Areas 1, 3, 5 map to default 1, 2, 3 respectively.
_Kingdom Outskirts_, _Pale Keep_, and _Moonlit Pinnacle_ are made to match their correspondents as well.
Areas 2 and 4 in Kingdom **Fullmetal** act as filler, bridging the level gap that typically exists between areas.

Level scaling between encounters is typical, which increases one level per notch.
The only deviation is in _Outskirts_, where in default _Outskirts_, levels increase twice per enemy.
In Kingdom **Fullmetal**, each notch still increases the enemy level by 1.

The same idea is applied to other routes. As you get closer to the final boss,
the levels increase almost linearly up to 68, the vanilla level.

### Levels and Notches of Each Kingdom Area

| Outsk.1 | Outsk.2 | Area 1 | Area 2 | Area 3 | Area 4 | Area 5 | Keep  | Pinna. |
|---------|---------|--------|--------|--------|--------|--------|-------|--------|
| 2       | 10      | shop   | shop   | shop   | shop   | shop   | shop  | scence |
| 3       | 11      | 14     | 21     | 28     | 36     | 45     | 63    | 68b    |
| chest   | 12      | 15     | 22     | 29     | 37     | 46     | 64    |        |
| 5       | 13      | 16     | 23     | 30     | 38     | 47     | 65    |        |
| 6       |         | 17     | 24     | 31     | 39     | 48     | 66    |        |
| 7       |         | 18m    | 25m    | 32m    | 40m    | 49m    | 67    |        |
| 8       |         | chest  |        | chest  |        | chest  | chest |        |
| chest   |         | 20b    | 26b    | 34b    | 41b    | 51b    |       | end    |

### Levels and Notches of Each Underground Area

| Geode1 | Geode2 | Area 1 | Area 2 | Area 3 | Hallw. | Pool  |
|--------|--------|--------|--------|--------|--------|-------|
| 2      | 8      | shop   | shop   | shop   | shop   | scene |
| 3      | 9      | 14     | 28     | 44     | 62     | 68b   |
| 4      | 10     | 15     | 29     | 45     | 63     |       |
| chest  |        | 16     | 30     | 46     | 64     |       |
| 5      |        | 17     | 31     | 47     | 65     |       |
| 6      |        | 18m    | 32m    | 48m    | 66m    |       |
| 7      |        | chest  | chest  | chest  | chest  |       |
| chest  |        | 19b    | 33b    | 49b    |        | end   |

### Levels and Notches of Each Random Area
| O/G1 | O/G2 | O/G3 | A1  | A2  | A3  | A4  | A5  | A6  | A7  | A8  | K/H1 | K/H2 | P/P |
|------|------|------|-----|-----|-----|-----|-----|-----|-----|-----|------|------|-----|
| 1    | 8    | 14   | sh  | sh  | sh  | sh  | sh  | sh  | sh  | sh  | sh   | sh   | sc  |
| 2    | 9    | 15   | 18  | 23  | 28  | 33  | 38  | 43  | 48  | 53  | 58   | 63   | 68b |
| ch   | 9    | 16   | 19  | 24  | 29  | 34  | 39  | 44  | 49  | 54  | 59   | 64   | sc  |
| 3    | ch   | 17   | 20  | 25  | 30  | 35  | 40  | 45  | 50  | 55  | 60   | 65   | 68b |
| 4    | 10   |      | 21  | 26  | 31  | 36  | 41  | 46  | 51  | 56  | 61   | 66   |     |
| 5    | 11   |      | 22m | 27m | 32m | 37m | 42m | 47m | 52m | 57m | 62   | 67   |     |
| 6    | 12   |      | ch  |     |     | ch  |     |     |     | ch  |      | ch   |     |
| 7    | 13   |      | 22b | 27b | 32b | 37b | 42b | 47b | 52b | 57b |      |      | end |

*note: abbreviations are used. `m` for midboss, `b` for boss*\
*final table has additional abbreviations: `ch` for chest, `sh` for shop, `sc` for scene*\
*column names are: `O/G` for Outskirts/Geode, `A` for Area, `K/H` for Keep/Hallway, `P/P` for Pinnacle/Pool*

# Other Notes
## HalfMetal
There is a workshop mod named _HalfMetal_ by _Sinofuma_.
This is a simplified, yet more accessible, version of **Fullmetal**.
_Sinofuma_ is a friend and is actually listed in **Special Thanks** as _xgit_.\
If the specifics of **Fullmetal** do not matter, feel free to use _HalfMetal_ instead.

## Abbreviations
In sharing clear info, there have been many ways people have abbreviated **Fullmetal**.
This section describe my preferred way of doing so

Historically, **Fullmetal** has been shortened to FM. With the release of EX, FM refers to Kingdom routes only.
Alternatively, a number can be added to denote which route is taken, since it determines how many areas there are

FM5, FM3, FM8 are valid ways of referring to Kingdom, Underground and Random routes.
This is probably the most standard and clean method of naming them

Before EX release, I have been referring to FM8 as FMEX. Since there is little reason to run FM3, I prefer to label my runs as either FM or FMEX

# Special Thanks
Thank you to the following people for helping me with the production of this mod

**Development**
- glassoup
- fuzzything44
- mino_dev

**Testing**
- kkx
- gamefreak9
- ry_i
- TomeKing
- tranced
- xgit
- yuuniquea
