using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Deuteros.Code.Objects.ModuleTextFrame
{
	public class Line
	{
		public string Text { get; set; }
		public Godot.Color TextColor { get; set; }
		public bool Pause { get; set; }
		public int Length { get; set; }
		public bool DelayEachWord { get; set; }

		public Line(string text, Color textColor, bool pause, bool delayEachWord = false, int length = 750)
		{
			Text = text;
			TextColor = textColor;
			DelayEachWord = delayEachWord;
			Pause = pause;
			Length = length;
		}

		public string GetText(List<string> dynamicValues)
		{
			if (TextColor != Colors.White)
				return $"[color=#{TextColor.ToHtml(false)}]" + string.Format(Text, dynamicValues.ToArray()) + "[/color]" + "\r\n";
			else
				return string.Format(Text, dynamicValues.ToArray()) + "\r\n";
		}
	}
}