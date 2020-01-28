using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Kosson.KORM.DB
{
	/// <summary>
	/// Abstract IDB implementation based on ADO.NET providers.
	/// </summary>
	public abstract class ADONETDB : IDB
	{
		private Logging log;
		private IDBCommandBuilder commandBuilder;
		private bool isImplicit;

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

		/// <summary>
		/// Determines whether ADO.NET provider expects command preparation before execution.
		/// </summary>
		protected virtual bool PrepareCommands => true;

		/// <summary>
		/// Determines whether ADO.NET provider supports command cancelation.
		/// </summary>
		protected virtual bool SupportsCancel => true;

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

		/// <summary>
		/// Creates new instance of ADONETDB.
		/// </summary>
		public ADONETDB(IOptionsMonitor<KORMOptions> optionsMonitor, ILogger logger)
		{
			IsolationLevel = IsolationLevel.Unspecified;
			commandBuilder = CreateCommandBuilder();
			optionsMonitor.OnChange(ApplyOptions);
			ApplyOptions(optionsMonitor.CurrentValue);
			if (logger.IsEnabled(LogLevel.Critical)) log = new Logging(logger);
			else log = new Logging(null);
		}

		private void ApplyOptions(KORMOptions options)
		{
			ConnectionString = options.ConnectionString;
		}

		#region Synchronization
		private IDisposable AcquireLock()
		{
			syncroot.Wait();
			return new SemaphoreSlimReleaser(syncroot);
		}

		private async Task<IDisposable> AcquireLockAsync()
		{
			await syncroot.WaitAsync();
			return new SemaphoreSlimReleaser(syncroot);
		}

		class SemaphoreSlimReleaser : IDisposable
		{
			private SemaphoreSlim sem;

			public SemaphoreSlimReleaser(SemaphoreSlim sem)
			{
				this.sem = sem;
			}

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
				HandleException(exc, null);
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
			if (IsTransactionOpen) throw new KORMInvalidOperationException(IsImplicitTransaction ? "Implicit transaction already open." : "Transaction already open.");
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
				if (dbconn == null) dbconn = CreateConnection();
				if (dbtran == null)
				{
					dbtran = dbconn.BeginTransaction(IsolationLevel);
					this.isImplicit = isImplicit;
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
						log.Log(exc, null, default(TraceToken));
					}
					else
					{
						HandleException(exc, null);
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
						log.Log(exc, null, default(TraceToken));
					}
					else
					{
						HandleException(exc, null);
						throw;
					}

				}
			}
		}

		/// <inheritdoc/>
		void IDisposable.Dispose()
		{
			Close(true);
			syncroot.Dispose();
		}
		#endregion
		#region Parameters handling
		private bool IsNull(object val)
		{
			if (val == null) return true;
			if (val is DateTime && ((DateTime)val).Ticks == 0) return true;
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
			else if (val is IHasID)
			{
				var id = (val as IHasID).ID;
				if (id == 0) return DBNull.Value;
				return id;
			}
			else if (val is bool)
				if ((bool)val)
					return 1;
				else
					return 0;
			else return val;
		}

		DbParameter IDB.AddParameter(DbCommand command, string name, object value)
		{
			if (!name.StartsWith(CommandBuilder.ParameterPrefix)) name = CommandBuilder.ParameterPrefix + name;
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
		#region Command execution
		int IDB.ExecuteNonQuery(DbCommand command)
		{
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

		IReadOnlyList<IRow> IDB.ExecuteQuery(DbCommand command, int limit)
		{
			var token = log.Start(command);
			try
			{
				using (AcquireLock())
				{
					using (DbDataReader reader = command.ExecuteReader())
					{
						log.StopStart(ref token, "<PROCESS>");
						var rows = ProcessReader(reader, limit);
						if (limit != -1 && rows.Count == limit && SupportsCancel) command.Cancel();
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

		async Task<IReadOnlyList<IRow>> IDB.ExecuteQueryAsync(DbCommand command, int limit)
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
						var rows = await ProcessReaderAsync(reader, limit);
						if (limit != -1 && rows.Count == limit && SupportsCancel) command.Cancel();
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
		private IReadOnlyList<IRow> ProcessReader(DbDataReader reader, int limit)
		{
			List<IRow> rows = limit == -1 ? new List<IRow>() : new List<IRow>(limit);
			Dictionary<string, int> meta = CreateMetaFromReader(reader);
			while (limit != 0 && reader.Read())
			{
				IRow row = CreateRowFromReader(reader, meta);
				rows.Add(row);
				limit--;
			}
			return rows;
		}

		private async Task<IReadOnlyList<IRow>> ProcessReaderAsync(DbDataReader reader, int limit)
		{
			List<IRow> rows = limit == -1 ? new List<IRow>() : new List<IRow>(limit);
			Dictionary<string, int> meta = CreateMetaFromReader(reader);
			while (limit != 0 && await reader.ReadAsync())
			{
				IRow row = CreateRowFromReader(reader, meta);
				rows.Add(row);
				limit--;
			}
			return rows;
		}

		private IRow CreateRowFromReader(DbDataReader reader, Dictionary<string, int> meta)
		{
			object[] items = new object[reader.FieldCount];
			reader.GetValues(items);
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] is DBNull) items[i] = null;
			}
			return new ArrayBasedRow(items, meta);
		}

		private Dictionary<string, int> CreateMetaFromReader(DbDataReader reader)
		{
			int count = reader.FieldCount;
			Dictionary<string, int> meta = new Dictionary<string, int>(count * 2 /* is it worth it? */, StringComparer.OrdinalIgnoreCase);
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
		/// <param name="cmd">Command causing the exception.</param>
		/// <returns>Provider-independent exception.</returns>
		protected virtual Exception TranslateException(Exception exc, DbCommand cmd)
		{
			if (exc is System.Data.Common.DbException) return new KORMException(exc.Message, exc, cmd);
			else if (exc is InvalidOperationException) return new KORMException(exc.Message, exc, cmd);
			else if (exc is KORMException) return null; // rethrown in catch block calling HandleException
			return new KORMException(exc.Message, exc, cmd);
		}

		private void HandleException(Exception exc, DbCommand cmd, TraceToken token = default(TraceToken))
		{
			var translated = TranslateException(exc, cmd);
			if (translated == null) return; // untranslated exceptions will be rethrown by caller
			log.Log(translated, cmd, token);
			throw translated;
		}
		#endregion
	}
}
