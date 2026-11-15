using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects.GameData
{
	[Serializable]
	public class Config
	{
		public int ShuttleRefuelThreshold { get; set; }
		public int IOSRefuelThreshold { get; set; }

	}
}
