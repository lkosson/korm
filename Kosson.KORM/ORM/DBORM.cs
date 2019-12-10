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
		private IConverter converter;
		private IRecordLoader recordLoader;
		private IFactory factory;
		private static ReaderRecordLoaderCache cache;

		/// <summary>
		/// Creates a new Object-relational mapper using provided IDB for database communication.
		/// </summary>
		public DBORM(IDB db, IMetaBuilder metaBuilder, IConverter converter, IRecordLoader recordLoader, IFactory factory)
		{
			if (cache == null) cache = new ReaderRecordLoaderCache(metaBuilder);
			this.db = db;
			this.metaBuilder = metaBuilder;
			this.converter = converter;
			this.recordLoader = recordLoader;
			this.factory = factory;
		}

		void IORM.CreateTables(IEnumerable<Type> types) => new DBTableCreator(db, metaBuilder).Create(types);
		IORMSelect<TRecord> IORM.Select<TRecord>() => new DBQuerySelect<TRecord>(db, metaBuilder, converter, recordLoader, factory, cache.GetLoader<TRecord>());
		IORMInsert<TRecord> IORM.Insert<TRecord>() => new DBORMInsert<TRecord>(db, metaBuilder, converter);
		IORMUpdate<TRecord> IORM.Update<TRecord>() => new DBORMUpdate<TRecord>(db, metaBuilder);
		IORMDelete<TRecord> IORM.Delete<TRecord>() => new DBORMDelete<TRecord>(db, metaBuilder);
	}
}
