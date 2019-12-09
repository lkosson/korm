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
		private BackupRestorer restorer;

		/// <summary>
		/// Creates a new backup writer storing records in a specified database.
		/// </summary>
		/// <param name="targetDB">Database provider to use for storing records.</param>
		public DatabaseBackupWriter(IDB targetDB)
		{
			this.targetDB = targetDB;
			targetORM = new ORM.DBORM(targetDB);
			restorer = new BackupRestorer(null, targetORM);
		}

		/// <summary>
		/// Writes all record of specified types from default database to a specific database using provided connectionstring.
		/// Records referenced by foreign keys are included.
		/// </summary>
		/// <param name="provider">Database provider for destination database.</param>
		/// <param name="connectionString">Connectionstring to use for destination database.</param>
		/// <param name="tables">Tables to include in the process.</param>
		public static void Run(string provider, string connectionString, IEnumerable<Type> tables)
		{
			var db = KORMContext.Current.DBFactory.Create(provider);
			db.ConnectionString = connectionString;
			db.CreateDatabase();
			db.BeginTransaction();
			new DBTableCreator(db).Create(tables);
			using (var dw = new DatabaseBackupWriter(db))
			{
				new BackupClearer(dw.targetORM, tables).Clear();
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
