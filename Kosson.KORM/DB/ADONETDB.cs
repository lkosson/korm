using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Kosson.KORM.DB
{
	/// <summary>
	/// Abstract IDB implementation based on ADO.NET providers.
	/// </summary>
	public abstract class ADONETDB : IDB
	{
		private readonly Logging log;
		private readonly IDBCommandBuilder commandBuilder;
		private bool isImplicit;
		private readonly IDisposable optionsMonitorDisposer;

		/// <summary>
		/// Synchronization root for accessing ADO.NET objects.
		/// </summary>
		protected SemaphoreSlim syncroot = new SemaphoreSlim(1);

		/// <summary>
		/// Current ADO.NET connection.
		/// </summary>
		protected DbConnection dbconn;

		/// <summary>
		/// Current ADO.NET transaction.
		/// </summary>
		protected DbTransaction dbtran;

		/// <inheritdoc/>
		public virtual int? CommandTimeout { get; set; }

		/// <inheritdoc/>
		public virtual IsolationLevel IsolationLevel { get; set; }

		/// <inheritdoc/>
		public virtual string ConnectionString { get; set; }

		/// <inheritdoc/>
		public virtual IDBCommandBuilder CommandBuilder => commandBuilder;

		/// <inheritdoc/>
		public virtual bool IsTransactionOpen => dbtran != null;

		/// <inheritdoc/>
		public virtual bool IsImplicitTransaction => isImplicit;

		/// <inheritdoc/>
		public virtual bool IsBatchSupported => false;

		/// <summary>
		/// Determines whether ADO.NET provider expects command preparation before execution.
		/// </summary>
		protected virtual bool PrepareCommands => false;

		/*
		 * SupportsCancel removed due to multiple problems:
		 *  - on MSSQL cancelling already completed query (e.g. getLastID during batch insert) causes another roundtrip to database.
		 *  - on SQLite cancelling already completed query (e.g. getLastID during batch insert) causes next execution of the command to return no rows.
		 *  - on PGSQL cancelling a command causes subsequent calls to be also aborted.
		 */

		/// <summary>
		/// Determines whether new line characters in command text should be removed before passing to ADO.NET provider.
		/// </summary>
		protected virtual bool ReplaceNewLines => false;

		/// <summary>
		/// Determines whether CommandTimeout property of ADO.NET objects expects value in seconds instead of milliseconds.
		/// </summary>
		protected virtual bool CommandTimeoutSeconds => false;

		/// <summary>
		/// Creates new ADO.NET DbConnection.
		/// </summary>
		/// <returns>New ADO.NET DbConnection.</returns>
		protected abstract DbConnection CreateConnection();

		private StackTrace connectionStartStackTrace;
		private StackTrace connectionEndStackTrace;
		private StackTrace transactionStartStackTrace;
		private StackTrace transactionEndStackTrace;

		/// <summary>
		/// Creates new instance of ADONETDB.
		/// </summary>
		public ADONETDB(IOptionsMonitor<KORMOptions> optionsMonitor, ILogger logger)
		{
			IsolationLevel = IsolationLevel.Unspecified;
			commandBuilder = CreateCommandBuilder();
			optionsMonitorDisposer = optionsMonitor.OnChange(ApplyOptions);
			ApplyOptions(optionsMonitor.CurrentValue);
			if (logger.IsEnabled(LogLevel.Critical)) log = new Logging(logger);
			else log = new Logging(null);
		}

		private void ApplyOptions(KORMOptions options)
		{
			ConnectionString = options.ConnectionString;
		}

		#region Synchronization
		private SemaphoreSlimReleaser AcquireLock()
		{
			syncroot.Wait();
			return new SemaphoreSlimReleaser(syncroot);
		}

		private async Task<IDisposable> AcquireLockAsync()
		{
			await syncroot.WaitAsync();
			return new SemaphoreSlimReleaser(syncroot);
		}

		class SemaphoreSlimReleaser(SemaphoreSlim sem) : IDisposable
		{
			public void Dispose()
			{
				sem.Release();
			}
		}
		#endregion
		#region Database creation
		void IDB.CreateDatabase()
		{
			try
			{
				CreateDatabaseImpl();
			}
			catch (Exception exc)
			{
				HandleException(exc);
				throw;
			}
		}

		/// <summary>
		/// Creates a database if it doesn't exist already.
		/// </summary>
		protected virtual void CreateDatabaseImpl()
		{
		}
		#endregion
		#region Connection management
		ITransaction IDB.BeginTransaction(IsolationLevel isolationLevel)
		{
			if (IsTransactionOpen)
			{
				if (!IsImplicitTransaction) throw new KORMInvalidOperationException("Transaction already open.");
				if (isolationLevel != IsolationLevel.Unspecified && isolationLevel != IsolationLevel) throw new KORMInvalidOperationException("Implicit transaction already open at isolation level " + IsolationLevel + ".");
				isImplicit = false;
				return new Transaction(this);
			}
			IsolationLevel = isolationLevel;
			Open(false);
			return new Transaction(this);
		}

		/// <summary>
		/// Creates a new DbConnection and DbTransaction if they are not created already.
		/// </summary>
		/// <param name="isImplicit">Determines whether the method is called from CreateCommand to start implicit transaction if no transaction is active at the moment.</param>
		protected void Open(bool isImplicit)
		{
			using (AcquireLock())
			{
				if (dbconn == null)
				{
					dbconn = CreateConnection();
					connectionStartStackTrace = CaptureStackTrace();
				}
				if (dbtran == null)
				{
					dbtran = dbconn.BeginTransaction(IsolationLevel);
					this.isImplicit = isImplicit;
					transactionStartStackTrace = CaptureStackTrace();
				}
			}
		}

		void IDB.Commit()
		{
			using (AcquireLock())
			{
				if (!IsTransactionOpen) throw new KORMInvalidOperationException("Transaction not started.");
				if (IsImplicitTransaction) throw new KORMInvalidOperationException("Implicit transaction cannot be committed.");
				dbtran.Commit();
				dbtran.Dispose();
				dbtran = null;
				transactionEndStackTrace = CaptureStackTrace();
			}
		}

		void IDB.Rollback()
		{
			using (AcquireLock())
			{
				if (!IsTransactionOpen) throw new KORMInvalidOperationException("Transaction not started.");
				if (IsImplicitTransaction) throw new KORMInvalidOperationException("Implicit transaction cannot be rolled back.");
				dbtran.Rollback();
				dbtran.Dispose();
				dbtran = null;
				transactionEndStackTrace = CaptureStackTrace();
			}
		}

		/// <summary>
		/// Closes current DbConnection and DbTransaction if they exist.
		/// </summary>
		protected void Close(bool dontThrow)
		{
			using (AcquireLock())
			{
				try
				{
					if (dbtran != null)
					{
						dbtran.Dispose();
						dbtran = null;
					}
				}
				catch (Exception exc)
				{
					if (dontThrow)
					{
						var translated = TranslateException(exc, null, null);
						log.Log(translated, default);
					}
					else
					{
						HandleException(exc);
						throw;
					}
				}

				try
				{
					if (dbconn != null)
					{
						dbconn.Dispose();
						dbconn = null;
					}
				}
				catch (Exception exc)
				{
					if (dontThrow)
					{
						var translated = TranslateException(exc, null, null);
						log.Log(translated, default);
					}
					else
					{
						HandleException(exc);
						throw;
					}
				}
				transactionEndStackTrace = CaptureStackTrace();
				connectionEndStackTrace = CaptureStackTrace();
			}
		}

		/// <inheritdoc/>
		void IDisposable.Dispose()
		{
			Close(true);
			syncroot.Dispose();
			optionsMonitorDisposer.Dispose();
			GC.SuppressFinalize(this);
		}

		private static StackTrace CaptureStackTrace()
		{
			if (!Debugger.IsAttached) return null;
			return new StackTrace();
		}
		#endregion
		#region Parameters handling
		private static bool IsNull(object val)
		{
			if (val == null) return true;
			if (val is DateTime date && date.Ticks == 0) return true;
			if (val is bool?) return !((bool?)val).HasValue;
			if (val is int?) return !((int?)val).HasValue;
			if (val is long?) return !((long?)val).HasValue;
			if (val is float?) return !((float?)val).HasValue;
			if (val is double?) return !((double?)val).HasValue;
			if (val is decimal?) return !((decimal?)val).HasValue;
			return false;
		}

		/// <summary>
		/// Converts native object to a value understood by ADO.NET provider.
		/// </summary>
		/// <param name="val">Value to convert.</param>
		/// <returns>Converted value understood by ADO.NET provider.</returns>
		protected virtual object NativeToSQL(object val)
		{
			if (IsNull(val)) return DBNull.Value;
			else if (val is Enum) return (int)val;
			else if (val is IHasID id) return id.ID == 0 ? DBNull.Value : id.ID;
			else if (val is bool boolValue) return boolValue ? 1 : 0;
			else return val;
		}

		DbParameter IDB.AddParameter(DbCommand command, string name, object value)
		{
			if (!name.StartsWith(CommandBuilder.ParameterPrefix, StringComparison.Ordinal)) name = CommandBuilder.ParameterPrefix + name;
			DbParameter param = command.CreateParameter();
			param.ParameterName = name;
			((IDB)this).SetParameter(param, value);
			command.Parameters.Add(param);
			return param;
		}

		/// <summary>
		/// Database engine-specific post-process of database command parameter before sending it to ADO.NET provider.
		/// </summary>
		/// <param name="parameter">Database parameter to post-process.</param>
		protected virtual void FixParameter(DbParameter parameter)
		{
		}

		void IDB.SetParameter(DbParameter parameter, object value)
		{
			parameter.Value = NativeToSQL(value);
			FixParameter(parameter);
		}

		void IDB.ClearParameters(DbCommand command)
		{
			command.Parameters.Clear();
		}
		#endregion
		#region Command creation
		/// <inheritdoc/>
		protected virtual IDBCommandBuilder CreateCommandBuilder()
		{
			return new CommandBuilder.DBCommandBuilder();
		}

		DbCommand IDB.CreateCommand(string command)
		{
			DbCommand cmd = null;
			try
			{
				Open(true);
				cmd = dbconn.CreateCommand();
				if (CommandTimeout.HasValue)
				{
					if (CommandTimeoutSeconds)
						cmd.CommandTimeout = CommandTimeout.Value / 1000;
					else
						cmd.CommandTimeout = CommandTimeout.Value;
				}
				cmd.Connection = dbconn;
				cmd.Transaction = dbtran;
				if (ReplaceNewLines) command = command.Replace('\r', ' ').Replace('\n', ' ');
				cmd.CommandText = command;
				if (PrepareCommands) cmd.Prepare();
				return cmd;
			}
			catch (Exception exc)
			{
				HandleException(exc, cmd);
				throw;
			}
		}
		#endregion
		#region Batch support
		DbBatch IDB.CreateBatch()
		{
			try
			{
				Open(true);
				var batch = dbconn.CreateBatch();
				if (CommandTimeout.HasValue) batch.Timeout = CommandTimeout.Value / 1000;
				batch.Transaction = dbtran;
				return batch;
			}
			catch (Exception exc)
			{
				HandleException(exc);
				throw;
			}
		}

		DbBatchCommand IDB.CreateCommand(DbBatch batch, string command)
		{
			try
			{
				Open(true);
				var cmd = batch.CreateBatchCommand();
				if (ReplaceNewLines) command = command.Replace('\r', ' ').Replace('\n', ' ');
				cmd.CommandText = command;
				batch.BatchCommands.Add(cmd);
				return cmd;
			}
			catch (Exception exc)
			{
				HandleException(exc);
				throw;
			}
		}

		DbParameter IDB.AddParameter(DbBatchCommand command, string name, object value)
		{
			if (!name.StartsWith(CommandBuilder.ParameterPrefix, StringComparison.Ordinal)) name = CommandBuilder.ParameterPrefix + name;
			DbParameter param = command.CreateParameter();
			param.ParameterName = name;
			((IDB)this).SetParameter(param, value);
			command.Parameters.Add(param);
			return param;
		}

		int IDB.ExecuteNonQuery(DbBatch batch)
		{
			if (IsImplicitTransaction) throw new KORMInvalidOperationException("ExecuteNonQuery not allowed in implcit transaction.");
			var token = log.Start(batch);
			try
			{
				int result;
				using (AcquireLock())
				{
					result = batch.ExecuteNonQuery();
				}
				log.Stop(token, result);
				return result;
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, token);
				throw;
			}
		}

		async Task<int> IDB.ExecuteNonQueryAsync(DbBatch batch)
		{
			if (IsImplicitTransaction) throw new KORMInvalidOperationException("ExecuteNonQueryAsync not allowed in implcit transaction.");
			var token = log.Start(batch);
			try
			{
				int result;
				using (await AcquireLockAsync())
				{
					result = await batch.ExecuteNonQueryAsync();
				}
				log.Stop(token, result);
				return result;
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, token);
				throw;
			}
		}

		DbDataReader IDB.ExecuteReader(DbBatch batch)
		{
			var token = log.Start(batch);
			try
			{
				using (AcquireLock())
				{
					var reader = batch.ExecuteReader();
					log.Stop(token);
					return reader;
				}
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, token);
				throw;
			}
		}

		async Task<DbDataReader> IDB.ExecuteReaderAsync(DbBatch batch)
		{
			var token = log.Start(batch);
			try
			{
				using (AcquireLock())
				{
					var reader = await batch.ExecuteReaderAsync();
					log.Stop(token);
					return reader;
				}
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, token);
				throw;
			}
		}
		#endregion
		#region Command execution
		int IDB.ExecuteNonQuery(DbCommand command)
		{
			if (IsImplicitTransaction) throw new KORMInvalidOperationException("ExecuteNonQuery not allowed in implcit transaction.");
			var token = log.Start(command);
			try
			{
				int result;
				using (AcquireLock())
				{
					result = command.ExecuteNonQuery();
				}
				log.Stop(token, result);
				return result;
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, command, token);
				throw;
			}
		}

		async Task<int> IDB.ExecuteNonQueryAsync(DbCommand command)
		{
			if (IsImplicitTransaction) throw new KORMInvalidOperationException("ExecuteNonQueryAsync not allowed in implcit transaction.");
			var token = log.Start(command);
			try
			{
				int result;
				using (await AcquireLockAsync())
				{
					result = await command.ExecuteNonQueryAsync();
				}
				log.Stop(token, result);
				return result;
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, command, token);
				throw;
			}
		}

		IReadOnlyList<IRow> IDB.ExecuteQuery(DbCommand command)
		{
			var token = log.Start(command);
			try
			{
				using (AcquireLock())
				{
					using var reader = command.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
					log.StopStart(ref token, "<PROCESS>");
					var rows = ProcessReader(reader);
					log.Stop(token, rows.Count);
					return rows;
				}
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, command, token);
				throw;
			}
		}

		async Task<IReadOnlyList<IRow>> IDB.ExecuteQueryAsync(DbCommand command)
		{
			var token = log.Start(command);
			try
			{
				using (await AcquireLockAsync())
				{
					DbDataReader reader = await command.ExecuteReaderAsync();
					using (reader)
					{
						log.StopStart(ref token, "<PROCESS>");
						var rows = await ProcessReaderAsync(reader);
						log.Stop(token, rows.Count);
						return rows;
					}
				}
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, command, token);
				throw;
			}
		}

		DbDataReader IDB.ExecuteReader(DbCommand command)
		{
			var token = log.Start(command);
			try
			{
				using (AcquireLock())
				{
					var reader = command.ExecuteReader();
					log.Stop(token);
					return reader;
				}
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, command, token);
				throw;
			}
		}

		async Task<DbDataReader> IDB.ExecuteReaderAsync(DbCommand command)
		{
			var token = log.Start(command);
			try
			{
				using (AcquireLock())
				{
					var reader = await command.ExecuteReaderAsync();
					log.Stop(token);
					return reader;
				}
			}
			catch (Exception exc)
			{
				log.Stop(token);
				HandleException(exc, command, token);
				throw;
			}
		}
		#endregion
		#region Reader processing
		private static List<IRow> ProcessReader(DbDataReader reader)
		{
			var rows = new List<IRow>();
			var meta = CreateMetaFromReader(reader);
			while (reader.Read())
			{
				IRow row = CreateRowFromReader(reader, meta);
				rows.Add(row);
			}
			return rows;
		}

		private static async Task<IReadOnlyList<IRow>> ProcessReaderAsync(DbDataReader reader)
		{
			var rows = new List<IRow>();
			var meta = CreateMetaFromReader(reader);
			while (await reader.ReadAsync())
			{
				var row = CreateRowFromReader(reader, meta);
				rows.Add(row);
			}
			return rows;
		}

		private static ArrayBasedRow CreateRowFromReader(DbDataReader reader, Dictionary<string, int> meta)
		{
			var items = new object[reader.FieldCount];
			reader.GetValues(items);
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] is DBNull) items[i] = null;
			}
			return new ArrayBasedRow(items, meta);
		}

		private static Dictionary<string, int> CreateMetaFromReader(DbDataReader reader)
		{
			int count = reader.FieldCount;
			var meta = new Dictionary<string, int>(count * 2 /* is it worth it? */, StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < count; i++)
			{
				string name = reader.GetName(i);
				meta[name] = i;
			}
			return meta;
		}
		#endregion
		#region Exception handling
		/// <summary>
		/// Translates ADO.NET provider-specific exception to KORMException or its subtype.
		/// </summary>
		/// <param name="exc">Caught exception to translate.</param>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		/// <returns>Provider-independent exception.</returns>
		protected virtual KORMException TranslateException(Exception exc, string commandText, DbParameterCollection commandParameters)
		{
			if (exc is System.Data.Common.DbException) return new KORMException(exc.Message, exc, commandText, commandParameters);
			else if (exc is InvalidOperationException) return new KORMException(exc.Message, exc, commandText, commandParameters);
			else if (exc is KORMException) return null; // rethrown in catch block calling HandleException
			return new KORMException(exc.Message, exc, commandText, commandParameters);
		}

		private void HandleException(Exception exc, string commandText, DbParameterCollection commandParameters, TraceToken token = default)
		{
			var translated = TranslateException(exc, commandText, commandParameters);
			if (translated == null) return; // untranslated exceptions will be rethrown by caller
			log.Log(translated, token);
			throw translated;
		}

		internal void HandleException(Exception exc, TraceToken token = default)
		{
			if (exc is DbException dbException && dbException.BatchCommand != null) HandleException(exc, dbException.BatchCommand, token);
			HandleException(exc, null, null, token);
		}

		internal void HandleException(Exception exc, DbCommand command, TraceToken token = default)
		{
			HandleException(exc, command.CommandText, command.Parameters, token);
		}

		internal void HandleException(Exception exc, DbBatchCommand command, TraceToken token = default)
		{
			HandleException(exc, command.CommandText, command.Parameters, token);
		}
		#endregion
	}
}
