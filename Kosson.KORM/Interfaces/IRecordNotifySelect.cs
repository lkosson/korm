namespace Kosson.Interfaces
{
	/// <summary>
	/// Interface for receiving progress notifications and controlling record retrieval performed by ORM.
	/// </summary>
	public interface IRecordNotifySelect
	{
		/// <summary>
		/// Handler called before creating a record from given database row.
		/// </summary>
		/// <param name="row">Retrieved database row.</param>
		/// <returns>Determines whether ORM should contine the process and create a record.</returns>
		RecordNotifyResult OnSelect(IRow row);

		/// <summary>
		/// Handler called after creating a record from given database row.
		/// </summary>
		/// <param name="row">Retrieved database row.</param>
		/// <returns>Determines whether ORM should continue the process and include the record in command result.</returns>
		RecordNotifyResult OnSelected(IRow row);
	}
}
