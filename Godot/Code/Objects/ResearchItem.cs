using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
    public class ResearchItem
    {
        public Enums.ItemTypes ItemType { get; set; }
        public int ResearchMultiplier { get; set; }
        public bool Researched { get; set; }
        public int Index { get; set; }
        public bool Locked { get; set; }
        public int TechLevel { get; set; }
        public int ResearchValue { get; set; }
        public int ResearchPercentageComplete { get; set; }
        public int ResearchOrder { get; set; }
        public int ResearchLimit { get; set; }

        public ResearchItem()
        { }

        public ResearchItem(Enums.ItemTypes itemType, int index, int techLevel)
        { 
            ItemType = itemType;
            TechLevel = techLevel;
            Index = index;
            ResearchMultiplier = 64;
            Researched = false;
            ResearchValue = 64;
            ResearchPercentageComplete = 1;
            Locked = true;
            ResearchLimit = 100;
        }
    }
}