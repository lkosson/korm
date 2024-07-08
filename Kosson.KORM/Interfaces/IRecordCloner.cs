namespace Kosson.KORM
{
	/// <summary>
	/// Interface for creating deep copies of a record.
	/// </summary>
	public interface IRecordCloner
	{
		/// <summary>
		/// Creates a deep copy of provided record.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">Source record for copying.</param>
		/// <returns>Copied record.</returns>
		T? Clone<T>(T? source) where T : class, new();
	}
}
