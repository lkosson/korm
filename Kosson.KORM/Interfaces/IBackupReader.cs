using System;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Interface for deserializing records stored in backup.
	/// </summary>
	public interface IBackupReader : IDisposable
	{
		/// <summary>
		/// Reads next record from the backups.
		/// </summary>
		/// <returns>Record from the backup or null if there are no more records.</returns>
		IRecord ReadRecord();
	}
}
