using Deuteros.Code.Platform.Base;
using Deuteros.Code.Objects;
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Objects.Interfaces;
using System.Resources;
using System.Text;

namespace Deuteros.Code.Platform.Screens
{
	public partial class News : BaseSubScene
	{
		public List<Label> NewsLabels { get; set; }
		public TextureButton ReplayIcon { get; set; }

		public override void _Ready()
		{
			NewsLabels = new List<Label>();

			for (int i = 0; i < 12; i++)
			{
                var label = GetNode<Label>("NewsLines/" + i.ToString().PadLeft(2, '0'));
                label.ClipText = true;
                label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
                label.Size = new Vector2(240, 8);
                label.MouseFilter = Control.MouseFilterEnum.Pass;
                NewsLabels.Add(label);
			}

			ReplayIcon = GetNode<TextureButton>("Images/Icon");

			ReplayIcon.Pressed += ReplayIcon_Pressed;

			DrawData();

			base._Ready();
		}

		private void ReplayIcon_Pressed()
		{
			var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
			if (save.News.LastBulletin is Enums.BulletinTypes.Transmission1 or Enums.BulletinTypes.Transmission2
				&& save.AlienTransmissions?.LastStage is >= 0)
				GameCore.SingletonInstance.ShowAlienTransmission();
			else if (save.News.LastBulletin != Enums.BulletinTypes.None)
				GameCore.SingletonInstance.ShowBulletin(save.News.LastBulletin, replay: true);
		}

		protected override void DayTick(uint previousDay, uint currentDay)
		{
			DrawData();
		}

		public void DrawData()
		{
			ReplayIcon.Disabled = GameCore.SingletonInstance.GameData.ActiveSaveFile.News.LastBulletin == Enums.BulletinTypes.None;
			var rowCount = 0;

			foreach (var item in GameCore.SingletonInstance.GameData.ActiveSaveFile.News.GetNews(12).AsEnumerable().Reverse())
			{
				NewsLabels[rowCount].Text = item;
                NewsLabels[rowCount].TooltipText = item;

				rowCount++;
			}

			while (rowCount < 12)
			{
				NewsLabels[rowCount].Text = "";
                NewsLabels[rowCount].TooltipText = "";

				rowCount++;
			}
		}
	}
}
