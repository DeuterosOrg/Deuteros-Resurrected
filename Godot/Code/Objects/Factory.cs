using Deuteros.Code.Platform.Screens;
using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
	public class Factory
	{
		//The item being produced, and the number of days it has been in production
		public List<ProductionItem> ProductionQueue { get; set; }
		public Staff Builder { get; set; }
		public bool AOC { get; set; }
		public bool Ground { get; set; }
		//Helps maintain animation on main screen
		public int ProdCycle { get; set; }

		public Factory()
		{
			ProductionQueue = new List<ProductionItem>();
		}

		public ProductionItem CurrentProductionItem()
		{
			return ProductionQueue.SingleOrDefault(T => T.Active);
		}

		public void IncrementCurrentProd()
		{

			var currentProductionItem = CurrentProductionItem();

			if (currentProductionItem != null)
			{
				ProdCycle++;

				if (ProdCycle == 7)
					ProdCycle = 0;

				var VRatio = 0;

				if (!AOC)
					VRatio = (Builder.Count << Builder.GetLevel()) * currentProductionItem.Object_Multiplier / 801;
				else
					VRatio = 128;

				if ((currentProductionItem.Production_Value + VRatio) > 255)
				{
					currentProductionItem.Production_Value = (currentProductionItem.Production_Value + VRatio) & 0xFF; // keep low 8 bits

					if (currentProductionItem.Production_Complete < 4)
					{
						currentProductionItem.Production_Complete++;
					}
				}
				else
				{
					currentProductionItem.Production_Value += VRatio;
				}
			}
		}

		public void ClearQueue()
		{
			ProductionQueue.Clear();
		}
	}
}