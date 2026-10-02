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

        public void StationCaptured(SaveFile save, StellarBodies starId)
        {
            if (AssignedStars.Contains(starId)
                || save.BaseGameData.Planets.Values.Any(p => p.ParentStar == starId && p.ActiveMethanoid)) return;
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
        }
    }
}
