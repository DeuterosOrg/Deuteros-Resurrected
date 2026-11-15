using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
	public class BuildRequirement
	{
		public Enums.ItemTypes ItemType { get; set; }
		public int ItemCount { get; set; }

		public BuildRequirement(Enums.ItemTypes itemType,  int itemCount)
		{
			ItemType = itemType;
			ItemCount = itemCount;
		}
	}
}
