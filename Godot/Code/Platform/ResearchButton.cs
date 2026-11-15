using Deuteros.Code;
using Deuteros.Code.Objects;
using Godot;
using System;
using System.Reflection;
using System.Xml;

namespace Deuteros.Code.Platform
{
	public partial class ResearchButton : ButtonAdapter<ResearchItem>
	{
		public ResearchButton()
		{
		}

		public override void Redraw(bool dayPassed)
		{
			if (ObjectData != null)
			{
				if (ObjectData.ItemType != Enums.ItemTypes.none)
				{
					var item = GameCore.SingletonInstance.GameData.GetItem(ObjectData.ItemType);

					HoverText = (item.Research.Locked) ? "" : item.FullName;
				}

				if (Selected)
				{
					if (ObjectData.Locked)
						AnimationState = Enums.SidePanel_Button_State_Animations.Static_Locked;
					else if (ObjectData.Researched)
						AnimationState = Enums.SidePanel_Button_State_Animations.Green;
					else if (ObjectData.ResearchPercentageComplete > 1)
						AnimationState = Enums.SidePanel_Button_State_Animations.Yellow;
					else
						AnimationState = Enums.SidePanel_Button_State_Animations.Red;
				}
				else
				{
					if (ObjectData.Locked)
						AnimationState = Enums.SidePanel_Button_State_Animations.Static_Locked;
					else if (ObjectData.Researched)
						AnimationState = Enums.SidePanel_Button_State_Animations.Static_Green;
					else if (ObjectData.ResearchPercentageComplete > 1)
						AnimationState = Enums.SidePanel_Button_State_Animations.Static_Yellow;
					else
						AnimationState = Enums.SidePanel_Button_State_Animations.Static_Red;
				}
			}
			else if (ObjectData == null)
			{
				AnimationState = Enums.SidePanel_Button_State_Animations.Static_Locked;
			}

			if (AnimatedSprite != null && AnimatedSprite.Animation.ToString() != AnimationState.ToString())
				AnimatedSprite.Play(AnimationState.ToString());
		}

		public override void MouseClickedMe()
		{
			EmitSignal("Clicked", ObjectData == null ? 0 : ObjectData.Index);
		}
	}
}
