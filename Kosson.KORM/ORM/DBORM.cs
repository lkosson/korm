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
		private IMetaBuilder metaBuilder;

		/// <summary>
		/// Creates a new Object-relational mapper using provided IDB for database communication.
		/// </summary>
		public DBORM(IDB db, IMetaBuilder metaBuilder)
		{
			this.db = db;
			this.metaBuilder = metaBuilder;
		}

		void IORM.CreateTables(IEnumerable<Type> types) => new DBTableCreator(db, metaBuilder).Create(types);
		IORMSelect<TRecord> IORM.Select<TRecord>() => new DBQuerySelect<TRecord>(db, metaBuilder);
		IORMInsert<TRecord> IORM.Insert<TRecord>() => new DBORMInsert<TRecord>(db, metaBuilder);
		IORMUpdate<TRecord> IORM.Update<TRecord>() => new DBORMUpdate<TRecord>(db, metaBuilder);
		IORMDelete<TRecord> IORM.Delete<TRecord>() => new DBORMDelete<TRecord>(db, metaBuilder);
		TDelegate IORM.Execute<TDelegate>() => ExecuteProxyBuilder<TDelegate>.Get();
	}
}
