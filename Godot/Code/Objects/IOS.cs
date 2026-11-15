using Deuteros.Code.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
	public class IOS : InterStellarShip, IShip
	{
		public IOS()
		{
			ShipID = Guid.NewGuid();
		}
	}
}