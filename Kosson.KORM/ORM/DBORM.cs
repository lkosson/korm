using Kosson.Interfaces;
using Kosson.Kore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD.ORM
{
	/// <summary>
	/// Object-relational mapper using IDB for database communication.
	/// </summary>
	public class DBORM : IORM
	{
		private IDB db;

		/// <summary>
		/// Creates a new Object-relational mapper using provided IDB for database communication.
		/// </summary>
		/// <param name="db">Database provider to use. Current context's default database provider is used when this parameter is null</param>
		public DBORM(IDB db = null)
		{
			this.db = db;
		}

		void IORM.CreateTables(IEnumerable<Type> types)
		{
			new DBTableCreator(db ?? KORMContext.Current.DB).Create(types);
		}

		IORMSelect<TRecord> IORM.Select<TRecord>()
		{
			return new DBQuerySelect<TRecord>(db ?? KORMContext.Current.DB);
		}

		IORMInsert<TRecord> IORM.Insert<TRecord>()
		{
			return new DBORMInsert<TRecord>(db ?? KORMContext.Current.DB);
		}

		IORMUpdate<TRecord> IORM.Update<TRecord>()
		{
			return new DBORMUpdate<TRecord>(db ?? KORMContext.Current.DB);
		}

		IORMDelete<TRecord> IORM.Delete<TRecord>()
		{
			return new DBORMDelete<TRecord>(db ?? KORMContext.Current.DB);
		}

		TDelegate IORM.Execute<TDelegate>()
		{
			return ExecuteProxyBuilder<TDelegate>.Get();
		}
	}
}
