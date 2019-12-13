using Kosson.Interfaces;
using Kosson.KRUD.ORM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kosson.KRUD
{
	public class DatabaseScripting
	{
		private IServiceProvider serviceProvider;
		private IBackupProvider backupProvider;

		public DatabaseScripting(IServiceProvider serviceProvider, IBackupProvider backupProvider)
		{
			this.serviceProvider = serviceProvider;
			this.backupProvider = backupProvider;
		}

		public void WriteScript(Stream stream, IEnumerable<Type> tables)
		{
			using (var writer = ActivatorUtilities.CreateInstance<SQLScriptBackupWriter>(serviceProvider, stream))
			{
				var backupset = backupProvider.CreateBackupSet(writer);
				foreach (var table in tables) backupset.AddTable(table);
			}
		}
	}
}
