using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Deuteros.Code.Objects
{
    public sealed class GameClock
    {
        // 15,780 original VBlanks at the nominal PAL rate; runtime comparison is still required.
        public const double NormalIntervalSeconds = 315.6;
        public ulong DateCentidays { get; set; }
        public double NormalElapsed { get; set; }
        public int PendingIncrement { get; set; }
        [JsonIgnore] public ulong PreviousCentidays { get; private set; }
        public string RelativeDate => (DateCentidays / 100m).ToString("0.##", CultureInfo.InvariantCulture);

        public static string FormatDate(ulong centidays) => FormattableString.Invariant(
            $"{3100 + centidays / 100000} {(centidays / 100) % 1000:000}.{centidays % 100:00}");

        public void AdvanceNormal(double delta)
        {
            if (!double.IsFinite(delta) || delta <= 0 || PendingIncrement != 0) return;
            NormalElapsed += delta;
            if (NormalElapsed < NormalIntervalSeconds) return;
            // The original producer stops at its pending flag; stalled time is not a backlog.
            NormalElapsed = 0;
            QueueIncrement(1);
        }

        public void QueueManual() => QueueIncrement(100);

        private void QueueIncrement(int increment)
        {
            if (PendingIncrement != 0) return;
            DateCentidays = checked(DateCentidays + (ulong)increment);
            PendingIncrement = increment;
        }

        public bool Consume()
        {
            if (PendingIncrement == 0) return false;
            PreviousCentidays = DateCentidays - (ulong)PendingIncrement;
            PendingIncrement = 0;
            return true;
        }
    }
}
