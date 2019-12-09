using Kosson.Interfaces;
using Kosson.KRUD.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD
{
	/// <summary>
	/// Backup writer storing records in a specified database.
	/// </summary>
	public class DatabaseBackupWriter : IBackupWriter
	{
		private IDB targetDB;
		private IORM targetORM;
		private IMetaBuilder metaBuilder;
		private BackupRestorer restorer;

		/// <summary>
		/// Creates a new backup writer storing records in a specified database.
		/// </summary>
		/// <param name="targetDB">Database provider to use for storing records.</param>
		public DatabaseBackupWriter(IDB targetDB, IMetaBuilder metaBuilder, IPropertyBinder propertyBinder, IConverter converter, IRecordLoader recordLoader, IFactory factory)
		{
			this.targetDB = targetDB;
			this.metaBuilder = metaBuilder;
			targetORM = new DBORM(targetDB, metaBuilder, converter, recordLoader, factory);
			restorer = new BackupRestorer(null, targetORM, propertyBinder);
		}

		/// <summary>
		/// Writes all record of specified types from default database to a specific database using provided connectionstring.
		/// Records referenced by foreign keys are included.
		/// </summary>
		/// <param name="provider">Database provider for destination database.</param>
		/// <param name="connectionString">Connectionstring to use for destination database.</param>
		/// <param name="tables">Tables to include in the process.</param>
		public static void Run(IMetaBuilder metaBuilder, IPropertyBinder propertyBinder, IConverter converter, IRecordLoader recordLoader, IFactory factory, string provider, string connectionString, IEnumerable<Type> tables)
		{
			var db = KORMContext.Current.DBFactory.Create(provider, connectionString);
			db.CreateDatabase();
			db.BeginTransaction();
			new DBTableCreator(db, metaBuilder).Create(tables);
			using (var dw = new DatabaseBackupWriter(db, metaBuilder, propertyBinder, converter, recordLoader, factory))
			{
				new BackupClearer(dw.targetORM, metaBuilder, tables).Clear();
				var bs = KORMContext.Current.BackupProvider.CreateBackupSet(dw);
				foreach (var table in tables) bs.AddTable(table);
			}
			db.Commit();
		}

		/// <inheritdoc/>
		public void WriteRecord(IRecord record)
		{
			restorer.ProcessRecord(record);
		}

		void IDisposable.Dispose()
		{
		}
	}
}
