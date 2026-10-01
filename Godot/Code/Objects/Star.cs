using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace Deuteros.Code.Objects
{
    [Serializable]
    public partial class Star
    {
        public Enums.StellarBodies StarId { get; set; }
        public Enums.StellarBodies ArtifactLocation { get; set; }
        public List<int> PlanetDistanceList { get; set; }

        public Star(Enums.StellarBodies planetId)
        {
            StarId = planetId;
            PlanetDistanceList = new List<int>();
        }
    }
}