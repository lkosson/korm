using System;
using System.Collections.Generic;

namespace Kosson.KORM
{
	/// <summary>
	/// Backup set during creation.
	/// </summary>
	public interface IBackupSet : IDisposable
	{
		/// <summary>
		/// Adds all records from a given table to the backup. Records referenced by foreign keys
		/// are also included.
		/// </summary>
		/// <param name="type">Type representing a table to backup.</param>
		void AddTable(Type type);

		/// <summary>
		/// Adds provided records to the backup. Records referenced by foreign keys are also included.
		/// </summary>
		/// <typeparam name="T">Type of records to add to the backup.</typeparam>
		/// <param name="records">Records to add to the backup -or- null to add all records from the table.</param>
		void AddRecords<T>(IEnumerable<T>? records = null)
			where T : class, IRecord, new();
	}
}
