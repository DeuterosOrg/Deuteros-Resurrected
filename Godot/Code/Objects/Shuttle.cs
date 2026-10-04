using Deuteros.Code.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
    public class Shuttle : Ship, IShip
    {
        public bool OnGround { get; set; }
        
        public Shuttle()
        {
            ShipID = Guid.NewGuid();
            Name = "Shuttle Craft";
        }

        public override int TravelTimeRemain()
        {
            if (ShipState == Enums.Ship_States.Landing)
                return 2 - (int)(GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay - StartTravelDay);
            else if (ShipState == Enums.Ship_States.TakingOff)
                return 5 - (int)(GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay - StartTravelDay);
            else
                return 0;
        }

        public void CompleteRepairs()
        {
            if ( ShipState == Enums.Ship_States.CrewRepairing)
            {
                if (GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay - StartRepairDay >= 2)
                {
                    Modules[0].ItemStored = ItemTypes.none;
                    Modules[0].ItemCount = 0;
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[PlanetLocation].BaseDamaged = false;
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[PlanetLocation].CaptiveBase = false;

                    ShipState = Enums.Ship_States.Docked;
                }
            }

        }
    }
}
