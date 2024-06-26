using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMInsert<TRecord> : DBORMCommandBase<TRecord, IDBInsert>, IORMInsert<TRecord> where TRecord : IRecord
	{
		private IConverter converter;
		private string commandText;
		private static string cachedCommandText;
		private static Type cachedCommandTextDBType;

		public DBORMInsert(IDB db, IMetaBuilder metaBuilder, IConverter converter, ILogger operationLogger, ILogger recordLogger)
			: base(db, metaBuilder, operationLogger, recordLogger)
		{
			this.converter = converter;
			var dbType = db.GetType();
			if (cachedCommandTextDBType != dbType)
			{
				cachedCommandText = null;
				cachedCommandTextDBType = dbType;
			}

			if (cachedCommandText == null) cachedCommandText = commandText = Command.ToString();
			else commandText = cachedCommandText;
		}

		private DBORMInsert(DBORMInsert<TRecord> template)
			: base(template)
		{
			converter = template.converter;
			commandText = template.commandText;
		}

		IORMInsert<TRecord> IORMCommand<IORMInsert<TRecord>>.Clone() => new DBORMInsert<TRecord>(this);

		protected override IDBInsert BuildCommand(IDBCommandBuilder cb)
		{
			var template = cb.Insert();
			template.Table(cb.Identifier(meta.DBSchema, meta.DBName));
			template.PrimaryKeyReturn(cb.Identifier(meta.PrimaryKey.DBName));

			PrepareTemplate(cb, template, meta);

			return template;
		}

		private static void PrepareTemplate(IDBCommandBuilder cb, IDBInsert template, IMetaRecord meta)
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly) continue;
				if (field.IsInline)
				{
					PrepareTemplate(cb, template, field.InlineRecord);
				}
				else
				{
					template.Column(cb.Identifier(field.DBName), cb.Parameter(field.DBName));
				}
			}
		}

		public IORMInsert<TRecord> Tag(IDBComment comment)
		{
			commandText = null;
			Command.Tag(comment);
			return this;
		}

		public IORMInsert<TRecord> WithProvidedID()
		{
			commandText = null;
			var cb = DB.CommandBuilder;
			var pk = meta.PrimaryKey;
			Command.PrimaryKeyInsert(cb.Identifier(pk.DBName), cb.Parameter(pk.DBName));
			return this;
		}

		public int Records(IEnumerable<TRecord> records)
		{
			if (DB.IsBatchSupported && commandText != null && records.Count() > 1 && records.First() is not IRecordNotifyInsert) return RecordsBatch(records);

			var token = LogStart();
			var getlastid = template.GetLastID;
			var manualid = meta.IsManualID;
			using (var cmdInsert = DB.CreateCommand(commandText ?? Command.ToString()))
			using (var cmdGetLastID = getlastid == null ? null : DB.CreateCommand(getlastid))
			{
				int count = 0;
				RecordNotifyResult result = RecordNotifyResult.Continue;
				foreach (var record in records)
				{
					var notify = record as IRecordNotifyInsert;
					if (notify != null) result = notify.OnInsert();
					if (result == RecordNotifyResult.Break) break;
					if (result == RecordNotifyResult.Skip) continue;

					LogRecord(LogLevel.Information, token, record);
					DB.ClearParameters(cmdInsert);
					DBParameterLoader<TRecord>.Run(DB, meta, cmdInsert, record);

					if (cmdGetLastID != null) DB.ExecuteNonQuery(cmdInsert);
					var rows = DB.ExecuteQuery(cmdGetLastID ?? cmdInsert);
					if (!manualid && rows.Any()) record.ID = converter.Convert<long>(rows.First()[0]);

					LogID(token, record);

					count++;
					if (notify != null) result = notify.OnInserted();
					if (result == RecordNotifyResult.Break) break;
				}
				LogEnd(token, count);
				return count;
			}
		}

		private int RecordsBatch(IEnumerable<TRecord> records)
		{
			var token = LogStart();
			var getlastid = template.GetLastID;
			var manualid = meta.IsManualID;

			using var batch = DB.CreateBatch();

			foreach (var record in records)
			{
				LogRecord(LogLevel.Information, token, record);
				var command = DB.CreateCommand(batch, commandText);
				DBParameterLoader<TRecord>.Run(DB, meta, command, record);

				if (getlastid != null) DB.CreateCommand(batch, getlastid);
			}

			using var reader = DB.ExecuteReader(batch);

			int count = 0;
			foreach (var record in records)
			{
				if (!reader.Read()) break;
				var id = reader.GetInt64(0);
				if (!manualid) record.ID = id;
				LogID(token, record);

				reader.NextResult();
				count++;
			}

			LogEnd(token, count);
			return count;
		}

		public async Task<int> RecordsAsync(IEnumerable<TRecord> records)
		{
			if (DB.IsBatchSupported && commandText != null && records.Count() > 1 && records.First() is not IRecordNotifyInsert) return await RecordsBatchAsync(records);

			var token = LogStart();
			var getlastid = template.GetLastID;
			var manualid = meta.IsManualID;
			using (var cmdInsert = DB.CreateCommand(commandText ?? Command.ToString()))
			using (var cmdGetLastID = getlastid == null ? null : DB.CreateCommand(getlastid))
			{
				int count = 0;
				RecordNotifyResult result = RecordNotifyResult.Continue;
				foreach (var record in records)
				{
					var notify = record as IRecordNotifyInsert;
					if (notify != null) result = notify.OnInsert();
					if (result == RecordNotifyResult.Break) break;
					if (result == RecordNotifyResult.Skip) continue;

					LogRecord(LogLevel.Information, token, record);
					DB.ClearParameters(cmdInsert);
					DBParameterLoader<TRecord>.Run(DB, meta, cmdInsert, record);

					if (cmdGetLastID != null) await DB.ExecuteNonQueryAsync(cmdInsert);
					var rows = await DB.ExecuteQueryAsync(cmdGetLastID ?? cmdInsert);
					if (!manualid && rows.Any()) record.ID = converter.Convert<long>(rows.First()[0]);
					LogID(token, record);

					count++;
					if (notify != null) result = notify.OnInserted();
					if (result == RecordNotifyResult.Break) break;
				}
				LogEnd(token, count);
				return count;
			}
		}

		private async Task<int> RecordsBatchAsync(IEnumerable<TRecord> records)
		{
			var token = LogStart();
			var getlastid = template.GetLastID;
			var manualid = meta.IsManualID;

			using var batch = DB.CreateBatch();

			foreach (var record in records)
			{
				LogRecord(LogLevel.Information, token, record);
				var command = DB.CreateCommand(batch, commandText);
				DBParameterLoader<TRecord>.Run(DB, meta, command, record);

				if (getlastid != null) DB.CreateCommand(batch, getlastid);
			}

			using var reader = await DB.ExecuteReaderAsync(batch);

			int count = 0;
			foreach (var record in records)
			{
				if (!await reader.ReadAsync()) break;
				var id = reader.GetInt64(0);
				if (!manualid) record.ID = id;
				LogID(token, record);

				await reader.NextResultAsync();
				count++;
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
