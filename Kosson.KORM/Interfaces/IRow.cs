namespace Kosson.Interfaces
{
	/// <summary>
	/// Key-value pairs addressed by integer, zero-based column index.
	/// </summary>
	public interface IIndexBasedRow
	{
		/// <summary>
		/// Retrieves value for a given index.
		/// </summary>
		/// <param name="index">Index of value to retrieve.</param>
		/// <returns>Row value for a given column index.</returns>
		object this[int index] { get; }

		/// <summary>
		/// Number of columns of the row.
		/// </summary>
		int Length { get; }
	}

	/// <summary>
	/// Key-value pairs addressed by string-based column name or by integer, zero-based column index.
	/// </summary>
	public interface IRow : IIndexBasedRow
	{
		/// <summary>
		/// Retrieves value for a given column name.
		/// </summary>
		/// <param name="name">Name of column to retrieve.</param>
		/// <returns>Row value for a given column.</returns>
		object this[string name] { get; }

		/// <summary>
		/// Retrieves index for a given column name.
		/// </summary>
		/// <param name="name">Column name to retrieve index for.</param>
		/// <returns>Column index for a given column name. Negative value for nonexisting name.</returns>
		int GetIndex(string name);

		/// <summary>
		/// Retrieves column name for a given column index.
		/// </summary>
		/// <param name="index">Column index to retrieve name for.</param>
		/// <returns>Column name for a given index.</returns>
		string GetName(int index);
	}
}
