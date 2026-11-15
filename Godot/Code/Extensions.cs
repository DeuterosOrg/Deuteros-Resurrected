using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Extensions
{
	public static string ToScreenString(this Enum value, string spaceChar = "")
	{
		var raw = value.ToString();

		var sb = new StringBuilder();
		int i = 0;
		bool capitalizeNext = true;

		while (i < raw.Length)
		{
			if (i + 1 < raw.Length && raw[i] == '_' && raw[i + 1] == '_')
			{
				sb.Append(". ");
				capitalizeNext = true;
				i += 2;
			}
			else if (raw[i] == '_')
			{
				sb.Append(spaceChar);
				capitalizeNext = true;
				i += 1;
			}
			else
			{
				char c = raw[i];

				if (capitalizeNext)
				{
					sb.Append(char.ToUpperInvariant(c));
					capitalizeNext = false;
				}
				else
				{
					sb.Append(char.ToLowerInvariant(c));
				}

				i++;
			}
		}

		return sb.ToString();
	}
}