using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects.ModuleTextFrame
{
	public class FrameContainer
	{
		private List<TextFrame> TextFrames { get; set; }

		public FrameContainer()
		{
			TextFrames = new List<TextFrame>();
		}

		public void Add(TextFrame textFrame)
		{
			TextFrames.Add(textFrame);
		}

		public TextFrame this[Enums.ModuleFrameText index]
		{
			get => TextFrames.Single(T => T.FrameTextName == index);
		}
	}
}