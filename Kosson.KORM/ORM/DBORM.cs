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
		private readonly ILogger operationLogger;
		private readonly ILogger recordLogger;
		private static ReaderRecordLoaderCache cache = default!;

		/// <summary>
		/// Creates a new Object-relational mapper using provided IDB for database communication.
		/// </summary>
		public DBORM(IDB db, IMetaBuilder metaBuilder, IConverter converter, IFactory factory, ILoggerFactory loggerFactory)
		{
			cache ??= new ReaderRecordLoaderCache(metaBuilder);
			this.db = db;
			this.metaBuilder = metaBuilder;
			this.converter = converter;
			this.factory = factory;
			this.operationLogger = loggerFactory.CreateLogger("Kosson.KORM.ORM");
			this.recordLogger = loggerFactory.CreateLogger("Kosson.KORM.ORM.Records");
		}

		void IORM.CreateTables(IEnumerable<Type> types) => new DBTableCreator(db, metaBuilder, operationLogger).Create(types);
		IORMSelect<TRecord> IORM.Select<TRecord>() => new DBORMSelect<TRecord>(db, metaBuilder, converter, factory, cache.GetLoader<TRecord>(), operationLogger, recordLogger);
		IORMInsert<TRecord> IORM.Insert<TRecord>() => new DBORMInsert<TRecord>(db, metaBuilder, converter, operationLogger, recordLogger);
		IORMUpdate<TRecord> IORM.Update<TRecord>() => new DBORMUpdate<TRecord>(db, metaBuilder, operationLogger, recordLogger);
		IORMDelete<TRecord> IORM.Delete<TRecord>() => new DBORMDelete<TRecord>(db, metaBuilder, operationLogger, recordLogger);
	}
}
