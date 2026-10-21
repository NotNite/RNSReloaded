# Bunny Extinction

**Limits online play.**

*Your faults will not be forgiven*

## Features
Individual settings can be enabled or disabled\
If **Infernal BBQ** is enabled, all of the settings will be enabled\
To customize, **Infernal BBQ** must be disabled

### Infernal BBQ 
- Bullet Deletion Disabled
- Invulnerability Disabled
- 1 Health Max
- Intended Speed
- Hitbox size cannot be changed (Sunflower Crown and Evasion Potion)
- Permadeath. Dying once in a run means you can no longer participate in future encounters
- (deprecated) Shira "Steel Yourself" attacks are invulnable (must be placed above Steelheart)

## About Speed
Through many patches of the game mino_dev has designed fights around certain speeds. All Kingdom fights were originally designed around old `-1`. All Underground fights were designed around old `-1`. The **Intended Speed** setting caps your speed to these values for the respective fights.

`-2` Base speed is equivalent to `6.3` pixels per frame at `60` fps\
`-1` Base speed is equivalent to `7` pixels per frame at `60` fps

Abilities can still lower your speed below this amount. The speed cap is calculated after the default movement, so speed up items can still make you move faster when using abilities that bring you below the cap.

Speed limit applies to all player abilities. _Heavyblade Special_ and _Bruiser Defensive_ cannot bring you above BBQ speed, despite them being force march
Mechanics that move you faster than the limit still function, such as knockbacks

## Steelheart Compatibility
_Deprecated. To be reimplemented_

Steelheart forces you to tank attacks during the final boss.
In Bunny Extinction, because maximum health is `1`, these attacks can be invulned again or reenabled.
Ensure that BunnyExtinction is placed above Steelheart within your mod arrangement and the config setting is enabled.

# Special Thanks
Thank you to the following people for helping me with the production of this mod

**Development**
- glassoup
- fuzzything44
- mino_dev

**Testing**
- _cherri
- gamefreak9
- razormad
- tkxkd
- xgit
