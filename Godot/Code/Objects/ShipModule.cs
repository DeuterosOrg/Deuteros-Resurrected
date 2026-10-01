using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
    public class ShipModule
    {
        public Enums.Module_Types ModuleType { get; set; }
        public Enums.ItemTypes ItemStored { get; set; }
        public Staff StaffStored { get; set; }
        //Used for count of items and amount of ore
        public int ItemCount { get; set; }
		public GrappleItem HeldItem { get; set; }
        //The last day an attached AMA mined resources
        public uint LastMinedDay { get; set; }

		public ShipModule()
        {
            ItemStored = Enums.ItemTypes.none;
            ModuleType = Enums.Module_Types.None;
            StaffStored = null;
            ItemCount = 0;
            HeldItem = null;
		}
    }
}
