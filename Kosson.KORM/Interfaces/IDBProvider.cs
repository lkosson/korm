using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// KRUD Database provider information.
	/// </summary>
	public interface IDBProvider
	{
		/// <summary>
		/// Name of the database provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Creates a new instance of the provider
		/// </summary>
		/// <returns>A new instance of database provider</returns>
		IDB Create();
	}
}
