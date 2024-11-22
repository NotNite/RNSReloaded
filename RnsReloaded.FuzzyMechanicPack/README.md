# Fuzzy's Mechanic Pack

**Limits online play.**

A bunch of custom mechanics to use in fights.

## Buff on Hit

Whenever a player is hit by an attack from the enemy using this, they'll get a buff. 
Attacks from other enemies will not buff the player. Can be used with custom buffs.

Call this by using `bp_apply_hbs_synced` and passing in the following custom arguments:

    type: set this to 1 to enable this variant. 0 will always be default colormatch.
          Higher values may be used for future variants
    hbsIndex: the index of the buff to add (string hbsName for Reloaded callers)
    hbsDuration: the duration of the buff
    hbsStrength: the strength of the buff
    trgBinary: which players will get buffs
    eraseDelay: how long until the pattern ends and players are no longer buffed
    timeBetween: duration that must pass before a player can be re-buffed, to prevent spamming

## Bullet Delete

Create your own custom bullet deletion zones, to make safe spaces during bullet spam patterns.

Call this by using `bp_fieldlimit_rectangle` and passing in the following custom arguments:

    type: set this to 1 to enable this variant. 0 will always be default fieldlimit_rectangle.
          Higher values may be used for future variants
    spawnDelay: how long until it starts erasing bullets
    eraseDelay: how long until it stops erasing bullets. 0 (default) is permanent
    x, y: Coordinates for the center of the field
    width/height/radius: Either specify width and height for a rectangle, or radius for a circle field
    stat: set to 1 if you want to invert the field (delete all bullets outside it instead of inside)

## Colormatch Swap

A colormatch, but with special rings that change the color of your ring when you stand in them.  
I recommend combining it with other mechanics that restrict movement to force people to think about their colors.

Call this by using `bp_colormatch` and passing in the following custom arguments:

    type: set this to 1 to enable this variant. 0 will always be default colormatch.
          Higher values may be used for future variants
    number: number of colors to use (up to 4 supported)
    warningDelay, spawnDelay, radius, warnMsg, displayNumber as normal
          (displayNumber displays on ALL circles, damage and set)
    posX_<i>, posY_<i>: coordinates for circle to set player color to i
    offX_<i>, offY_<i>: coordinates for matching color i (set to -1, -1 to disable)
    orderBin_<i>: Which players start with color i. There should be no bits enabled in more than one of these (disjoint)
    playerId_<i>: color i (used as element_<i>, since there's no indexed element variable)
    amount: radius of the circles that set your color (since there's no radius2 variable)
