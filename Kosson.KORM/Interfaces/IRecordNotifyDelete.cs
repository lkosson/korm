namespace Kosson.KORM
{
	/// <summary>
	/// Interface for receiving progress notifications and controlling deletion process performed by ORM.
	/// </summary>
	public interface IRecordNotifyDelete
	{
		/// <summary>
		/// Handler called before deleting a record from backing database table.
		/// </summary>
		/// <returns>Determines whether ORM should contine the process and delete record.</returns>
		RecordNotifyResult OnDelete();

		/// <summary>
		/// Handler called after deleting a record from backing database table.
		/// </summary>
		/// <returns>Determines whether ORM should contine the process.</returns>
		RecordNotifyResult OnDeleted();
	}

	/// <summary>
	/// Notification results determining whether ORM command should be continued.
	/// </summary>
	public enum RecordNotifyResult
	{
		/// <summary>
		/// Continues execution of ORM command.
		/// </summary>
		Continue,

		/// <summary>
		/// Skips this record and continues the process.
		/// </summary>
		Skip,

		/// <summary>
		/// Skips this record and stop the process.
		/// </summary>
		Break
	}
}
