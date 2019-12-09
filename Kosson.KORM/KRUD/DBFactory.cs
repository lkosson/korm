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
		private Dictionary<string, Func<IDB>> providers;

		/// <summary>
		/// Creates a new DBFactory instance.
		/// </summary>
		public DBFactory()
		{
			providers = new Dictionary<string, Func<IDB>>();
			providers["empty"] = () => new EmptyDB();
			providers["firebird"] = () => new FirebirdDB();
			//providers["sqlce"] = () => new SQLCEDB();
			providers["oracle"] = () => new OracleDB();
		}

		void IDBFactory.RegisterProvider(IDBProvider provider)
		{
			providers[provider.Name] = provider.Create;
		}

		IDB IDBFactory.Create(string providerName, bool addToContext)
		{
			return CreateIDB(providerName);
		}

		private IDB CreateIDB(string provider)
		{
			Func<IDB> factory;
			if (providers.TryGetValue(provider, out factory)) return factory();
			throw new InvalidOperationException("KRUD DB provider " + provider + " not found.");
		}
	}
}
