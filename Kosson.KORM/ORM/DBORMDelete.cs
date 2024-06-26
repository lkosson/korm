using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMDelete<TRecord> : DBORMCommandBase<TRecord, IDBDelete>, IORMDelete<TRecord> where TRecord : IRecord
	{
		protected override bool UseFullFieldNames { get { return false; } }
		private string commandText;
		private static string cachedCommandText;
		private static Type cachedCommandTextDBType;

		public DBORMDelete(IDB db, IMetaBuilder metaBuilder, ILogger operationLogger, ILogger recordLogger)
			: base(db, metaBuilder, operationLogger, recordLogger)
		{
			var dbType = db.GetType();
			if (cachedCommandTextDBType != dbType)
			{
				cachedCommandText = null;
				cachedCommandTextDBType = dbType;
			}

			if (cachedCommandText == null) cachedCommandText = commandText = BuildCommandTextForRecords();
			else commandText = cachedCommandText;
		}

		private DBORMDelete(DBORMDelete<TRecord> template)
			: base(template)
		{
			commandText = template.commandText;
		}

		IORMDelete<TRecord> IORMCommand<IORMDelete<TRecord>>.Clone() => new DBORMDelete<TRecord>(this);

		protected override IDBDelete BuildCommand(IDBCommandBuilder cb)
		{
			var template = cb.Delete();
			template.Table(cb.Identifier(meta.DBSchema, meta.DBName));
			return template;
		}

		private static void PrepareTemplate(IDBCommandBuilder cb, IDBDelete template, IMetaRecord meta)
		{
			var idfield = meta.PrimaryKey.DBName;
			var rowVersionField = meta.RowVersion;
			template.Where(cb.Equal(cb.Identifier(idfield), cb.Parameter(idfield)));
			if (rowVersionField != null) template.Where(cb.Equal(cb.Identifier(rowVersionField.DBName), cb.Parameter(rowVersionField.DBName)));
		}

		private string BuildCommandTextForRecords()
		{
			var command = Command.Clone();
			PrepareTemplate(DB.CommandBuilder, command, meta);
			return command.ToString();
		}

		public IORMDelete<TRecord> Where(IDBExpression expression)
		{
			commandText = null;
			Command.Where(expression);
			return this;
		}

		public IORMDelete<TRecord> Or()
		{
			commandText = null;
			Command.StartWhereGroup();
			return this;
		}

		public IORMDelete<TRecord> Tag(IDBComment comment)
		{
			commandText = null;
			Command.Tag(comment);
			return this;
		}

		public int Execute()
		{
			var token = LogStart();
			var sql = Command.ToString();
			LogRaw(token, sql, Parameters);
			var result = DB.ExecuteNonQueryRaw(sql, Parameters);
			LogEnd(token, result);
			return result;
		}

		public int Records(IEnumerable<TRecord> records)
		{
			if (DB.IsBatchSupported && commandText != null && records.Count() > 1 && records.First() is not IRecordNotifyDelete) return RecordsBatch(records);
			var token = LogStart();
			var idfield = meta.PrimaryKey.DBName;
			var rowVersionField = meta.RowVersion;

			using (var cmdDelete = DB.CreateCommand(commandText ?? BuildCommandTextForRecords()))
			{
				int count = 0;
				RecordNotifyResult result = RecordNotifyResult.Continue;
				DbParameter idParameter = DB.AddParameter(cmdDelete, idfield, null);
				DbParameter rowVersionParameter = rowVersionField == null ? null : DB.AddParameter(cmdDelete, rowVersionField.DBName, null);
				foreach (var record in records)
				{
					var notify = record as IRecordNotifyDelete;
					if (notify != null) result = notify.OnDelete();
					if (result == RecordNotifyResult.Break) break;
					if (result == RecordNotifyResult.Skip) continue;

					LogRecord(LogLevel.Information, token, record);
					var rowVersion = record as IRecordWithRowVersion;
					if (rowVersion != null) DB.SetParameter(rowVersionParameter, rowVersion.RowVersion);
					DB.SetParameter(idParameter, record.ID);
					int lcount = DB.ExecuteNonQuery(cmdDelete);
					if (rowVersion != null && lcount == 0) throw new KORMConcurrentModificationException(cmdDelete.CommandText, cmdDelete.Parameters);

					count += lcount;
					if (notify != null) result = notify.OnDeleted();
					if (result == RecordNotifyResult.Break) break;
				}
				LogEnd(token, count);
				return count;
			}
		}

		private int RecordsBatch(IEnumerable<TRecord> records)
		{
			var token = LogStart();
			var idfield = meta.PrimaryKey.DBName;
			var rowVersionField = meta.RowVersion?.DBName;

			using var batch = DB.CreateBatch();

			foreach (var record in records)
			{
				LogRecord(LogLevel.Information, token, record);
				var command = DB.CreateCommand(batch, commandText);
				DB.AddParameter(command, idfield, record.ID);
				if (rowVersionField != null) DB.AddParameter(command, rowVersionField, ((IRecordWithRowVersion)record).RowVersion);
			}

			var count = DB.ExecuteNonQuery(batch);

			foreach (var command in batch.BatchCommands)
			{
				if (command.RecordsAffected == 0) throw new KORMConcurrentModificationException(command.CommandText, command.Parameters);
			}

			LogEnd(token, count);
			return count;
		}

		public async Task<int> ExecuteAsync()
		{
			var token = LogStart();
			var sql = Command.ToString();
			LogRaw(token, sql, Parameters);
			var result = await DB.ExecuteNonQueryRawAsync(sql, Parameters);
			LogEnd(token, result);
			return result;
		}

		public async Task<int> RecordsAsync(IEnumerable<TRecord> records)
		{
			if (DB.IsBatchSupported && commandText != null && records.Count() > 1 && records.First() is not IRecordNotifyDelete) return await RecordsBatchAsync(records);

			var token = LogStart();
			var idfield = meta.PrimaryKey.DBName;
			var rowVersionField = meta.RowVersion;

			using (var cmdDelete = DB.CreateCommand(commandText ?? BuildCommandTextForRecords()))
			{
				int count = 0;
				RecordNotifyResult result = RecordNotifyResult.Continue;
				DbParameter idParameter = DB.AddParameter(cmdDelete, idfield, null);
				DbParameter rowVersionParameter = rowVersionField == null ? null : DB.AddParameter(cmdDelete, rowVersionField.DBName, null);
				foreach (var record in records)
				{
					var notify = record as IRecordNotifyDelete;
					if (notify != null) result = notify.OnDelete();
					if (result == RecordNotifyResult.Break) break;
					if (result == RecordNotifyResult.Skip) continue;

					LogRecord(LogLevel.Information, token, record);
					var rowVersion = record as IRecordWithRowVersion;
					if (rowVersion != null) DB.SetParameter(rowVersionParameter, rowVersion.RowVersion);
					DB.SetParameter(idParameter, record.ID);
					int lcount = await DB.ExecuteNonQueryAsync(cmdDelete);
					if (rowVersion != null && lcount == 0) throw new KORMConcurrentModificationException(cmdDelete.CommandText, cmdDelete.Parameters);

					count += lcount;
					if (notify != null) result = notify.OnDeleted();
					if (result == RecordNotifyResult.Break) break;
				}
				LogEnd(token, count);
				return count;
			}
		}

		private async Task<int> RecordsBatchAsync(IEnumerable<TRecord> records)
		{
			var token = LogStart();
			var idfield = meta.PrimaryKey.DBName;
			var rowVersionField = meta.RowVersion?.DBName;

			using var batch = DB.CreateBatch();

			foreach (var record in records)
			{
				LogRecord(LogLevel.Information, token, record);
				var command = DB.CreateCommand(batch, commandText);
				DB.AddParameter(command, idfield, record.ID);
				if (rowVersionField != null) DB.AddParameter(command, rowVersionField, ((IRecordWithRowVersion)record).RowVersion);
			}

			var count = await DB.ExecuteNonQueryAsync(batch);

			foreach (var command in batch.BatchCommands)
			{
				if (command.RecordsAffected == 0) throw new KORMConcurrentModificationException(command.CommandText, command.Parameters);
			}

			LogEnd(token, count);
			return count;
		}

		public override string ToString()
		{
			return commandText ?? Command.ToString();
		}
	}
}
