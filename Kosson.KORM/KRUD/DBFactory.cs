using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Kosson.KRUD
{
	/// <inheritdoc/>
	public class DBFactory : IDBFactory
	{
		private Dictionary<string, Func<string, IDB>> providers;

		/// <summary>
		/// Creates a new DBFactory instance.
		/// </summary>
		public DBFactory()
		{
			providers = new Dictionary<string, Func<string, IDB>>();
			providers["empty"] = connectionString => new EmptyDB();
		}

		void IDBFactory.RegisterProvider(IDBProvider provider)
		{
			providers[provider.Name] = provider.Create;
		}

		IDB IDBFactory.Create(string providerName, string connectionString)
		{
			Func<string, IDB> factory;
			if (providers.TryGetValue(providerName, out factory)) return factory(connectionString);
			throw new InvalidOperationException("KRUD DB provider " + providerName + " not found.");
		}
	}
}
