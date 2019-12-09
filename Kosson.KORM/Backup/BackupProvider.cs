using Kosson.Interfaces;
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
		private IORM orm;

		public BackupProvider(IORM orm)
		{
			this.orm = orm;
		}

		IBackupSet IBackupProvider.CreateBackupSet(IBackupWriter writer)
		{
			return new BackupSet(writer);
		}

		void IBackupProvider.Restore(IBackupReader reader)
		{
			new BackupRestorer(reader, orm).Restore();
		}

		void IBackupProvider.ClearTables(IEnumerable<Type> types)
		{
			new BackupClearer(orm, types).Clear();
		}
	}
}
