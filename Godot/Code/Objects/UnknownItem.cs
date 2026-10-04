using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
    public class UnknownItem : GrappleItem
    {
        public Enums.GrappleItemTypes GrappleItemType { get; set; }
        public Enums.UnknownItemTypes ItemType { get; set; }

        public UnknownItem(Enums.UnknownItemTypes itemtype)
        {
            GrappleItemType = Enums.GrappleItemTypes.UnknownItem;
            ItemType = itemtype;

        }

        public static UnknownItem ScanForItems(InterStellarShip ship)
        {
            if (GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars.Values.FirstOrDefault(s => s.ArtifactLocation == ship.PlanetLocation)!=null)
            {
                return new UnknownItem(Enums.UnknownItemTypes.AlienArtifact);
            }

            return null;
        }
    }
}
