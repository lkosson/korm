using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Kosson.KORM.ORM
{
	/// <summary>
	/// Object-relational mapper using IDB for database communication.
	/// </summary>
	class DBORM : IORM
	{
		private readonly IDB db;
		private readonly IMetaBuilder metaBuilder;
		private readonly IConverter converter;
		private readonly IFactory factory;
		private readonly ILogger logger;
		private static ReaderRecordLoaderCache cache;

		/// <summary>
		/// Creates a new Object-relational mapper using provided IDB for database communication.
		/// </summary>
		public DBORM(IDB db, IMetaBuilder metaBuilder, IConverter converter, IFactory factory, ILogger<Kosson.KORM.ORM.DBORM> logger)
		{
			if (cache == null) cache = new ReaderRecordLoaderCache(metaBuilder);
			this.db = db;
			this.metaBuilder = metaBuilder;
			this.converter = converter;
			this.factory = factory;
			this.logger = logger;
		}

		void IORM.CreateTables(IEnumerable<Type> types) => new DBTableCreator(db, metaBuilder, logger).Create(types);
		IORMSelect<TRecord> IORM.Select<TRecord>() => new DBQuerySelect<TRecord>(db, metaBuilder, converter, factory, cache.GetLoader<TRecord>(), logger);
		IORMInsert<TRecord> IORM.Insert<TRecord>() => new DBORMInsert<TRecord>(db, metaBuilder, converter, logger);
		IORMUpdate<TRecord> IORM.Update<TRecord>() => new DBORMUpdate<TRecord>(db, metaBuilder, logger);
		IORMDelete<TRecord> IORM.Delete<TRecord>() => new DBORMDelete<TRecord>(db, metaBuilder, logger);
	}
}
