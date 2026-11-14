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

        // Original $38A42 is indexed by systems still containing hostile stations.
        static readonly uint[] BuildFrequencies = { 0, 700, 1000, 950, 900, 900, 800, 700, 700, 800 };


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
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            if (save.AtWar)
            {
                if (save.Clock.DateCentidays >= save.EnemyBuildDay)
                {
                    save.AlienTransmissions.SampleEnemySystems(save);
                    var buildfrequency = BuildFrequencies[save.AlienTransmissions.EnemySystems];
                    save.EnemyBuildDay = save.Clock.DateCentidays + buildfrequency;

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
