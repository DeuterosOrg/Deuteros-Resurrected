using Deuteros.Code.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Platform
{
	internal class Unlocker
	{
		public Unlocker()
		{
			GameCore.SingletonInstance.ProductionFinished += SingletonInstance_ProductionFinished;
			GameCore.SingletonInstance.ResearchFinished += SingletonInstance_ResearchFinished;
			GameCore.SingletonInstance.ShipCreated += SingletonInstance_ShipCreated;
			GameCore.SingletonInstance.StationPiecePlaced += SingletonInstance_StationPiecePlaced;
			GameCore.SingletonInstance.AlienTechDiscovery += SingletonInstance_AlienTechDiscovery;
		}

		private void SingletonInstance_AlienTechDiscovery(Enums.ItemTypes techType)
		{
			switch (techType)
			{
                case Enums.ItemTypes.alien_artifact:
					var researchitem = GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.alien_artifact).Research;
                    researchitem.Locked = false;
                    researchitem.ResearchLimit += 11;
                    break;

                case Enums.ItemTypes.commspod:
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.CommsPod);
                    GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.commspod).Research.Locked = false;
					break;

                case Enums.ItemTypes.pulse_blaster_laser:
                    GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.pulse_blaster_laser).Research.Locked = false;
                    break;

                case Enums.ItemTypes.m__t__x:
					GameCore.SingletonInstance.ShowBulletin(Enums.BulletinTypes.Matter_Transmitter);
					GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.Mass_Tranceiver);
					GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.m__t__x).Research.Locked = false;
					break;

				case Enums.ItemTypes.s__d__m:
					GameCore.SingletonInstance.ShowBulletin(Enums.BulletinTypes.Self_Destruct);
					GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.Self_Destruct);
					GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.s__d__m).Research.Locked = false;
					break;
			}
		}

		public void DayTick(uint previousDay, uint currentDay)
		{
			if (
				GameCore.SingletonInstance.GameData.ActiveSaveFile.MethanoidTradeCount < 17 &&
				GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar &&
				currentDay == GameCore.SingletonInstance.GameData.ActiveSaveFile.WarDeclaredDay + 1 &&
				!GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.D_F_C_C))
			{
				GameCore.SingletonInstance.ShowBulletin(Enums.BulletinTypes.Drone_Ships);
				GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.D_F_C_C);
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.d__f__c__c).Research.Locked = false;
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.ios_drone).Research.Locked = false;
			}
		}


		private void SingletonInstance_StationPiecePlaced(Enums.StellarBodies stellarBody)
		{
			if (stellarBody == Enums.StellarBodies.earth && GameCore.SingletonInstance.GetCurrentPlanet().Station.BuildParts == 1)
			{
				GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.First_Station_Segment);

				GameCore.SingletonInstance.ShowBulletin(Enums.BulletinTypes.IOS);

				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.i_chassis).Research.Locked = false;
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.i_drive).Research.Locked = false;
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.a__c__c).Research.Locked = false;
			}
			else if (stellarBody == Enums.StellarBodies.earth && GameCore.SingletonInstance.GetCurrentPlanet().Station.Built)
			{
				GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.Space_Stations);
			}
		}

		private void SingletonInstance_ShipCreated(Objects.Interfaces.IShip ship)
		{
			if (ship.ShipType == Enums.Ship_Types.Shuttle && !GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Shuttle_Unlock))
			{
				GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.Shuttle_Unlock);

				GameCore.SingletonInstance.TriggerUnlockAdded(Enums.Game_Unlocks.Shuttle_Unlock);
			}
		}

		private void SingletonInstance_ResearchFinished(Objects.ResearchItem researchItem)
		{
			if (researchItem.ItemType == Enums.ItemTypes.pulse_blaster_laser)
			{
				GameCore.SingletonInstance.ShowBulletin(Enums.BulletinTypes.Methanoid_Laser);
				GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.D_F_C_C);
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.d__f__c__c).Research.Locked = false;
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.ios_drone).Research.Locked = false;
			}

        }

        private void SingletonInstance_ProductionFinished(Objects.Factory factory)
		{
			if (factory.CurrentProductionItem().Product.ItemType == Enums.ItemTypes.i_chassis && !GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.IOS_Attachments))
			{
				GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.IOS_Attachments);

				GameCore.SingletonInstance.ShowBulletin(Enums.BulletinTypes.IOS_Attachments);

				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.a__m__a).Research.Locked = false;
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.a__o__c).Research.Locked = false;
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.bandaid).Research.Locked = false;
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.grapple).Research.Locked = false;
				GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.r_frame).Research.Locked = false;
			}
		}

	}
}
