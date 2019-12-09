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
	/// Extension methods for Kosson.KRUD.IBackupProvider.
	/// </summary>
	public static class BackupExtensions
	{
		/// <summary>
		/// Creates a new XML-based backup containing all data from provided tables.
		/// </summary>
		/// <param name="provider">Backup provider to use.</param>
		/// <param name="file">Name of XML file to create.</param>
		/// <param name="tables">Tables to include in backup. Tables referenced by foreign keys are also included.</param>
		public static void ToXML(this IBackupProvider provider, IMetaBuilder metaBuilder, IPropertyBinder propertyBinder, IConverter converter, string file, params Type[] tables)
		{
			using (var fs = new FileStream(file, FileMode.Create))
			using (var xml = new XMLBackupWriter(metaBuilder, propertyBinder, converter, fs))
			{
				var backupset = provider.CreateBackupSet(xml);
				foreach (var table in tables) backupset.AddTable(table);
			}
		}

		/// <summary>
		/// Restores records from a XML-based backup to database.
		/// </summary>
		/// <param name="provider">Backup provider to use.</param>
		/// <param name="file">Name of XML file containing the backup.</param>
		public static void FromXML(this IBackupProvider provider, string file)
		{
			using (var fs = new FileStream(file, FileMode.Open))
			using (var xml = new XMLBackupReader(fs))
			{
				provider.Restore(xml);
			}
		}
	}
}
