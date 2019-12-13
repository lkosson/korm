using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Interface for copying data between databases.
	/// </summary>
	public interface IDatabaseCopier
	{
		/// <summary>
		/// Copies records from provided tables in current database to target database.
		/// </summary>
		/// <typeparam name="TDB">Target database.</typeparam>
		/// <param name="targetConfiguration">Connection parameters to target database.</param>
		/// <param name="tables">Tables to copy records from.</param>
		void CopyTo<TDB>(KORMConfiguration targetConfiguration, IEnumerable<Type> tables)
			where TDB : IDB;
	}
}
