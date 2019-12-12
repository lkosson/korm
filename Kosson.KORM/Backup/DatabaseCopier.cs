using Kosson.Interfaces;
using Kosson.KRUD.ORM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KRUD
{
	public class DatabaseCopier
	{
		private IServiceProvider serviceProvider;
		private IMetaBuilder metaBuilder;
		private IBackupProvider backupProvider;

		public DatabaseCopier(IServiceProvider serviceProvider, IMetaBuilder metaBuilder, IBackupProvider backupProvider)
		{
			this.serviceProvider = serviceProvider;
			this.metaBuilder = metaBuilder;
			this.backupProvider = backupProvider;
		}

		public void CopyTo<TDB>(KORMConfiguration targetConfiguration, IEnumerable<Type> tables)
			where TDB : IDB
		{
			using (var targetDB = ActivatorUtilities.CreateInstance<TDB>(serviceProvider, targetConfiguration))
			{
				var targetORM = ActivatorUtilities.CreateInstance<DBORM>(serviceProvider, targetDB);

				targetDB.CreateDatabase();
				targetDB.BeginTransaction();
				new DBTableCreator(targetDB, metaBuilder).Create(tables);
				new BackupClearer(targetORM, metaBuilder).Clear(tables);

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
