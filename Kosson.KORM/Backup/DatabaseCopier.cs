using Kosson.Interfaces;
using Kosson.KORM.ORM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Kosson.KORM.Backup
{
	class DatabaseCopier : IDatabaseCopier
	{
		private readonly IServiceProvider serviceProvider;
		private readonly IMetaBuilder metaBuilder;

		public DatabaseCopier(IServiceProvider serviceProvider, IMetaBuilder metaBuilder)
		{
			this.serviceProvider = serviceProvider;
			this.metaBuilder = metaBuilder;
		}

		void IDatabaseCopier.CopyTo<TDB>(KORMConfiguration targetConfiguration, IEnumerable<Type> tables)
		{
			using (var targetDB = ActivatorUtilities.CreateInstance<TDB>(serviceProvider, targetConfiguration))
			{
				var targetORM = ActivatorUtilities.CreateInstance<DBORM>(serviceProvider, targetDB);
				IDatabaseEraser targetEraser = new DatabaseEraser(targetORM, metaBuilder);

				targetDB.CreateDatabase();
				targetDB.BeginTransaction();
				new DBTableCreator(targetDB, metaBuilder).Create(tables);
				targetEraser.Clear(tables);

				using (var writer = ActivatorUtilities.CreateInstance<DatabaseBackupWriter>(serviceProvider, targetDB, targetORM))
				{
					var backupset = ActivatorUtilities.CreateInstance<BackupSet>(serviceProvider, writer);
					foreach (var table in tables) backupset.AddTable(table);
					targetDB.Commit();
				}
			}
		}
	}
}
