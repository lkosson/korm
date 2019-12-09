using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Interface for creating and restoring database backups.
	/// </summary>
	public interface IBackupProvider
	{
		/// <summary>
		/// Creates a new backup set using provided record serializer.
		/// </summary>
		/// <param name="writer">Record serializer to use for writing records.</param>
		/// <returns>A new, empty backup set.</returns>
		IBackupSet CreateBackupSet(IBackupWriter writer);

		/// <summary>
		/// Restores all serialized records from a backup to database, preserving all existing records.
		/// </summary>
		/// <param name="reader">Record deserializer to read records from.</param>
		void Restore(IBackupReader reader);

		/// <summary>
		/// Clears all data from tables backing provided record types.
		/// </summary>
		/// <param name="types">Record types to clear.</param>
		void ClearTables(IEnumerable<Type> types);
	}
}
