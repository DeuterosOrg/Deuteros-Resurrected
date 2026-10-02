using System;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task QueuedNavigationInputLifetime()
        {
            // This exact navigation/collection sequence reproduced the case-61
            // handle errors. Preserve real controls, scene changes and input routing.
            for (var cycle = 0; cycle < 80; cycle++)
            {
                await RightClickNavigation(Scenes.Store);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
