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
		public const string ResearchSpriteBasePath = "res://Sprites//Items//Research//";
		public const string ProductionProgressSpriteBasePath = "res://Sprites//Items//Production//";
		public List<ProductionButton> Buttons { get; set; }
		public ProductionButton SelectedButton { get; set; }
		Label ProductionNameLabel { get; set; }
		Label StaffNameLabel { get; set; }
		Label StaffRankLabel { get; set; }
		Label StaffCountLabel { get; set; }
		TextureRect SmallItemImageTextureRect { get; set; }
		TextureRect ItemProgressImageTextureRect { get; set; }

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

		private void RemoveStaff_Pressed()
		{
			if (CurrentFactory.Builder != null && CurrentFactory.Builder.Count > 0 && (Ground ? CurrentPlanet.PlanetResources.Staff : CurrentPlanet.Station.Resources.Staff).Any(T => T == null))
			{
				((Resource)(Ground ? CurrentPlanet.PlanetResources : CurrentPlanet.Station.Resources)).AddStaff(CurrentFactory.Builder);
				CurrentFactory.Builder = null;

				foreach (var item in CurrentFactory.ProductionQueue.Where(T => T.Active))
					AddResourceByItem(CurrentPlanet, item.Product, Ground);

				CurrentFactory.ClearQueue();

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

			if (!CurrentFactory.AOC)
				SelectedButton.Selected = true;

			CheckProductionStart();

			DrawData();
			clickedButton.Redraw(false);
		}

		private void CheckProductionStart()
		{
			{
				//There is no staff
				if (!CurrentFactory.AOC && (CurrentFactory.Builder == null || CurrentFactory.Builder.Count == 0))
					return;

				if (!CurrentFactory.AOC)
				{
					if (SelectedButton != null)
					{
						var addedItem = (Item)SelectedButton.ObjectData;

						
						if (addedItem.OrbitOnly && CurrentFactory.Ground)
						{
							GameCore.ShowError(this, "This Item Can\nOnly BE Made\nIn Orbit.");
							return;
						}

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
							if (CheckResourceAvailable(CurrentPlanet, addedItem, Ground))
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
			GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Production && !T.Locked
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
				SelectedButton = null;
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
				SmallItemImageTextureRect = SpriteManager.LoadImageToTextureRect(ResearchSpriteBasePath + currentProductionItem.Product.ItemType.ToString() + ".png", SmallItemImageTextureRect);
				ItemProgressImageTextureRect = SpriteManager.LoadImageToTextureRect(ProductionProgressSpriteBasePath + currentProductionItem.Product.ItemType.ToString() + "_" + currentProductionItem.Production_Complete + ".png", ItemProgressImageTextureRect);
			}
			else
			{
				ProductionNameLabel.Text = "";
				SmallItemImageTextureRect.Texture = null;
				ItemProgressImageTextureRect = SpriteManager.LoadImageToTextureRect(ProductionProgressSpriteBasePath + "idle.png", ItemProgressImageTextureRect);
			}

			if (CurrentFactory.AOC)
			{
				foreach(ProductionButton b in Buttons)
				{
					b.Redraw(false);
				}
			}

		}

		#region Statics

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

								currentPlanet.AddItems(currentFactory.CurrentProductionItem().Product.ItemType, 1);

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
								else if (currentFactory.CurrentProductionItem().AOCRepeat)
								{
									var nextItem = currentFactory.ProductionQueue.FirstOrDefault(T => T != currentFactory.CurrentProductionItem() && (CheckResourceAvailable(currentPlanet, T.Product, currentFactory.Ground)));
									var currItem = currentFactory.CurrentProductionItem();
									currItem.Production_Complete = 1;
									currItem.Production_Value = currItem.Product.Research.ResearchValue;

									if (nextItem != null)
									{
										currItem.Active = false;
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

						foreach (var autoProduced in GameCore.SingletonInstance.GameData.GetAllActiveItems().Where(T => T.AutoProduce))
						{
							if (CheckResourceAvailable(currentPlanet, autoProduced, currentFactory.Ground))
							{
								if (autoProduced.AutoProduceFlip)
								{
									autoProduced.AutoProduceFlip = false;
								}
								else
								{
									autoProduced.AutoProduceFlip = true;
									RemoveResourceByItem(currentPlanet, autoProduced, currentFactory.Ground);
									if (currentFactory.Ground)
										currentPlanet.PlanetResources.Stores[autoProduced.ItemType] += 3;
									else
										currentPlanet.Station.Resources.Stores[autoProduced.ItemType] += 3;
								}
							}
						}
					}
				}
			}
		}

		public static bool CheckResourceAvailable(IPlanet productionPlanet, Item productionItem, bool ground)
		{
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
