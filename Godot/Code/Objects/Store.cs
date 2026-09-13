using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
    [Serializable]
    public partial class Store
    {
        public Dictionary<ItemTypes, int> Items { get; set; }
		//Display Mineral list or alt(Could be items or MTX)
        public bool AlternativeView { get; set; }
		public Objects.MTX MTX { get; set; }

		public Store()
        {
            Items = new Dictionary<ItemTypes, int>();
            MTX = new Objects.MTX();
        }

        public int this[ItemTypes itemType]
        {
            get
            {
                //TODO DEBUG
                if (GameCore.SingletonInstance.InfiniteResources)
                    return 50000;

                return Items.ContainsKey(itemType) ? Items[itemType] : 0;
            }
            set 
            {
                if (Items.ContainsKey(itemType))
                    Items[itemType] = value;
                else
                    Items.Add(itemType, value);
            }
        }
    }
}