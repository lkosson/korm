using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMUpdate<TRecord> : DBORMCommandBase<TRecord, IDBUpdate>, IORMUpdate<TRecord> where TRecord : IRecord
	{
		private const string ROWVERSION_CURRENT = "ROWVERSION_CURRENT";
		protected override bool UseFullFieldNames { get { return false; } }
		private string commandText;
		private static string cachedCommandText;
		private static Type cachedCommandTextDBType;

		public DBORMUpdate(IDB db, IMetaBuilder metaBuilder, ILogger operationLogger, ILogger recordLogger)
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

		protected override IDBUpdate BuildCommand(IDBCommandBuilder cb)
		{
			var template = cb.Update();
			template.Table(cb.Identifier(meta.DBSchema, meta.DBName));
			return template;
		}

		private static void PrepareTemplate(IDBCommandBuilder cb, IDBUpdate template, IMetaRecord meta)
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsPrimaryKey)
				{
					template.Where(cb.Equal(cb.Identifier(field.DBName), cb.Parameter(field.DBName)));
					continue;
				}

				if (field.IsReadOnly) continue;
				if (field.IsInline)
				{
					PrepareTemplate(cb, template, field.InlineRecord);
				}
				else
				{
					template.Set(cb.Identifier(field.DBName), cb.Parameter(field.DBName));
				}
			}

			var rvfield = meta.RowVersion;
			if (rvfield != null) template.Where(cb.Equal(cb.Identifier(rvfield.DBName), cb.Parameter(ROWVERSION_CURRENT)));
		}

		private string BuildCommandTextForRecords()
		{
			var command = Command.Clone();
			PrepareTemplate(DB.CommandBuilder, command, meta);
			return command.ToString();
		}

		public IORMUpdate<TRecord> Set(IDBIdentifier field, IDBExpression value)
		{
			commandText = null;
			Command.Set(field, value);
			return this;
		}

		public IORMUpdate<TRecord> Where(IDBExpression expression)
		{
			commandText = null;
			Command.Where(expression);
			return this;
		}

		public IORMUpdate<TRecord> Or()
		{
			commandText = null;
			Command.StartWhereGroup();
			return this;
		}

		public IORMUpdate<TRecord> Tag(IDBComment comment)
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

		private void ApplyRowVersion(IRecordWithRowVersion record, DbParameter rowVersionParameter)
		{
			if (record == null) return;
			long current = record.RowVersion;
			DB.SetParameter(rowVersionParameter, current);
			record.RowVersion++;
		}

		public int Records(IEnumerable<TRecord> records)
		{
			var token = LogStart();

			using (var cmdUpdate = DB.CreateCommand(commandText ?? BuildCommandTextForRecords()))
			{
				int count = 0;
				RecordNotifyResult result = RecordNotifyResult.Continue;
				DbParameter[] parameters = null;
				DbParameter rowVersionParameter = meta.RowVersion == null ? null : DB.AddParameter(cmdUpdate, ROWVERSION_CURRENT, null);
				foreach (var record in records)
				{
					var notify = record as IRecordNotifyUpdate;
					if (notify != null) result = notify.OnUpdate();
					if (result == RecordNotifyResult.Break) break;
					if (result == RecordNotifyResult.Skip) continue;

					LogRecord(LogLevel.Information, token, record);
					var rowVersion = record as IRecordWithRowVersion;
					ApplyRowVersion(rowVersion, rowVersionParameter);
					DBParameterLoader<TRecord>.Run(DB, meta, cmdUpdate, record, ref parameters);
					int lcount = DB.ExecuteNonQuery(cmdUpdate);
					if (rowVersion != null && lcount == 0) throw new KORMConcurrentModificationException(cmdUpdate);

					count += lcount;
					if (notify != null) result = notify.OnUpdated();
					if (result == RecordNotifyResult.Break) break;
				}
				LogEnd(token, count);
				return count;
			}
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
			var token = LogStart();

			using (var cmdUpdate = DB.CreateCommand(commandText ?? BuildCommandTextForRecords()))
			{
				int count = 0;
				RecordNotifyResult result = RecordNotifyResult.Continue;
				DbParameter[] parameters = null;
				DbParameter rowVersionParameter = meta.RowVersion == null ? null : DB.AddParameter(cmdUpdate, ROWVERSION_CURRENT, null);
				foreach (var record in records)
				{
					var notify = record as IRecordNotifyUpdate;
					if (notify != null) result = notify.OnUpdate();
					if (result == RecordNotifyResult.Break) break;
					if (result == RecordNotifyResult.Skip) continue;

					LogRecord(LogLevel.Information, token, record);
					var rowVersion = record as IRecordWithRowVersion;
					ApplyRowVersion(rowVersion, rowVersionParameter);
					DBParameterLoader<TRecord>.Run(DB, meta, cmdUpdate, record, ref parameters);
					int lcount = await DB.ExecuteNonQueryAsync(cmdUpdate);
					if (rowVersion != null && lcount == 0) throw new KORMConcurrentModificationException(cmdUpdate);

					count += lcount;
					if (notify != null) result = notify.OnUpdated();
					if (result == RecordNotifyResult.Break) break;
				}
				LogEnd(token, count);
				return count;
			}
		}

		public override string ToString()
		{
			return commandText ?? Command.ToString();
		}
	}
}
