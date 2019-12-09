using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Abstract factory of IDB components. Created concrete implementation is selected based on current configuration.
	/// </summary>
	public interface IDBFactory
	{
		/// <summary>
		/// Registers provider for given database type.
		/// </summary>
		/// <param name="provider">Database provider to register.</param>
		void RegisterProvider(IDBProvider provider);

		/// <summary>
		/// Creates a new provider for a given database type. Providers already existing in context are not used. 
		/// New provider instance is added to current context with unique key.
		/// </summary>
		/// <param name="providerName">Provider name.</param>
		/// <param name="addToContext">Determines whether created provider should be added to current context with unique key.</param>
		/// <returns>New provider instance.</returns>
		IDB Create(string providerName, bool addToContext = true);
	}
}
