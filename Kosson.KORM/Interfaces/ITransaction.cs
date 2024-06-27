using System;

namespace Kosson.KORM
{
	/// <summary>
	/// A database transaction.
	/// </summary>
	public interface ITransaction : IDisposable
	{
		/// <summary>
		/// Commits this database transaction.
		/// </summary>
		void Commit();

		/// <summary>
		/// Rolls back any changes made in this transaction.
		/// </summary>
		void Rollback();

		/// <summary>
		/// Determines whether this transaction is still active (i.e. it hasn't been committed or rolled back).
		/// </summary>
		bool IsOpen { get; }
	}
}
