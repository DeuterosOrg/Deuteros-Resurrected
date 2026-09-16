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

		public News() 
		{ 
			NewsItems = new List<string>();
			LastBulletin = Enums.BulletinTypes.None;
		}

		public void AddNews(string NewsItem)
		{
			NewsItems.Add(GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay.ToString().PadRight(3, ' ') + ": " + NewsItem);
		}

		public List<string> GetNews(int count)
		{
			return NewsItems.TakeLast(count).ToList();
		}
	}
}
