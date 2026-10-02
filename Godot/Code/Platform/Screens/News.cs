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
				NewsLabels.Add(GetNode<Label>("NewsLines/" + i.ToString().PadLeft(2, '0')));
			}

			ReplayIcon = GetNode<TextureButton>("Images/Icon");

			ReplayIcon.Pressed += ReplayIcon_Pressed;

			DrawData();

			base._Ready();
		}

		private void ReplayIcon_Pressed()
		{
			if (GameCore.SingletonInstance.GameData.ActiveSaveFile.News.LastBulletin != Enums.BulletinTypes.None)
				GameCore.SingletonInstance.ShowBulletin(GameCore.SingletonInstance.GameData.ActiveSaveFile.News.LastBulletin);
		}

		protected override void DayTick(uint previousDay, uint currentDay)
		{
			DrawData();
		}

		public void DrawData()
		{
			ReplayIcon.Disabled = GameCore.SingletonInstance.GameData.ActiveSaveFile.News.LastBulletin == Enums.BulletinTypes.None;
			var rowCount = 0;

			foreach (var item in GameCore.SingletonInstance.GameData.ActiveSaveFile.News.GetNews(12))
			{
				NewsLabels[11 - rowCount].Text = item;

				rowCount++;
			}

			while (rowCount < 12)
			{
				NewsLabels[11 - rowCount].Text = "";

				rowCount++;
			}
		}
	}
}
