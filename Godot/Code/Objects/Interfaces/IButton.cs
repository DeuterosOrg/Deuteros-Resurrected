using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects.Interfaces
{
	public interface IButton
	{
		void Redraw(bool dayPassed);
		void MouseClickedMe();
	}
}