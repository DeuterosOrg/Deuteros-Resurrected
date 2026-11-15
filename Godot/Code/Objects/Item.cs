using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
	public class Item
	{
		public Enums.ItemTypes ItemType { get; set; }
		public Enums.ItemCategory ItemCategory { get; set; }
		public ResearchItem Research { get; set; }
		public string FullName { get; set; }
		public string ShortName { get; set; }
		public List<BuildRequirement> BuildRequirements { get; set; }
		public bool OrbitOnly { get; set; }
		public bool UniqueItem { get; set; }
		public bool AutoProduce { get; set; }
		public bool AutoProduceFlip { get; set; }
		public bool UniquePerPlanetItem { get; set; }
		public int Mass { get; set; }
		public int Index { get; set; }
		public bool Locked { get; set; }
		public bool Production { get; set; }
		public bool ToolPod { get; set; }
		public bool ToolPodSingular { get; set; }

		public Item()
		{
			Production = true;
		}
	}
}