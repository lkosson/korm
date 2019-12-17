using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kosson.KORM.Backup
{
	class BackupProvider : IBackupProvider
	{
		private readonly IServiceProvider serviceProvider;
		private readonly IBackupRestorer backupRestorer;

		public BackupProvider(IServiceProvider serviceProvider, IBackupRestorer backupRestorer)
		{
			this.serviceProvider = serviceProvider;
			this.backupRestorer = backupRestorer;
		}

		IBackupSet IBackupProvider.CreateBackupSet(IBackupWriter writer)
		{
			var backupset = ActivatorUtilities.CreateInstance<BackupSet>(serviceProvider, writer);
			return backupset;
		}

		void IBackupProvider.Restore(IBackupReader reader)
		{
			backupRestorer.Restore(reader);
		}
	}
}
