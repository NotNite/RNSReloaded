# Battle Script Notes

> The following is based on current understandings of how the game scripts function, and information may be missing or incomplete.

## Pattern Script Structure
Battle pattern scripts (both entire fight timelines and individual mechanics) are stored as `bp_**`.
[[note 1]](#notes) [[note 2]](#notes)

Parameters are not passed directly to pattern scripts; instead they are passed to various pattern setup scripts (`bpatt_**`) and stored for later. _[how ?]_

Most parameters are passed to `bpatt_var`, a variable argument function that takes alternating keys and values, e.g. `bpatt_var("x", 500, "y", 300)`.

Once all the neccessary parameters have been loaded, patterns are executed by passing a reference to the script to `bpatt_add`, e.g. `bpatt_add(bp_clockspot)`. [[note 1]](#notes)

Patterns appear to have default values for if a required parameter is not set.

## Pattern Script Parameters
Script parameters all appear to be listed in the global `netPattVar` (GML DS Map). Parameters are listed below with descriptions if known.
- `x`, `y`: Coordinates of a single object or multiple objects with a common source.
- `rot`: Orientation of a single object.
- `spd`: Speed of moving objects.
- `numLines`, `numCones`, `numPoints`: Amount of objects to be spawned, for patterns which can spawn multiple objects at once.
- `posX_[0-19]`, `posY_[0-19]`, `rot_[0-19]`, `spd_[0-19]`: Equivalent to their singular counterparts (`x`, `y`, `rot`, `spd`), used when spawning multiple objects at once.
- `number`: Amount of bullets[only use?] to be spawned.
- `angle`: Appears to be similar to `rot`.
- `showWarning`: Whether to show a warning telegraph.
- `warningDelay`: Delay before warning telegraphs are shown.
- `warningDelay2`: Delay before 2nd warning telegraphs are shown for patterns with a multi-stage telegraph.
- `warnMsg`
- `displayNumber`: Displays a number over the object, for telegraphing patterns executed in sequence
- `spawnDelay`: Delay before the pattern activates (essentially dictates the length of pattern telegraphs, assuming `warningDelay` is unused or set to 0).
- `spawnDelay2`
- `spawnDelayTotal`
- `eraseDelay`: Delay before the object(s) are removed, for patterns that remain activated for an extended period.
- `permanent`: Can be used instead of `eraseDelay`, makes the object remain activated indefinitely.
- `timeBetween`: Time between activations for patterns which perform multiple actions in sequence
- `radius`: Radius of circular objects and patterns that measure the distance from an object.
- `fanAngle`: Angular width of conal patterns.
- `scale`: Scale factor for size of object(s).
- `scaleInc`
- `scaleEnd`
- `kbAmount`: Amount of knockback applied to targets.
- `hbsColorInd`
- `hbsIndex`: Index of status effect to apply.
- `hbsDuration`: Duration of apllied status effect.
- `hbsStrength`
- `hbsHitDelay`
- `targetId`: Player to be targeted.
- `trgBinary`: Players to be targeted as a bitmask, i.e. 0x0A targets players 2 and 4. [[note 3]](#notes)
- `trgBinary2`
- `orderBin_[0-3]`: Equivalent to their singular counterpart `trgBinary`, used when spawning multiple objects at once.
- `shouldMove`: Whether the target(s) stand still or keep moving.

- `fx`
- `fy`
- `delay`
- `delay2`
- `stat`
- `rand`
- `duration`
- `dir`
- `fdir`
- `frot`
- `mult`
- `oAngle`
- `speedMult`
- `moveSpeed`
- `speedDuration`
- `spdDur`
- `amount`
- `minimum`
- `maximum`
- `minAmount`
- `maxAmount`
- `doubled`
- `extraHit`
- `playSound`
- `time`
- `timeExtra`
- `varIndex`
- `varNum`
- `faded`
- `hasFixed`
- `resetAnim`
- `laserSpawnDelay`
- `laserEraseDelay`
- `type`
- `element`
- `startAngle`
- `lineAngle`
- `lineDir`
- `bulletDir`
- `angleInc`
- `lineLength`
- `ringNum`
- `horizontal`
- `lifespan`
- `spawnHealth`
- `num`
- `width`
- `widthInc`
- `length`
- `height`
- `spacing`
- `offset`
- `spdMin`
- `spdMax`
- `exTrgId`
- `dialogInd0`
- `dialogInd1`
- `dialogInd2`
- `offX`
- `offY`
- `playerId_0`
- `t_xpos0`
- `t_ypos0`
- `offX_0`
- `offY_0`
- `playerId_1`
- `t_xpos1`
- `t_ypos1`
- `offX_1`
- `offY_1`
- `playerId_2`
- `t_xpos2`
- `t_ypos2`
- `offX_2`
- `offY_2`
- `playerId_3`
- `t_xpos3`
- `t_ypos3`
- `offX_3`
- `offY_3`
- `playerId_4`
- `t_xpos4`
- `t_ypos4`
- `offX_4`
- `offY_4`
- `playerId_5`
- `t_xpos5`
- `t_ypos5`
- `offX_5`
- `offY_5`

## Notes
1. All code samples are written as GML, and names may be altered when interfacting through Reloaded or looking at decompiled game code. See [internals.md](./internals.md#naming)
2. (Requires further exploration) Do full timelines function same as individual mechs?
3. (Requires further exploration) Default values for this parameter appear to be integer 0x3F, why this value?