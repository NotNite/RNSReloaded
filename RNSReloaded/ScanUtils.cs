using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace RNSReloaded;

public class ScanUtils {
    public nint BaseAddr;

    private WeakReference<IStartupScanner> scannerRef;
    private ILoggerV1 logger;
    private int scansRemaining;

    public ScanUtils(WeakReference<IStartupScanner> scannerRef, ILoggerV1 logger) {
        this.scannerRef = scannerRef;
        this.logger = logger;
        this.BaseAddr = Process.GetCurrentProcess().MainModule!.BaseAddress;
    }

    public void Scan(string sig, Action<nint> callback) {
        if (this.scannerRef.TryGetTarget(out var scanner)) {
            this.scansRemaining++;
            scanner.AddMainModuleScan(sig, status => {
                if (status.Found) {
                    var addr = this.BaseAddr + status.Offset;
                    if (sig.StartsWith("E8") || sig.StartsWith("E9")) {
                        addr += 5 + Marshal.ReadInt32(addr + 1);
                    }
                    callback(addr);
                } else {
                    this.logger.PrintMessage($"Failed to scan {sig}", Color.Red);
                }

                this.scansRemaining--;
                if (this.scansRemaining == 0) {
                    // do something if we need to here
                }
            });
        }
    }
}
