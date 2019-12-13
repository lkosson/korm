namespace Kosson.Interfaces
{
	/// <summary>
	/// Interface for receiving progress notifications and controlling insertion process performed by ORM.
	/// </summary>
	public interface IRecordNotifyInsert
	{
		/// <summary>
		/// Handler called before inserting a record to backing database table.
		/// </summary>
		/// <returns>Determines whether ORM should contine the process and insert record.</returns>
		RecordNotifyResult OnInsert();

		/// <summary>
		/// Handler called after inserting a record to backing database table.
		/// </summary>
		/// <returns>Determines whether ORM should contine the process.</returns>
		RecordNotifyResult OnInserted();
	}
}
