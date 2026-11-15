using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Interfaces;
using Godot;
using System;
using System.Reflection;
using System.Xml;

namespace Deuteros.Code.Objects
{
	public abstract partial class ButtonAdapter<ObjectDataType> : Platform.Base.HoverButton, Objects.Interfaces.IButton
	{
		public int EmissionValue;
		protected AnimatedSprite2D AnimatedSprite { get; set; }
		public bool _selected;
		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				_selected = value;
				Redraw(false);
			}
		}
		protected Enums.SidePanel_Button_State_Animations AnimationState { get; set; }

		public ObjectDataType ObjectData { get; set; }
		[Signal]
		public delegate void ClickedEventHandler(int index);

		public abstract void Redraw(bool dayPassed);
		public abstract void MouseClickedMe();

		public override void _Ready()
		{
			AnimatedSprite = GetNode<AnimatedSprite2D>("ButtonSpriteAnimation");

			Pressed += () => MouseClickedMe();

			Redraw(false);

			base._Ready();
		}

		public void Click()
		{
			EmitSignal("Clicked");
		}
	}
}
