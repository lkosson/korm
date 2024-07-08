using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Kosson.KORM
{
	/// <summary>
	/// Provider for creation, manipulation and execution of database commands and transactions for a given database engine.
	/// </summary>
	public interface IDB : IDisposable
	{
		/// <summary>
		/// Connection string constructed from provider configuration used to access the database.
		/// </summary>
		string ConnectionString { get; set; }

		/// <summary>
		/// Retrieves command builder for creating commands of a given database dialect.
		/// </summary>
		IDBCommandBuilder CommandBuilder { get; }

		/// <summary>
		/// Gets or sets command timeout in milliseconds.
		/// </summary>
		int? CommandTimeout { get; set; }

		/// <summary>
		/// Gets or sets transaction isolation level used for database transactions.
		/// </summary>
		IsolationLevel IsolationLevel { get; set; }

		/// <summary>
		/// Determines whether there is a database transaction currently opened within this component.
		/// </summary>
		bool IsTransactionOpen { get; }

		/// <summary>
		/// Determines whether current transaction has been opened implicitly by CreateCommand method.
		/// </summary>
		bool IsImplicitTransaction { get; }

		/// <summary>
		/// Determines whether the database supports command batching via DbBatch.
		/// </summary>
		bool IsBatchSupported { get; }

		/// <summary>
		/// Checks whether database specified in provider configuration exists and creates it if it does not.
		/// </summary>
		void CreateDatabase();

		/// <summary>
		/// Explicitly opens new database transaction. Fails if transaction is already open.
		/// </summary>
		/// <param name="isolationLevel">Isolation level to use for transaction.</param>
		ITransaction BeginTransaction(IsolationLevel isolationLevel = System.Data.IsolationLevel.Unspecified);

		/// <summary>
		/// Commits current database transaction. Fails if transaction has been opened in different context, but requires owning context to also perform commit.
		/// </summary>
		void Commit();

		/// <summary>
		/// Rolls back current database transaction. Fails if transaction has been opened in different context, but requires owning context to also perform rollback.
		/// </summary>
		void Rollback();

		/// <summary>
		/// Adds new database parameter to a given database command. Provided value is converted to a type recognized by database engine.
		/// </summary>
		/// <param name="command">Database command to add parameter to.</param>
		/// <param name="name">Name of the parameter to add.</param>
		/// <param name="value">Value of the parameter to add.</param>
		/// <returns>Object representing command parameter.</returns>
		DbParameter AddParameter(DbCommand command, string name, object? value);

		/// <summary>
		/// Changes value of a given database parameter to a new one. Provided value is converted to a type recognized by database engine.
		/// </summary>
		/// <param name="parameter">Parameter to change.</param>
		/// <param name="value">New value of the parameter.</param>
		void SetParameter(DbParameter parameter, object? value);

		/// <summary>
		/// Removes all parameters from the database command.
		/// </summary>
		/// <param name="command">Database command to remove parameters from.</param>
		void ClearParameters(DbCommand command);

		/// <summary>
		/// Creates new database command with a given text.
		/// </summary>
		/// <param name="command">Database command text.</param>
		/// <returns>New database command for a given text.</returns>
		DbCommand CreateCommand(string command);

		/// <summary>
		/// Creates a new database command batch. Available only when IsBatchSupported = true.
		/// </summary>
		/// <returns>New database command batch.</returns>
		DbBatch CreateBatch();

		/// <summary>
		/// Creates new database command with a given text associated with provided command batch.
		/// </summary>
		/// <param name="batch">Batch do add a command to.</param>
		/// <param name="command">Database command text.</param>
		/// <returns>New database command for a given text.</returns>
		DbBatchCommand CreateCommand(DbBatch batch, string command);

		/// <summary>
		/// Adds new database parameter to a given database command. Provided value is converted to a type recognized by database engine.
		/// </summary>
		/// <param name="command">Database command to add parameter to.</param>
		/// <param name="name">Name of the parameter to add.</param>
		/// <param name="value">Value of the parameter to add.</param>
		/// <returns>Object representing command parameter.</returns>
		DbParameter AddParameter(DbBatchCommand command, string name, object value);

		/// <summary>
		/// Executes given database non-query command batch, handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="batch">Database command batch to execute.</param>
		/// <returns>Database command result code or a number of affected rows.</returns>
		int ExecuteNonQuery(DbBatch batch);

		/// <summary>
		/// Asynchronous version of ExecuteNonQuery.
		/// Executes given database non-query command batch, handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="batch">Database command batch to execute.</param>
		/// <returns>A task representing asynchronous operation returning database command result code or a number of affected rows.</returns>
		Task<int> ExecuteNonQueryAsync(DbBatch batch);

		/// <summary>
		/// Executes given database command batch, returning a reader providing one-time, on-the-fly enumerator of rows based on the command result and handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="batch">Database command batch to execute.</param>
		/// <returns>One-time reader of rows representing result of the command batch.</returns>
		DbDataReader ExecuteReader(DbBatch batch);

		/// <summary>
		/// Asynchronous version of ExecuteReader.
		/// Executes given database command batch, returning a reader providing one-time, on-the-fly enumerator of rows based on the command result and handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="batch">Database command batch to execute.</param>
		/// <returns>A task representing the asynchronous operation returning one-time reader of rows representing result of the command batch.</returns>
		Task<DbDataReader> ExecuteReaderAsync(DbBatch batch);

		/// <summary>
		/// Executes given database non-query command, handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="command">Database command to execute.</param>
		/// <returns>Database command result code or a number of affected rows.</returns>
		int ExecuteNonQuery(DbCommand command);

		/// <summary>
		/// Asynchronous version of ExecuteNonQuery.
		/// Executes given database non-query command, handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="command">Database command to execute.</param>
		/// <returns>A task representing asynchronous operation returning database command result code or a number of affected rows.</returns>
		Task<int> ExecuteNonQueryAsync(DbCommand command);

		/// <summary>
		/// Executes given database query, returning non-null array of rows based on the command result and handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="command">Database command to execute.</param>
		/// <returns>Array (possibly empty) of rows representing result of the command.</returns>
		IReadOnlyList<IRow> ExecuteQuery(DbCommand command);

		/// <summary>
		/// Asynchronous version of ExecuteQuery.
		/// Executes given database query, returning non-null array of rows based on the command result and handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="command">Database command to execute.</param>
		/// <returns>A task representing the asynchronous operation returning an Array (possibly empty) of rows representing result of the command.</returns>
		Task<IReadOnlyList<IRow>> ExecuteQueryAsync(DbCommand command);

		/// <summary>
		/// Executes given database query, returning a reader providing one-time, on-the-fly enumerator of rows based on the command result and handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="command">Database command to execute.</param>
		/// <returns>One-time reader of rows representing result of the command.</returns>
		DbDataReader ExecuteReader(DbCommand command);

		/// <summary>
		/// Asynchronous version of ExecuteReader.
		/// Executes given database query, returning a reader providing one-time, on-the-fly enumerator of rows based on the command result and handling and wrapping potential database engine-specific exceptions.
		/// </summary>
		/// <param name="command">Database command to execute.</param>
		/// <returns>A task representing the asynchronous operation returning one-time reader of rows representing result of the command.</returns>
		Task<DbDataReader> ExecuteReaderAsync(DbCommand command);
	}
}
