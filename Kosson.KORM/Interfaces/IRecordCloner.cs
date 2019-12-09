using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	public interface IRecordCloner
	{
		object Clone(object source);
	}
}
