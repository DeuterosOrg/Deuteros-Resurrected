using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
	public class News
	{
		private List<string> NewsItems { get; set; }
		public BulletinTypes LastBulletin { get; set; }
		public List<BulletinTypes> PendingBulletins { get; set; } = new List<BulletinTypes>();

		public News() 
		{ 
			NewsItems = new List<string>();
			LastBulletin = Enums.BulletinTypes.None;
		}

		public void AddNews(string NewsItem)
		{
			NewsItems.Add(GameCore.SingletonInstance.GameData.ActiveSaveFile.Clock.RelativeDate.PadRight(3, ' ') + ": " + NewsItem);
		}

        public void AddCrewLoss(IEnumerable<Staff> teams)
        {
            foreach (var team in teams.Where(t => t != null).Distinct())
                AddNews(team.GetLevelString() + " " + team.Leader + " Killed.");
        }

        public void AddShipLoss(Interfaces.IShip ship, string cause = "Destroyed.")
        {
            if (ship is EnemyFleet) return;
            AddCrewLoss((ship.Modules?.Where(m => m.ModuleType == Enums.Module_Types.Cryo)
                .Select(m => m.StaffStored) ?? Enumerable.Empty<Staff>()).Prepend(ship.Pilot));
            AddNews(ship.Name + " " + cause);
        }

		public List<string> GetNews(int count)
		{
			return NewsItems.TakeLast(count).ToList();
		}
	}
}
