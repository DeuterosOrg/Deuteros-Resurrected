using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
	public partial class MTXRow : Control
	{
		public TextureButton LeftArrow { get; set; }
		public TextureButton LeftCross { get; set; }
		public TextureButton LeftDiamond { get; set; }
		public TextureButton RightCross { get; set; }
		public TextureButton RightDiamond { get; set; }
		public Label RowName { get; set; }
		public Label Count { get; set; }

		public Enums.ItemTypes ItemType { get; set; }

		public void Load(Enums.ItemTypes itemType)
		{
			LeftArrow = this.GetNode<TextureButton>("LeftArrow");
			LeftDiamond = this.GetNode<TextureButton>("LeftDiamond");
			LeftCross = this.GetNode<TextureButton>("LeftCross");
			RightDiamond = this.GetNode<TextureButton>("RightDiamond");
			RightCross = this.GetNode<TextureButton>("RightCross");
			RowName = this.GetNode<Label>("Name");
			Count = this.GetNode<Label>("Count");

			ItemType = itemType;
		}
	}
}