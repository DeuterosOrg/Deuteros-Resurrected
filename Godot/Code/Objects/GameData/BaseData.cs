using Deuteros.Code.Objects.Bulletins;
using Deuteros.Code.Objects.ModuleTextFrame;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects.GameData
{
    [Serializable]
    public class BaseData
    {
        public List<string> PersonNames { get; set; }
        public Dictionary<Enums.StellarBodies, Deuteros.Code.Objects.Interfaces.IPlanet> Planets { get; set; }
        public Dictionary<Enums.StellarBodies, Deuteros.Code.Objects.Star> Stars { get; set; }
        public List<Item> ItemList { get; set; }
		public FrameContainer ModuleFrameTexts { get; set; }
		public BulletinContainer BulletinTexts { get; set; }
		public Dictionary<Enums.ItemTypes, int> ResourceLevels_Survey_Multiplier { get; set; }
        public Dictionary<Enums.ItemTypes, int> ResourceRate_Per_Derrick { get; set; }
        // HTML values preserve the original byte RGB palette; Color's numeric constructor uses 0–1 floats.
        public Color Red { get; set; } = new Color("#ff0000");
        public Color Green { get; set; } = new Color("#008800");
        public Color Blue { get; set; } = new Color("#002288");
        public Color Beige { get; set; } = new Color("#99aa77");
        public Color Dark_Beige { get; set; } = new Color("#556633");
        public Color Yellow { get; set; } = new Color("#ffff00");
        public Color LightBlue { get; set; } = new Color("#aaccee");
    }
}
