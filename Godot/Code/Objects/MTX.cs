using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
	public class MTX : ICloneable
	{
		public List<Enums.ItemTypes> SendItems { get; set; }
		public List<Enums.ItemTypes> BalanceItems { get; set; }
		public Enums.ItemTypes CurrentItem { get; set; }
		public Enums.StellarBodies Target { get; set; }
		public int CurrentScroll { get; set; }

		public MTX() 
		{
			CurrentItem = Enums.ItemTypes.iron;
			CurrentScroll = 0;

			Target = Enums.StellarBodies.none;

			SendItems = new List<Enums.ItemTypes>();
			BalanceItems = new List<Enums.ItemTypes>();
		}

		public object Clone()
		{
			var newMTX = new MTX();
			newMTX.SendItems = new List<Enums.ItemTypes> (SendItems);
			newMTX.BalanceItems = new List<Enums.ItemTypes> (BalanceItems);
			newMTX.CurrentItem = CurrentItem;
			newMTX.Target = Target;

			return newMTX;
		}
	}
}