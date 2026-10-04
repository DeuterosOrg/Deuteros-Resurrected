using Deuteros.Code.Platform.Base;
using Deuteros.Code.Objects;
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Deuteros.Code.Platform.Helpers;
using System.Security.Cryptography.X509Certificates;
using Deuteros.Code.Objects.Interfaces;
using static System.Collections.Specialized.BitVector32;
using System.ComponentModel.Design;
using Deuteros.Code.Utility;
using Godot.NativeInterop;

namespace Deuteros.Code.Platform.Screens
{
	public partial class Production : BaseSubScene
	{
		public const string ProductionIllustrationBasePath = "res://Sprites/Items/Production/Illustrations/";
		public const string ProductionProgressSpriteBasePath = "res://Sprites//Items//Production//";
		public List<ProductionButton> Buttons { get; set; }
		public ProductionButton SelectedButton { get; set; }
		Label ProductionNameLabel { get; set; }
		Label StaffNameLabel { get; set; }
		Label StaffRankLabel { get; set; }
		Label StaffCountLabel { get; set; }
		TextureRect SmallItemImageTextureRect { get; set; }
		TextureRect ItemProgressImageTextureRect { get; set; }
		TextureRect TeamFrameImage { get; set; }
		TextureRect AocPanel { get; set; }
		AnimatedSprite2D ProductionRod;
		// Original $20582 is a display counter retained across production-screen visits.
		static int RodCounter;
		double RodTicks;

		TextureButton RemoveStaff { get; set; }

		bool Ground {  get; set; }

		IPlanet CurrentPlanet { get; set; }
		Code.Objects.Factory CurrentFactory { get; set; }

		public override void _Ready()
		{
			ProductionNameLabel = GetNode<Label>("Labels/ProductionNameLabel");
			StaffNameLabel = GetNode<Label>("Labels/StaffNameLabel");
			StaffRankLabel = GetNode<Label>("Labels/StaffRankLabel");
			StaffCountLabel = GetNode<Label>("Labels/StaffCountLabel");

			SmallItemImageTextureRect = GetNode<TextureRect>("Sprites/SmallItemImage");
			ItemProgressImageTextureRect = GetNode<TextureRect>("Sprites/ItemProgressImage");
			SmallItemImageTextureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			SmallItemImageTextureRect.StretchMode = TextureRect.StretchModeEnum.Keep;
			ItemProgressImageTextureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			ItemProgressImageTextureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			TeamFrameImage = GetNode<TextureRect>("Sprites/TeamFrameImage");
			AocPanel = GetNode<TextureRect>("Sprites/AocPanel");
			ProductionRod = GetNode<AnimatedSprite2D>("Sprites/ProductionRod");

			RemoveStaff = GetNode<TextureButton>("RemoveStaff");

			RemoveStaff.Pressed += RemoveStaff_Pressed;

			CurrentPlanet = GameCore.SingletonInstance.GetCurrentPlanet();

			Ground = SceneVariables.Contains(Enums.SceneVariables.Ground);

			if (Ground)
				CurrentFactory = ((Earth)CurrentPlanet).Factory;
			else
				CurrentFactory = CurrentPlanet.Station.Factory;

			RefreshButtons();

			base._Ready();
		}

		public override void _Process(double delta)
		{
			if (!CanProcess() || CurrentFactory.CurrentProductionItem() == null) return;
			// $20584: nominal PAL VBlank, draw on even ticks, resource 130 - (counter >> 2).
			// Manual stepping retains the original short first frame and inactive phase.
			RodTicks += delta * 50;
			while (RodTicks >= 1)
			{
				RodTicks--;
				RodCounter = (RodCounter + 1) % 12;
				if ((RodCounter & 1) == 0) ProductionRod.Frame = RodCounter >> 2;
			}
		}

		private void RemoveStaff_Pressed()
		{
			if (CurrentFactory.Builder != null && CurrentFactory.Builder.Count > 0 && (Ground ? CurrentPlanet.PlanetResources.Staff : CurrentPlanet.Station.Resources.Staff).Any(T => T == null))
			{
				((Resource)(Ground ? CurrentPlanet.PlanetResources : CurrentPlanet.Station.Resources)).AddStaff(CurrentFactory.Builder);
				CurrentFactory.Builder = null;

				// Removing a team pauses paid work; its completed stages and materials remain reserved.
				var currentItem = CurrentFactory.CurrentProductionItem();
				if (currentItem != null)
				{
					currentItem.Production_Value = currentItem.Product.Research.ResearchValue;
					currentItem.Active = false;
				}

				SelectedButton = null;
				RefreshButtons();

				DrawData();
			}
		}

		private void ProductionButton_Clicked(int index)
		{
			//TODO Clicking a product you cannot build still somehow builds it - Not sure this is correct
			var clickedButton = Buttons.Single(T => T.ObjectData != null && T.ObjectData.Research.ResearchOrder == index);

			if (SelectedButton != null && !CurrentFactory.AOC)
				SelectedButton.Selected = false;

			SelectedButton = clickedButton;
			var recipeStore = Ground ? CurrentPlanet.PlanetResources.Stores : CurrentPlanet.Station.Resources.Stores;
			recipeStore.SelectedRecipe = clickedButton.ObjectData.ItemType;
			recipeStore.AlternativeView = false;

			if (!CurrentFactory.AOC)
				SelectedButton.Selected = true;

			CheckProductionStart();

			DrawData();
			clickedButton.Redraw(false);
		}

		private void CheckProductionStart()
		{
			if (SelectedButton?.ObjectData?.OrbitOnly == true && CurrentFactory.Ground)
			{
				GameCore.ShowError(this, "This Item Can\nOnly BE Made\nIn Orbit.");
				return;
			}
			if (SelectedButton?.ObjectData != null && IsStationInstallation(SelectedButton.ObjectData.ItemType)
				&& !CanInstallStationItem(CurrentPlanet, SelectedButton.ObjectData.ItemType, Ground)) return;
			{
				//There is no staff
				if (!CurrentFactory.AOC && (CurrentFactory.Builder == null || CurrentFactory.Builder.Count == 0))
					return;

				if (!CurrentFactory.AOC)
				{
					if (SelectedButton != null)
					{
						var addedItem = (Item)SelectedButton.ObjectData;

						
						if (addedItem.Research.TechLevel>CurrentFactory.Builder.GetLevel())
						{
							string level="";
							switch (addedItem.Research.TechLevel)
							{
								case 2:
									level = "Engineer   ";
									break;
								case 3:
									level = "Expert     ";
									break;
							}

							GameCore.ShowError(this, "Team Leader\nMust Be Rated\n" + level + "To\nProduce This\nItem"); ;
							return;

						}

						if (CurrentFactory.CurrentProductionItem() == null || CurrentFactory.CurrentProductionItem().Product.ItemType != addedItem.ItemType)
						{
							// Manual queued jobs already paid when created, including across save/load.
							if (CurrentFactory.ProductionQueue.Any(T => T.Product.ItemType == addedItem.ItemType)
								|| CheckResourceAvailable(CurrentPlanet, addedItem, Ground))
							{
								if (CurrentFactory.CurrentProductionItem() != null)
								{
									CurrentFactory.CurrentProductionItem().Production_Value = CurrentFactory.CurrentProductionItem().Product.Research.ResearchValue;
									CurrentFactory.CurrentProductionItem().Active = false;
								}

								if (CurrentFactory.ProductionQueue.Any(T => T.Product.ItemType == addedItem.ItemType))
								{
									CurrentFactory.ProductionQueue.Single(T => T.Product.ItemType == addedItem.ItemType).Active = true;
								}
								else
								{
									var newProdItem = new ProductionItem(addedItem);
									newProdItem.AOCOneTime = false;
									newProdItem.AOCRepeat = false;
									newProdItem.Active = true;
									newProdItem.Production_Value = addedItem.Research.ResearchValue;
									CurrentFactory.ProductionQueue.Add(newProdItem);

									RemoveResourceByItem(CurrentPlanet, addedItem, Ground);
								}
							}
						}
					}
				}
				else if (CurrentFactory.AOC)
				{
					if (SelectedButton != null)
					{
						var addedItem = (Item)SelectedButton.ObjectData;
						var production = CurrentFactory.ProductionQueue.SingleOrDefault(T => T.Product.ItemType == addedItem.ItemType);

						if (production == null)
						{
							var newProdItem = new ProductionItem(addedItem);
							newProdItem.AOCOneTime = true;
							newProdItem.AOCRepeat = false;
							newProdItem.Production_Value = addedItem.Research.ResearchValue;

							CurrentFactory.ProductionQueue.Add(newProdItem);
						}
						else if (production.AOCOneTime)
						{
							production.AOCRepeat = true;
							production.AOCOneTime = false;
						}
						else if (production.AOCRepeat)
						{
							if (production.Active)
							{
								production.AOCRepeat = false;
								production.AOCOneTime = false;
							}
							else
							{
								CurrentFactory.ProductionQueue.Remove(production);
							}
						}
						SelectedButton = null;
					}
				}
			}
		}

		protected override void ResearchFinished(Objects.ResearchItem researchItem)
		{
			Item selectedItem = null;

			if (SelectedButton != null)
				selectedItem = SelectedButton.ObjectData;

			RefreshButtons();

			if (selectedItem != null)
			{
				SelectedButton = Buttons.Single(T => T.ObjectData != null && T.ObjectData.ItemType == selectedItem.ItemType);
				SelectedButton.Selected = true;
			}

			DrawData();
		}

		private void RefreshButtons()
		{
			Buttons = Utility.Buttons.CreateButtons<ProductionButton, Item>(GetNode<GridContainer>("ProductionButtonGrid"),
			GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Production && T.BuildRequirements != null && !T.Locked
			&& !T.AutoProduce
			).Select(T => T).OrderBy(T => T.Research.ResearchOrder).ToDictionary(obj => obj.Research.ResearchOrder),
			this,
			nameof(ProductionButton_Clicked),
			"/Code/Platform/ProductionButton.cs",
			"ProductionButton");
		}

		protected override void ProductionFinished(Objects.Factory factory)
		{
			if (factory == CurrentFactory)
			{
				SelectedButton = null;
				foreach (var button in Buttons.Where(b => b.ObjectData != null && IsStationInstallation(b.ObjectData.ItemType)))
					button.Redraw(false);
			}
		}

		//Triggered from gamecore
		protected override void DayTick(uint previousDay, uint currentDay)
		{
			CheckProductionStart();

			DrawData();
		}

		// Called every update.
		public override void _Draw()
		{
			DrawData();
		}

		public void DrawData()
		{
			AocPanel.Visible = CurrentFactory.AOC;
			TeamFrameImage.Visible = !CurrentFactory.AOC;
			StaffNameLabel.Visible = StaffRankLabel.Visible = StaffCountLabel.Visible = !CurrentFactory.AOC;
			RemoveStaff.Visible = !CurrentFactory.AOC;
			if (CurrentFactory.Builder != null)
			{
				StaffCountLabel.Text = CurrentFactory.Builder.Count.ToString();
				StaffNameLabel.Text = CurrentFactory.Builder.Leader;
				StaffRankLabel.Text = ((Enums.StaffLevel_Production)CurrentFactory.Builder.GetLevel()).ToString();
			}
			else
			{
				StaffCountLabel.Text = "";
				StaffNameLabel.Text = "None";
				StaffRankLabel.Text = "";
			}

			if (CurrentFactory.CurrentProductionItem() != null)
			{
				var currentProductionItem = CurrentFactory.CurrentProductionItem();
				ProductionNameLabel.Text = currentProductionItem.Product.ShortName;
				SmallItemImageTextureRect = SpriteManager.LoadImageToTextureRect(ProductionIllustrationBasePath + currentProductionItem.Product.ItemType.ToString() + ".png", SmallItemImageTextureRect);
				var progressSprite = ProductionProgressSpriteBasePath + currentProductionItem.Product.ItemType.ToString() + "_" + currentProductionItem.Production_Complete + ".png";
				// Recovered source-sheet frames use atlas resources; retain the illustration
				// fallback for items whose supplied sheets still contain placeholder artwork.
				if (!ResourceLoader.Exists(progressSprite))
				{
					var atlasSprite = ProductionProgressSpriteBasePath + currentProductionItem.Product.ItemType.ToString() + "_" + currentProductionItem.Production_Complete + ".tres";
					progressSprite = ResourceLoader.Exists(atlasSprite) ? atlasSprite
						: ProductionIllustrationBasePath + currentProductionItem.Product.ItemType.ToString() + ".png";
				}
				ItemProgressImageTextureRect = SpriteManager.LoadImageToTextureRect(progressSprite, ItemProgressImageTextureRect);
			}
			else
			{
				ProductionNameLabel.Text = "";
				SmallItemImageTextureRect.Texture = null;
				ItemProgressImageTextureRect = SpriteManager.LoadImageToTextureRect(ProductionProgressSpriteBasePath + "idle.png", ItemProgressImageTextureRect);
			}
			// Source sheets remain byte-for-byte originals. Only their magenta key is
			// transparent at display time; ordinary PNGs retain their existing alpha.
			ItemProgressImageTextureRect.Material = ItemProgressImageTextureRect.Texture is AtlasTexture
				? GD.Load<ShaderMaterial>("res://Sprites/Items/Sheets/SourceSheet.tres") : null;

			if (CurrentFactory.AOC)
			{
				foreach(ProductionButton b in Buttons)
				{
					b.Redraw(false);
				}
			}

		}

		#region Statics

		private static bool IsStationInstallation(Enums.ItemTypes type) =>
			type == Enums.ItemTypes.m__t__x || type == Enums.ItemTypes.s__d__m;

		private static bool StationItemInstalled(IPlanet planet, Enums.ItemTypes type) => type switch
		{
			Enums.ItemTypes.m__t__x => planet.Station.MtxInstalled,
			Enums.ItemTypes.s__d__m => planet.Station.SdmInstalled,
			_ => false
		};

		private static bool CanInstallStationItem(IPlanet planet, Enums.ItemTypes type, bool ground)
		{
			return !ground && planet.Station.Built && !planet.ActiveMethanoid && !StationItemInstalled(planet, type);
		}

		public static void UpdateProduction(uint previousDay, uint currentDay)
		{
			foreach (var planet in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets)
			{
				var currentFactories = new List<Factory>() { planet.Value.Station.Factory };
				var currentPlanet = (Planet)planet.Value;

				//TODO This is garbage code, make it better
				if (planet.Value.PlanetId == Enums.StellarBodies.earth)
					currentFactories.Add(((Earth)planet.Value).Factory);

				foreach (var currentFactory in currentFactories)
				{
					if (currentFactory != null)
					{
						if (!currentFactory.Ground)
							currentFactory.ProductionQueue.RemoveAll(order => StationItemInstalled(currentPlanet, order.Product.ItemType));
						var currentItem = currentFactory.CurrentProductionItem()?.Product;
						if (currentItem?.OrbitOnly == true && currentFactory.Ground) continue;
						if (currentItem != null && IsStationInstallation(currentItem.ItemType)
							&& !CanInstallStationItem(currentPlanet, currentItem.ItemType, currentFactory.Ground)) continue;

						if (currentFactory.CurrentProductionItem() == null && currentFactory.AOC)
						{
							var productionItem = currentFactory.ProductionQueue.FirstOrDefault(T => CheckResourceAvailable(currentPlanet, T.Product, currentFactory.Ground)); ;

							if (productionItem != null)
							{
								RemoveResourceByItem(currentPlanet, productionItem.Product, currentFactory.Ground);
								productionItem.Active = true;
							}
						}

						currentFactory.IncrementCurrentProd();

						if (currentFactory.CurrentProductionItem() != null)
						{
							//Production complete
							if (currentFactory.CurrentProductionItem().Complete)
							{

								var outputStore = currentFactory.Ground ? currentPlanet.PlanetResources.Stores : currentPlanet.Station.Resources.Stores;
								var completedType = currentFactory.CurrentProductionItem().Product.ItemType;
								var installsStationItem = IsStationInstallation(completedType) && !currentFactory.Ground;
								if (installsStationItem)
								{
									if (completedType == Enums.ItemTypes.m__t__x) currentPlanet.Station.MtxInstalled = true;
									else currentPlanet.Station.SdmInstalled = true;
								}
								else
									outputStore[currentFactory.CurrentProductionItem().Product.ItemType]++;

								if (!currentFactory.AOC) currentFactory.Builder.AddAction();
								currentFactory.ProdCycle = 0;

								GameCore.SingletonInstance.TriggerProductionFinished(currentFactory);

								if (!currentFactory.AOC)
								{
									if (currentFactory.CurrentProductionItem().Product.ItemType == Enums.ItemTypes.a__o__c)
									{
										currentPlanet.Station.Resources.AddStaff(currentFactory.Builder);
										currentFactory.Builder = null;
										currentFactory.AOC = true;
									}

									currentFactory.ProductionQueue.Remove(currentFactory.CurrentProductionItem());
								}
								/*else if (currentFactory.ProductionQueue.Any(T => T.AOCRepeat))
								{
									var currentResearchOrder = currentFactory.CurrentProductionItem().Product.Research.ResearchOrder;

									if (currentFactory.CurrentProductionItem().AOCOneTime)
										currentFactory.ProductionQueue.Remove(currentFactory.CurrentProductionItem());

									if (currentFactory.ProductionQueue.Where(T => CheckResourceAvailable(currentPlanet, T.Product, currentFactory.Ground)).Count() > 0)
									{
										if (currentFactory.ProductionQueue.Any(T => T.Product.Research.ResearchOrder > currentResearchOrder))
											currentFactory.ProductionQueue.OrderBy(T => T.Product.Research.ResearchOrder).Where(T => CheckResourceAvailable(currentPlanet, T.Product, currentFactory.Ground) && T.Product.Research.ResearchOrder > currentResearchOrder).First().Active = true;
										else
											currentFactory.ProductionQueue.OrderBy(T => T.Product.Research.ResearchOrder).Where(T => CheckResourceAvailable(currentPlanet, T.Product, currentFactory.Ground)).First().Active = true;

										RemoveResourceByItem(currentPlanet, currentFactory.CurrentProductionItem().Product, currentFactory.Ground);
									}
								}*/
								else if (installsStationItem)
								{
									currentFactory.ProductionQueue.Remove(currentFactory.CurrentProductionItem());
								}
								else if (currentFactory.CurrentProductionItem().AOCRepeat)
								{
									var currItem = currentFactory.CurrentProductionItem();
									currItem.Active = false;
									var nextItem = currentFactory.ProductionQueue.FirstOrDefault(T => T != currItem && CheckResourceAvailable(currentPlanet, T.Product, currentFactory.Ground));
									if (nextItem == null && CheckResourceAvailable(currentPlanet, currItem.Product, currentFactory.Ground))
										nextItem = currItem;
									currItem.Production_Complete = 1;
									currItem.Production_Value = currItem.Product.Research.ResearchValue;

									if (nextItem != null)
									{
										RemoveResourceByItem(currentPlanet, nextItem.Product, currentFactory.Ground);
										nextItem.Active = true;
									}
								}
								else
								{
									currentFactory.ProductionQueue.Remove(currentFactory.CurrentProductionItem());
								}
							}
						}
					}
				}
			}
		}

		public static bool CheckResourceAvailable(IPlanet productionPlanet, Item productionItem, bool ground)
		{
			// Research-only technology is not a manufacturing recipe, including in older saves.
			if (productionItem.BuildRequirements == null) return false;
			if (productionItem.OrbitOnly && ground) return false;
			if (IsStationInstallation(productionItem.ItemType) && !CanInstallStationItem(productionPlanet, productionItem.ItemType, ground))
				return false;
			Objects.Store currentStore;

			if (productionPlanet.PlanetId == Enums.StellarBodies.earth && ground)
				currentStore = ((Earth)productionPlanet).PlanetResources.Stores;
			else
				currentStore = productionPlanet.Station.Resources.Stores;

			var prodPossible = true;

			foreach (var material in productionItem.BuildRequirements)
				if (material.ItemCount > currentStore[material.ItemType])
					prodPossible = false;

			return prodPossible;
		}

		public static void RemoveResourceByItem(IPlanet productionPlanet, Item productionItem, bool ground)
		{
			Objects.Store currentStore;

			if (productionPlanet.PlanetId == Enums.StellarBodies.earth && ground)
				currentStore = ((Earth)productionPlanet).PlanetResources.Stores;
			else
				currentStore = productionPlanet.Station.Resources.Stores;

			foreach (var material in productionItem.BuildRequirements)
				currentStore[material.ItemType] -= material.ItemCount;
		}

		public static void AddResourceByItem(IPlanet productionPlanet, Item productionItem, bool ground)
		{
			Objects.Store currentStore;

			if (productionPlanet.PlanetId == Enums.StellarBodies.earth && ground)
				currentStore = ((Earth)productionPlanet).PlanetResources.Stores;
			else
				currentStore = productionPlanet.Station.Resources.Stores;

			foreach (var material in productionItem.BuildRequirements)
				currentStore[material.ItemType] += material.ItemCount;
		}

		#endregion
	}
}
