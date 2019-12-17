using Kosson.KORM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kosson.KORM.Backup
{
	class DatabaseScriptGenerator : IDatabaseScriptGenerator
	{
		private IServiceProvider serviceProvider;
		private IBackupProvider backupProvider;

		public DatabaseScriptGenerator(IServiceProvider serviceProvider, IBackupProvider backupProvider)
		{
			this.serviceProvider = serviceProvider;
			this.backupProvider = backupProvider;
		}

		void IDatabaseScriptGenerator.GenerateScript(Stream stream, IEnumerable<Type> tables)
		{
			using (var writer = ActivatorUtilities.CreateInstance<SQLScriptBackupWriter>(serviceProvider, stream))
			{
				var backupset = backupProvider.CreateBackupSet(writer);
				foreach (var table in tables) backupset.AddTable(table);
			}
		}
	}
}
