using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Deuteros.Code.Objects
{
	[Serializable]
	public partial class PlanetResource : Deuteros.Code.Platform.Resource
	{
		public int Derricks { get; set; }
		public List<Objects.Material> Materials { get; set; }

		public PlanetResource(List<Objects.Material> materials)
		{
			Materials = materials;

			Stores = new Store();
		}
	}
}