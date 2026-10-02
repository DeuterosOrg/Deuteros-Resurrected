using System;
using System.Collections.Generic;
using System.Linq;
using Deuteros.Code.Objects.GameData;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
    public class AlienTransmissions
    {
        public List<StellarBodies> AssignedStars { get; set; } = new() { StellarBodies.the_sun };
        public List<StellarBodies> PendingLocations { get; set; } = new();
        public bool PendingCaptureDiscovery { get; set; }
        // Enemy scheduling samples ownership before the separate discovery delay consumes it.
        public int EnemySystems { get; set; } = 9;
        public int HyperlightSystems { get; set; } = 9;
        public int HyperlightCountdown { get; set; }
        public bool HyperlightPending { get; set; }

        public int Stage { get; set; } = -1;
        public int Countdown { get; set; }
        public bool Ready { get; set; }
        public int LastStage { get; set; } = -1;
        public StellarBodies LastLocation { get; set; }
        public uint LastMask { get; set; }

        public void SampleEnemySystems(SaveFile save)
            => EnemySystems = save.BaseGameData.Planets.Values.Where(p => p.ActiveMethanoid)
                .Select(p => p.ParentStar).Distinct().Count();

        public void AdvanceHyperlight(SaveFile save)
        {
            if (!save.AtWar || !save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research.Locked) return;
            // Original $37810 consumes an active delay before observing a changed count.
            if (HyperlightCountdown > 0) { HyperlightCountdown--; return; }
            if (EnemySystems != HyperlightSystems)
            {
                HyperlightSystems = EnemySystems;
                HyperlightCountdown = EnemySystems == 7 ? 8 : 0;
                return;
            }
            if (EnemySystems == 7) HyperlightPending = true;
        }

        public bool DiscoverHyperlight(SaveFile save)
        {
            if (!HyperlightPending) return false;
            HyperlightPending = false;
            var research = save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research;
            if (!research.Locked) return false;
            research.Locked = false;
            return true;
        }

        public void Start()
        {
            if (Stage >= 0) return;
            Stage = 0;
            Countdown = 10;
        }

        public bool Advance()
        {
            if (Ready) return true;
            if (Countdown == 0) return false;
            Ready = --Countdown == 0;
            return Ready;
        }

        public void FinalRecovery()
        {
            PendingLocations.Clear();
            Stage = 13;
            Countdown = 2;
            Ready = false;
        }

        public bool BeginDisplay()
        {
            if (!Ready || Stage < 0) return false;
            var locationMessage = Stage >= 5 && Stage <= 12;
            if (locationMessage && PendingLocations.Count == 0) return false;
            LastStage = Stage;
            LastLocation = locationMessage ? PendingLocations[0] : StellarBodies.none;
            LastMask = Stage == 0 ? 0 : Stage == 1 ? 0x08945949 : (uint)Random.Shared.NextInt64(1L << 32);
            if (locationMessage) PendingLocations.RemoveAt(0);
            Ready = false;
            Countdown = Stage switch { 0 or 1 => 250, 2 => 80, 3 => 200, _ => 0 };
            if (Stage < 12) Stage++;
            if (Stage >= 5 && Stage <= 12 && PendingLocations.Count > 0) Countdown = 2;
            return true;
        }

        public void StationCaptured(SaveFile save, StellarBodies starId)
        {
            if (save.BaseGameData.Planets.Values.Any(p => p.ParentStar == starId && p.ActiveMethanoid)) return;
            PendingCaptureDiscovery = true;
            if (AssignedStars.Contains(starId)) return;
            var star = save.BaseGameData.Stars[starId];
            if (star.ArtifactLocation == StellarBodies.none)
            {
                // Original $35C76 masks RNG to five bits before reducing by system size.
                // Keep the original parent-then-moon ordering, including its >32-body bias.
                var bodies = save.BaseGameData.Planets.Values.Where(p => p.ParentStar == starId)
                    .OrderBy(p => p.IsMoon ? save.BaseGameData.Planets[p.MoonParentPlanetId].Order : p.Order)
                    .ThenBy(p => p.IsMoon).ThenBy(p => p.Order).ToList();
                star.ArtifactLocation = bodies[Random.Shared.Next(32) % bodies.Count].PlanetId;
            }
            AssignedStars.Add(starId);
            PendingLocations.Add(star.ArtifactLocation);
            if (Stage >= 0 && Stage < 13 && !Ready) Countdown = 2;
        }

        public bool DiscoverScg(SaveFile save)
        {
            if (!PendingCaptureDiscovery) return false;
            PendingCaptureDiscovery = false;
            if (!save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.g_chassis).Research.Locked) return false;
            foreach (var type in new[] { ItemTypes.g_chassis, ItemTypes.star_drive, ItemTypes.hed_fuel })
                save.BaseGameData.ItemList.Single(i => i.ItemType == type).Research.Locked = false;
            if (!save.Unlocks.Contains(Game_Unlocks.Interstellar_Travel)) save.Unlocks.Add(Game_Unlocks.Interstellar_Travel);
            return true;
        }
    }
}
