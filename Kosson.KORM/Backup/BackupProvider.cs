using Kosson.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Kosson.KORM.Backup
{
	/// <summary>
	/// Default provider for creating and restoring database backups.
	/// </summary>
	public class BackupProvider : IBackupProvider
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
