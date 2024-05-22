# RNSReloaded

Rabbit and Steel modding through [Reloaded II](https://reloaded-project.github.io/Reloaded-II/). Partially based off of [YYToolkit](https://github.com/AurieFramework/YYToolkit).

## How to use

Download and install [Reloaded II](https://github.com/Reloaded-Project/Reloaded-II/releases/latest). Then, follow the tutorial to add RabbitSteel.exe to Reloaded. Once setup, you can download the mods in the first tab.

**If you were previously using Aurie, you will have to uninstall it, or else Reloaded will not function properly.**

## For developers

You can add the `RNSReloaded.Interfaces` project to your mod (preferably through a submodule), mark RNSReloaded as a dependency, and use it like so:

```cs
private WeakReference<IRNSReloaded>? rnsReloadedRef;

public void Start(IModLoaderV1 loader) {
  this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
  if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
    rnsReloaded.OnReady += this.Ready;
  }
}

private void Ready() {
  // Access GameMaker internals here
}
```

**Do not call any RNSReloaded functions before Ready is called.** It will crash the game spectacularly.

If you can, upstream your mods to this repository! (Or don't, I don't really care, this is open source do what you want as long as it doesn't violate the license.)
