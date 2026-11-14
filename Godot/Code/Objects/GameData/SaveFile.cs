using Deuteros.Code.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects.GameData
{
    [Serializable]
    public class SaveFile
    {
        public BaseData BaseGameData { get; set; }
        public Config GameConfig { get; set; }
        // Historical save name: consumed simulation updates, not the displayed fractional date.
        public uint CurrentDay { get; set; }
        public GameClock Clock { get; set; }
        public int RefiningPhase { get; set; }
        public double SdmTimerRemainder { get; set; }
        public int NextPersonIndex { get; set; }
        public bool TimeSkip { get; set; }
        public ulong TimeSkipStart { get; set; }
        public bool TimeSkipDay { get; set; }
		public News News { get; set; }
		public List<IShip> Ships { get; set; }
        public Enums.StellarBodies CurrentPlanet { get; set; }
        public List<Enums.Game_Unlocks> Unlocks { get; set; }
        public bool AtWar { get; set; }
        public uint WarDeclaredDay { get; set; }
        public int MethanoidTradeCount { get; set; }
        public int StarSystemsCaptured { get; set; }
        public AlienTransmissions AlienTransmissions { get; set; }
        public RogueCrew RogueCrew { get; set; } = new();
        // Historical save name: next enemy batch in displayed centidays.
        public ulong EnemyBuildDay { get; set; }
        public int EnemyStarCursor { get; set; }
        public int IOSCount { get { return Ships.Count(T => T.ShipType == Enums.Ship_Types.IOS); } }
        public int SCGCount { get { return Ships.Count(T => T.ShipType == Enums.Ship_Types.SCG); } }
    }
}
