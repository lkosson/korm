using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Interface for serializing database records in a backup.
	/// </summary>
	public interface IBackupWriter : IDisposable
	{
		/// <summary>
		/// Writes a record to the backup.
		/// </summary>
		/// <param name="record">Record to add to the backup.</param>
		void WriteRecord(IRecord record);
	}
}
