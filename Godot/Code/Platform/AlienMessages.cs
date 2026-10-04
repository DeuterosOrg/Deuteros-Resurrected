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
    public class AlienMessages
    {
        public static void ProcessAlienMessages(uint previousDay, uint currentDay)
        {
            if (GameCore.SingletonInstance.GameData.ActiveSaveFile.NextAlienMessageDay!=0 && currentDay >= GameCore.SingletonInstance.GameData.ActiveSaveFile.NextAlienMessageDay)
            {
                GameCore.SingletonInstance.ShowAlienMessage();
            }
        }
    }
}
