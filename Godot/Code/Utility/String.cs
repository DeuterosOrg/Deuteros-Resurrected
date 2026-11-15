using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Deuteros.Code.Utility
{
	public partial class String
	{
		private const int XFontCharWidth = 28;

		//Calculates how much left padding to apply to a string to right align it with a max size
		public static int PadX(int stringLength, int maxStringLength)
		{
			return (maxStringLength-stringLength) * XFontCharWidth;
		}

	}
}