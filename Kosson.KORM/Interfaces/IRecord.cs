namespace Kosson.KORM
{
	/// <summary>
	/// Database record with 64-bit integer as a primary key.
	/// </summary>
	public interface IRecord : IHasID
	{
		/// <summary>
		/// Primary key of the record.
		/// </summary>
		new long ID { get; set; }
	}
}
