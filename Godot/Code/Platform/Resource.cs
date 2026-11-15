using Deuteros.Code.Objects;
using Deuteros.Code.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Platform
{
	public class Resource
	{
		public Store Stores { get; set; }

		public Staff[] Staff { get; set; }

		public Resource()
		{
			Staff = new Staff[4];
			Stores = new Store();
		}

		public void AddStaff(Staff staff)
		{
			int firstIndex = -1;

			for (int i = 0; i < Staff.Length; i++)
			{
				if (Staff[i] == null)
				{
					firstIndex = i;
					break;
				}
			}

			Staff[firstIndex] = staff;
		}

		public Staff SwapStaff(Staff existingStaff, Staff replacementStaff)
		{
			for (int i = 0; i < Staff.Length; i++)
			{
				if (ReferenceEquals(Staff[i], existingStaff))
				{
					Staff[i] = replacementStaff;

					return existingStaff;
				}
			}

			throw new InvalidOperationException("Staff to replace was not found in array.");
		}
		public void RemoveAllStaff()
		{
			for (int i = 0; i < Staff.Length; i++)
			{
				Staff[i] = null;
			}
		}

		public void RemoveStaff(Staff staff)
		{
			for (int i = 0; i < Staff.Length; i++)
			{
				if (ReferenceEquals(Staff[i], staff))
				{
					Staff[i] = null;
					break;
				}
			}
		}

	}
}
