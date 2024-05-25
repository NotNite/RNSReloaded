# Editing Textures

There are two types of textures to edit:

- Textures like character spritesheets that exist inside of the game install folder ("unpacked" textures)
- Textures like UI elements that are included in the data.win file ("packed" textures)

## Editing unpacked textures

Follow these two tutorials:

- [Creating a new mod with Reloaded](https://reloaded-project.github.io/Reloaded-II/CreatingMods/)
- [Adding a file swap to your mod](https://github.com/Reloaded-Project/reloaded.universal.redirector/blob/master/README-REDIRECTOR.md)

If you wanted to edit, say, `Animations/spellsword_rabbit_0000.png`, your mod folder would look like:

```text
ModConfig.json
Redirector/
  Animations/
    spellsword_rabbit_0000.png
```

Test your mod in game before going any further.

Now, to upload to [GameBanana](https://gamebanana.com/games/20304), follow these two tutorials:

- [Adding update support](https://reloaded-project.github.io/Reloaded-II/EnablingUpdateSupport/)
- [Publishing the update](https://reloaded-project.github.io/Reloaded-II/CreatingRelease/)

Upload the resulting files to your mod, and you are done.

## Editing packed textures

Use a tool like [UndertaleModTool](https://github.com/UnderminersTeam/UndertaleModTool) to open the data.win file. Ignore the warning about YYC. Find the texture in the "Embedded textures" dropdown and export it, with the name being the ID it has (e.g. `Texture 0` would be saved as `0.png`). Edit this texture in your favorite photo editor.

To use the custom texture, first install the Texture Swapper mod, and open the config folder. Place the modded texture into this folder.

![Right click > User Config > Open Folder](https://fxdiscord.com/i/vfbf5v7c.png)

**Multiple mods applying to the same texture cannot be installed at once with Texture Swapper.** You will have to manually combine them in a photo editor.

Because multiple mods have to be manually combined in a photo editor, 1-click installs for packed textures is currently not supported.
