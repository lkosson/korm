namespace Kosson.KORM
{
	/// <summary>
	/// Interface for restoring database backup.
	/// </summary>
	public interface IBackupRestorer
	{
		/// <summary>
		/// Restores current database using provided reader as a backup source.
		/// </summary>
		/// <param name="reader">Backup to restore data from.</param>
		void Restore(IBackupReader reader);
	}
}
