using Kosson.Interfaces;
using Kosson.KRUD.ORM;
using Microsoft.Extensions.DependencyInjection;
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
		private BackupRestorer restorer;

		/// <summary>
		/// Creates a new backup writer storing records in a specified database.
		/// </summary>
		public DatabaseBackupWriter(IServiceProvider serviceProvider, IDB targetDB, IORM targetORM)
		{
			restorer = ActivatorUtilities.CreateInstance<BackupRestorer>(serviceProvider, targetDB, targetORM);
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
