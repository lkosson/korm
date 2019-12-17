using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM
{
	/// <summary>
	/// Interface for bulk record erasing.
	/// </summary>
	public interface IDatabaseEraser
	{
		/// <summary>
		/// Removes all records from provided tables.
		/// </summary>
		/// <param name="types"></param>
		void Clear(IEnumerable<Type> types);
	}
}
