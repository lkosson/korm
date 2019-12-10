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
		private IPropertyBinder propertyBinder;
		private IConverter converter;

		public BackupProvider(IORM orm, IMetaBuilder metaBuilder, IPropertyBinder propertyBinder, IConverter converter)
		{
			this.orm = orm;
			this.metaBuilder = metaBuilder;
			this.propertyBinder = propertyBinder;
			this.converter = converter;
		}

		IBackupSet IBackupProvider.CreateBackupSet(IBackupWriter writer)
		{
			return new BackupSet(orm, writer, metaBuilder, propertyBinder, converter);
		}

		void IBackupProvider.Restore(IBackupReader reader)
		{
			new BackupRestorer(reader, orm, propertyBinder).Restore();
		}

		void IBackupProvider.ClearTables(IEnumerable<Type> types)
		{
			new BackupClearer(orm, metaBuilder, types).Clear();
		}
	}
}
