using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kosson.KORM
{
	/// <summary>
	/// Enumerator of result of a reader-based database command.
	/// </summary>
	/// <typeparam name="TRecord">Type of record returned by the query.</typeparam>
	public interface IORMReader<TRecord> : IEnumerable<TRecord>, IDisposable
	{
		/// <summary>
		/// Constructs a record from current row of a reader.
		/// </summary>
		/// <returns>Record filled from current reader position.</returns>
		TRecord Read();

		/// <summary>
		/// Advances reader to a next row in result.
		/// </summary>
		/// <returns>True if advancing succeeded.</returns>
		bool MoveNext();

		/// <summary>
		/// Asynchronous version of MoveNext.
		/// Advances reader to a next row in result.
		/// </summary>
		/// <returns>True if advancing succeeded.</returns>
		Task<bool> MoveNextAsync();
	}
}
