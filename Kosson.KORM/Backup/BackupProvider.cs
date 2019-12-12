using Kosson.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD
{
	/// <summary>
	/// Default provider for creating and restoring database backups.
	/// </summary>
	public class BackupProvider : IBackupProvider
	{
		private IServiceProvider serviceProvider;
		private IBackupRestorer backupRestorer;

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

		void IBackupProvider.ClearTables(IEnumerable<Type> types)
		{
			var clearer = ActivatorUtilities.CreateInstance<BackupClearer>(serviceProvider);
			clearer.Clear(types);
		}
	}
}
