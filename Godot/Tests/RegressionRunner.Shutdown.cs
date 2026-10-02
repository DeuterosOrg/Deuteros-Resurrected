using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private static readonly ManualResetEventSlim ShutdownFinalizerEntered = new(false);
        private static readonly ManualResetEventSlim ShutdownFinalizerRelease = new(false);

        private sealed class ShutdownFinalizerGate
        {
            ~ShutdownFinalizerGate()
            {
                ShutdownFinalizerEntered.Set();
                // Bound the diagnostic even if the releasing worker cannot run.
                ShutdownFinalizerRelease.Wait(5000);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void QueueShutdownFinalizerGate() => _ = new ShutdownFinalizerGate();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference<GodotObject>[] QueueShutdownResources()
        {
            return Enumerable.Range(0, 64).Select(index =>
                new WeakReference<GodotObject>(new Resource { ResourceName = "shutdown-regression-" + index })).ToArray();
        }

        private void PendingFinalizersAtShutdown()
        {
            // Godot 4.2.2 tracks wrappers through weak references. Cleared weak
            // targets are skipped at engine teardown even if their finalizers
            // have not yet released the native references. Hold the finalizer
            // briefly to make that otherwise intermittent boundary reproducible.
            ShutdownFinalizerEntered.Reset();
            ShutdownFinalizerRelease.Reset();
            QueueShutdownFinalizerGate();
            GC.Collect();
            if (!ShutdownFinalizerEntered.Wait(2000))
            {
                ShutdownFinalizerRelease.Set();
                throw new InvalidOperationException("Shutdown finalizer gate did not start");
            }
            var resources = QueueShutdownResources();
            GC.Collect();
            // The worker uses no Godot API. Production shutdown must wait for
            // the actual finalizers, rather than sleeping for this test delay.
            _ = Task.Run(async () => { await Task.Delay(1000); ShutdownFinalizerRelease.Set(); });
            Equal(64, resources.Count(reference => !reference.TryGetTarget(out _)), "weak targets cleared while native cleanup is pending");
            closeWindowAfterTests = true;
            // The existing strict runner checks the entire process log and
            // clean exit, after this method and the production close path run.
        }
    }
}
