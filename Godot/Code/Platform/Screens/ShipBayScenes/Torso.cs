using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Platform.Screens.ShipBayScenes
{
	public partial class Torso : Control
	{
		public const string ComponentSpriteBasePath = "res://Sprites//SceneSprites//Ships//";
		public Color ProductionStaffColor { get; set; } = new Color("#002288");
		public Control SpriteHolder { get; set; }
		public TextureRect Component { get; set; }
		public Control Cargo { get; set; }
		public Label Contents { get; set; }
		public TextureButton AddSupplyPod { get; set; }
		public TextureButton AddToolPod { get; set; }
		public TextureButton AddCryoPod { get; set; }
		public Button ActivatePod { get; set; }
		public Staff CurrentStaff { get; set; }
		public ShipModule Module { get; set; }
		public int TorsoSection { get; set; }

		public delegate bool ChangeModuleDelegate(Module_Types moduletype, int torsoSection);
		public event ChangeModuleDelegate ModuleChanged;

		public delegate void OpenModuleDelegate(int torsoSection);
		public event OpenModuleDelegate ModuleOpened;

		public override void _Ready()
		{
			SpriteHolder = GetNode<Control>("SpriteHolder");
			Component = GetNode<TextureRect>("SpriteHolder/Cargo/Component");
			Cargo = GetNode<Control>("SpriteHolder/Cargo");
			Contents = GetNode<Label>("SpriteHolder/Labels/Contents");
			AddSupplyPod = GetNode<TextureButton>("SpriteHolder/Buttons/AddSupplyPod");
			AddToolPod = GetNode<TextureButton>("SpriteHolder/Buttons/AddToolPod");
			AddCryoPod = GetNode<TextureButton>("SpriteHolder/Buttons/AddCryoPod");
			ActivatePod = GetNode<Button>("SpriteHolder/Buttons/ActivatePod");

			AddSupplyPod.Pressed += () => ChangeModuleType(Module_Types.Supply);
			AddToolPod.Pressed += () => ChangeModuleType(Module_Types.Tool);
			AddCryoPod.Pressed += () => ChangeModuleType(Module_Types.Cryo);

			ActivatePod.Pressed += ActivatePod_Pressed;
		}

		private void ActivatePod_Pressed()
		{
			ModuleOpened.Invoke(TorsoSection);
		}

		public void UpdateState()
		{
			if (Module.ModuleType == Module_Types.None)
			{
				Cargo.Visible = false;
				Contents.Text = "";
				Contents.RemoveThemeColorOverride("font_color");
			}
			else if (Module.ModuleType == Module_Types.Tool)
			{
				Cargo.Visible = true;
				Contents.RemoveThemeColorOverride("font_color");

				if (Module.ItemStored == ItemTypes.none)
				{
					Component = SpriteManager.LoadImageToTextureRect(ComponentSpriteBasePath + "Component_ToolPod.png", Component);
					Contents.Text = "";
				}
				else if (GameCore.SingletonInstance.GameData.GetItem(Module.ItemStored).ItemCategory == Enums.ItemCategory.item)
				{
					var spritePath = ComponentSpriteBasePath + "Component_" + Module.ItemStored.ToScreenString().Replace(".", "").Replace(" ", "") + ".png";

					if (ResourceLoader.Exists(spritePath))
						Component = SpriteManager.LoadImageToTextureRect(spritePath, Component);
					else
						Component = SpriteManager.LoadImageToTextureRect(ComponentSpriteBasePath + "Component_Generic.png", Component);

					Contents.Text = GameCore.SingletonInstance.GameData.GetItem(Module.ItemStored).ShortName
						?? Module.ItemStored.ToScreenString(" ");
				}
			}
			else if (Module.ModuleType == Module_Types.Supply)
			{
				Cargo.Visible = true;
				Component = SpriteManager.LoadImageToTextureRect(ComponentSpriteBasePath + "Component_SupplyPod.png", Component);
				Contents.RemoveThemeColorOverride("font_color");

				if (Module.ItemCount > 0)
					Contents.Text = Module.ItemCount.ToString() + "\n" + Module.ItemStored.ToScreenString(" ");
				else
					Contents.Text = "";
			}
			else if (Module.ModuleType == Module_Types.Cryo)
			{
				Cargo.Visible = true;
				Component = SpriteManager.LoadImageToTextureRect(ComponentSpriteBasePath + "Component_CryoPod.png", Component);

				if (Module.StaffStored != null)
				{
                    Contents.Text = Module.StaffStored.GetTypeText2() + "\n" + Module.StaffStored.Count;

					if (Module.StaffStored.Type == StaffType.Production)
						Contents.AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Blue);
					else if (Module.StaffStored.Type == StaffType.Marines)
						Contents.AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red);
				}
				else
				{
					Contents.Text = "";
					Contents.RemoveThemeColorOverride("font_color");
				}
			}
		}

		public void ChangeModuleType(Module_Types moduleType)
		{
			bool success;

			if (moduleType == Module.ModuleType)
				success = ModuleChanged.Invoke(Module_Types.None, TorsoSection);
			else
				success = ModuleChanged.Invoke(moduleType, TorsoSection);

			if (success)
				UpdateState();
		}

		public void ChangeModule(ShipModule module)
		{
			Module = module;

			UpdateState();
		}
	}
}
