namespace Kosson.Interfaces
{
	/// <summary>
	/// Interface for receiving progress notifications and controlling update process performed by ORM.
	/// </summary>
	public interface IRecordNotifyUpdate
	{
		/// <summary>
		/// Handler called before updating a record in backing database table.
		/// </summary>
		/// <returns>Determines whether ORM should contine the process and update record.</returns>
		RecordNotifyResult OnUpdate();

		/// <summary>
		/// Handler called after updating a record in backing database table.
		/// </summary>
		/// <returns>Determines whether ORM should contine the process.</returns>
		RecordNotifyResult OnUpdated();
	}
}
