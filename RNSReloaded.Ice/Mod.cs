using System.Xml.Linq;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using System.Diagnostics;

namespace RNSReloaded.Ice;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;

    private WeakReference<IStartupScanner>? scannerRef;

    private ILoggerV1 logger = null!;

    private static Dictionary<string, IHook<ScriptDelegate>> ScriptHooks = new();
    private static IHook<RoutineDelegate>? MvHook;

    private nint mvFunctionPtr = 0;

    private static double FrameMult = 0.95;
    private static int[] DoOnce = [0, 0, 0, 0];
    private static bool[] XChanged = new bool[4];
    private static bool[] ClientLocal = new bool[4];
    private static double[] SpeedX = [0, 0, 0, 0];
    private static double[] SpeedY = [0, 0, 0, 0];
    private static double[] OldSpeedX = [0, 0, 0, 0];
    private static double[] OldSpeedY = [0, 0, 0, 0];
    private static double[] OldMoveX = [0, 0, 0, 0];
    private static double[] OldMoveY = [0, 0, 0, 0];
    private static double[] OldMoveStickMult = [0, 0, 0, 0];
    private static double[] OldmoveSpeedMult = [0, 0, 0, 0];
    private static RValue*[] MoveX = new RValue*[4];
    private static RValue*[] MoveY = new RValue*[4];
    private static RValue*[] MoveStickMult = new RValue*[4];
    private static RValue*[] MoveSpeedMult = new RValue*[4];

    public void StartEx(IModLoaderV1 loader, IModConfigV1 modConfig) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        this.logger = loader.GetLogger();

        this.scannerRef = loader.GetController<IStartupScanner>()!;
        string sig = "40 53 48 83 EC ?? 48 8B D9 48";
        if (this.scannerRef!.TryGetTarget(out var scanner)) {
            scanner.AddMainModuleScan(sig, status => {
                if (status.Found) {
                    this.mvFunctionPtr = Process.GetCurrentProcess().MainModule!.BaseAddress + status.Offset;
                } else {
                    Console.WriteLine("Error, cannot find function D:");
                }
            }
            );
        }

        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.OnReady += this.Ready;
        }

    }

    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
        ) {
            rnsReloaded.LimitOnlinePlay();

            string[] toHook = {
                "bp_tailwind",
                "bp_tailwind_permanent"
            };


            foreach (var hookStr in toHook) {
                RValue* Detour(
                    CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
                ) {
                    return this.NoWinds(hookStr, self, other, returnValue, argc, argv);
                }

                var script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(hookStr) - 100000);
                var hook = hooks.CreateHook<ScriptDelegate>(Detour, script->Functions->Function)!;

                hook.Activate();
                hook.Enable();

                ScriptHooks[hookStr] = hook;

            }


            RValue* Update_control_Detour(
                CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
            ) {
                return this.ControlDetour(self, other, returnValue, argc, argv);
            }
            var playerUpdateControlScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_player_update_control") - 100000);
            var playerUpdateControlHook = hooks.CreateHook<ScriptDelegate>(Update_control_Detour, playerUpdateControlScript->Functions->Function)!;

            playerUpdateControlHook.Activate();
            playerUpdateControlHook.Enable();

            ScriptHooks["scr_player_update_control"] = playerUpdateControlHook;

            MvHook = hooks.CreateHook<RoutineDelegate>(this.MvDetour, this.mvFunctionPtr);
            MvHook.Activate();
            MvHook.Disable();
        }
    }

    public unsafe delegate void FuncDelegate(
        RValue* returnValue, int argc, RValue** argv
    );

    private void MvDetour(RValue* returnValue, CInstance* self, CInstance* other, int argc, RValue** argv
    ) {
        // Unsure what this function(MvFunctionPtr) is for. seems quite generic as it is called often by many scripts
        // But it is called by scr_player_update_control and it writes new values to the player obj such as moveX and moveY
        // So this will catch if there are changes made to those values when this is called and changing them

        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var globalInstant = rnsReloaded.GetGlobalInstance();
            RValue* playersArray = rnsReloaded.FindValue(globalInstant, "player")->Get(0);

            RValue* inputs = rnsReloaded.FindValue(globalInstant, "inputP2I");
            for (int i = 0; i < 4; i++) {
                var id = inputs->Get(i)->Real;
                if (id < 0 || id == 10) {
                    ClientLocal[i] = false;
                    continue;
                } else {
                    ClientLocal[i] = true;
                }
                if (DoOnce[i] < 3) {
                    var playerObj = playersArray->Get(i);
                    if (playerObj->Type != RValueType.Undefined && playerObj != null) {
                        foreach (var key in rnsReloaded.GetStructKeys(playerObj)) {
                            var val = rnsReloaded.FindValue(playerObj->Object, key);
                            if (key == "moveX") {
                                MoveX[i] = val;
                            } else if (key == "moveY") {
                                MoveY[i] = val;
                            } else if (key == "moveStickMult") {
                                MoveStickMult[i] = val;
                            } else if (key == "moveSpeedMult") {
                                MoveSpeedMult[i] = val;
                            }
                        }
                        OldMoveX[i] = MoveX[i]->Real;
                        OldMoveY[i] = MoveY[i]->Real;
                        OldMoveStickMult[i] = MoveStickMult[i]->Real;
                        OldmoveSpeedMult[i] = MoveSpeedMult[i]->Real;
                    }
                }
            }
            

            MvHook!.OriginalFunction(returnValue, self, other, argc, argv);

            for (int i = 0; i < 4; i++) {
                if (!ClientLocal[i]) {
                    continue;
                }
                double newMoveX = MoveX[i]->Real;
                double newMoveY = MoveY[i]->Real;
                double newMoveStickMult = MoveStickMult[i]->Real;
                bool SetXChanged = false;

                if (OldMoveX[i] != newMoveX && newMoveX != 0) {
                    SetXChanged = true;
                }
                if (XChanged[i] || (OldMoveY[i] != newMoveY && newMoveY != 0) || (newMoveY == 0 && newMoveX == 0)) {
                    if (DoOnce[i] < 2) {
                        this.SetSpeeds(i, MoveSpeedMult[i], MoveStickMult[i], newMoveX, newMoveY);
                        DoOnce[i] = 2;
                    }

                    if (XChanged[i] || (OldMoveY[i] != newMoveY && newMoveY != 0)) {
                        this.SetSpeeds(i, MoveSpeedMult[i], MoveStickMult[i], newMoveX, newMoveY);
                    }
                    MoveSpeedMult[i]->Real = double.Max(1, MoveSpeedMult[i]->Real);
                    MoveStickMult[i]->Real = 1;
                    MoveX[i]->Real = SpeedX[i];
                    MoveY[i]->Real = SpeedY[i];


                    XChanged[i] = false;
                }



                if (SetXChanged) {
                    XChanged[i] = true;
                }
            }
        }
    }

    private void SetSpeeds(int i, RValue* moveSpeedMult, RValue* moveStickMult, double newMoveX, double newMoveY) {

        // bake in the config values:
        double configStopSpeed = 0.115;
        double configSlowRefill = 0.3;
        double configBrakes = 0.979;
        double brakes = 1;
        double configMoveFriction = 0.97;
        double configStopFriction = 0.99;
        double friction = configMoveFriction;

        if (newMoveX == 0 && newMoveY == 0) {
            friction = configStopFriction;
        }

        if (moveStickMult->Real < 0.9) {
            brakes = configBrakes;
            if (moveSpeedMult->Real < 0.97) {
                brakes = (moveSpeedMult->Real + ((1 - moveSpeedMult->Real) * configSlowRefill)) * brakes;
            }
        } else {
            if (moveSpeedMult->Real < 0.97) {
                brakes = moveSpeedMult->Real + (1 - moveSpeedMult->Real) * configSlowRefill;
            }
        }

        double frictionNow = Math.Pow(friction, FrameMult);
        double breaksNow = Math.Pow(brakes, FrameMult);

        // stop moving if not pressing btns
        // and total vector < stopspeed
        if (Math.Sqrt(Math.Pow(Math.Abs(SpeedX[i]), 2) + Math.Pow(Math.Abs(SpeedY[i]), 2)) < configStopSpeed && newMoveX == 0 && newMoveY == 0) {
            OldSpeedX[i] = 0.00;
            OldSpeedY[i] = 0.00;
        }

        SpeedX[i] = newMoveX * (1 - frictionNow) * breaksNow + frictionNow * OldSpeedX[i] * breaksNow;
        SpeedY[i] = newMoveY * (1 - frictionNow) * breaksNow + frictionNow * OldSpeedY[i] * breaksNow;

    }
    private RValue* ControlDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        var hook = ScriptHooks["scr_player_update_control"];
        DoOnce = [0, 0, 0, 0];
        Array.Copy(SpeedX, OldSpeedX, 4);
        Array.Copy(SpeedY, OldSpeedY, 4);
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            FrameMult = (rnsReloaded.ExecuteScript("math_inc_per_frame", null, null, [new RValue(1)]) ?? new RValue { }).Real;

            RValue empty = new RValue(0.4);
            MoveX = [&empty, &empty, &empty, &empty];
            MoveY = [&empty, &empty, &empty, &empty];
            MoveStickMult = [&empty, &empty, &empty, &empty];
            MoveSpeedMult = [&empty, &empty, &empty, &empty];
        }

        MvHook!.Enable();
        returnValue = hook.OriginalFunction(self, other, returnValue, argc, argv);

        MvHook.Disable();
        return returnValue;
    }

    private RValue* NoWinds(
        string name, CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        return returnValue;
    }

    public void Resume() { }
    public void Suspend() { }
    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;
    public Action Disposing => () => { };
}
