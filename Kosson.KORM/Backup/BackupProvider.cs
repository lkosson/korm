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
		private IMetaBuilder metaBuilder;

		public BackupProvider(IORM orm, IMetaBuilder metaBuilder)
		{
			this.orm = orm;
			this.metaBuilder = metaBuilder;
		}

		IBackupSet IBackupProvider.CreateBackupSet(IBackupWriter writer)
		{
			return new BackupSet(writer, metaBuilder);
		}

		void IBackupProvider.Restore(IBackupReader reader)
		{
			new BackupRestorer(reader, orm).Restore();
		}

		void IBackupProvider.ClearTables(IEnumerable<Type> types)
		{
			new BackupClearer(orm, metaBuilder, types).Clear();
		}
	}
}
