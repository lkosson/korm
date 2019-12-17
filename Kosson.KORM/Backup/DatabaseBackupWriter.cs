using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kosson.KORM.Backup
{
	class DatabaseBackupWriter : IBackupWriter
	{
		private BackupRestorer restorer;

		public DatabaseBackupWriter(IServiceProvider serviceProvider, IDB targetDB, IORM targetORM)
		{
			restorer = ActivatorUtilities.CreateInstance<BackupRestorer>(serviceProvider, targetDB, targetORM);
		}

		void IBackupWriter.WriteRecord(IRecord record)
		{
			restorer.ProcessRecord(record);
		}

		void IDisposable.Dispose()
		{
		}
	}
}
