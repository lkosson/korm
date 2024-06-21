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
			var token = LogStart();
			var getlastid = template.GetLastID;
			var manualid = meta.IsManualID;
			using (var cmdInsert = DB.CreateCommand(commandText ?? Command.ToString()))
			using (var cmdGetLastID = getlastid == null ? null : DB.CreateCommand(getlastid))
			{
				int count = 0;
				RecordNotifyResult result = RecordNotifyResult.Continue;
				DbParameter[] parameters = null;
				foreach (var record in records)
				{
					var notify = record as IRecordNotifyInsert;
					if (notify != null) result = notify.OnInsert();
					if (result == RecordNotifyResult.Break) break;
					if (result == RecordNotifyResult.Skip) continue;

					LogRecord(LogLevel.Information, token, record);
					DBParameterLoader<TRecord>.Run(DB, meta, cmdInsert, record, ref parameters);

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

		public async Task<int> RecordsAsync(IEnumerable<TRecord> records)
		{
			var token = LogStart();
			var getlastid = template.GetLastID;
			var manualid = meta.IsManualID;
			using (var cmdInsert = DB.CreateCommand(commandText ?? Command.ToString()))
			using (var cmdGetLastID = getlastid == null ? null : DB.CreateCommand(getlastid))
			{
				int count = 0;
				RecordNotifyResult result = RecordNotifyResult.Continue;
				DbParameter[] parameters = null;
				foreach (var record in records)
				{
					var notify = record as IRecordNotifyInsert;
					if (notify != null) result = notify.OnInsert();
					if (result == RecordNotifyResult.Break) break;
					if (result == RecordNotifyResult.Skip) continue;

					LogRecord(LogLevel.Information, token, record);
					DBParameterLoader<TRecord>.Run(DB, meta, cmdInsert, record, ref parameters);

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

		public override string ToString()
		{
			return commandText ?? Command.ToString();
		}
	}
}
