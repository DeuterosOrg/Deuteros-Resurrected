using Deuteros.Code.Objects;
using Deuteros.Code.Objects.GameData;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Screens.ModuleScenes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Platform
{
    public class EnemyDroneBuilder
    {

        //build frequencies based on star system (in reverse for some reason)
        static uint[] BuildFrequencies = { 0x2bc, 0x3e8, 0x3b6, 0x384, 0x384, 0x320, 0x2bc, 0x2bc, 0x320 };


        private static void ProcessEnemyFleets()
        {
            foreach(Star star in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars.Values)
            {
                EnemyFleet enemyFleet = (EnemyFleet)GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.FirstOrDefault(s => s.ShipType != Enums.Ship_Types.Shuttle && ((InterStellarShip)s).MethanoidOwned && ((InterStellarShip)s).StarLocation == star.StarId);

                if (enemyFleet != null)
                {
                    enemyFleet.ProcessFleet();
                }

            }

            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            save.EnemyStarCursor = (save.EnemyStarCursor + 1) % save.BaseGameData.Stars.Count;

            Star star2 = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars.Values.ToList()[save.EnemyStarCursor];
            EnemyFleet enemyFleet2 = (EnemyFleet)GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.FirstOrDefault(s => s.ShipType != Enums.Ship_Types.Shuttle && ((InterStellarShip)s).MethanoidOwned && ((InterStellarShip)s).StarLocation == star2.StarId);

            if (enemyFleet2 != null)
            {
                enemyFleet2.ProcessAttackTrigger();
            }
        }

        public static void BuildDrones(uint previousDay, uint currentDay)
        {
            if (GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar)
            {
                if (currentDay == GameCore.SingletonInstance.GameData.ActiveSaveFile.EnemyBuildDay || GameCore.SingletonInstance.GameData.ActiveSaveFile.EnemyBuildDay == 0)
                {
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.AlienTransmissions?.SampleEnemySystems(GameCore.SingletonInstance.GameData.ActiveSaveFile);
                    var buildfrequency = BuildFrequencies[GameCore.SingletonInstance.GameData.ActiveSaveFile.StarSystemsCaptured];
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.EnemyBuildDay = currentDay + buildfrequency / 100;

                    foreach (var star in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars.Keys)
                    {
                        var productionDroneCount =
                                   GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Count(p => p.ActiveMethanoid && p.ParentStar == star) * 2;

                        foreach (var p in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Where(p => p.ActiveMethanoid && p.ParentStar == star))
                        {
                            if (p.Station.Resources.Stores[Enums.ItemTypes.ios_drone] < 200)
                            {
                                p.Station.Resources.Stores[Enums.ItemTypes.ios_drone] += 2;
                                productionDroneCount -= 2;
                            }
                        }

                        if (productionDroneCount == 0)
                        {
                            productionDroneCount = 1;
                        }
                        else
                        {
                            productionDroneCount = 2;
                        }

                        InterStellarShip enemyFleet = (InterStellarShip)GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.FirstOrDefault(s => s.ShipType != Enums.Ship_Types.Shuttle && ((InterStellarShip)s).MethanoidOwned && ((InterStellarShip)s).StarLocation == star);

                        if (enemyFleet != null)
                        {
                            enemyFleet.DroneCount += productionDroneCount;
                            if (enemyFleet.DroneCount > 200) enemyFleet.DroneCount = 200;
                            
                        }
                    }
                }
                ProcessEnemyFleets();
            }

        }

    }
}
