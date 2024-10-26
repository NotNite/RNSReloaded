# Fuzzy's Mechanic Pack

**Limits online play.**

A bunch of custom mechanics to use in fights.

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
