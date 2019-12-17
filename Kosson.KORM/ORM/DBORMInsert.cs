﻿using Kosson.KORM;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMInsert<TRecord> : DBORMCommandBase<TRecord, IDBInsert>, IORMInsert<TRecord> where TRecord : IRecord
	{
		private IConverter converter;

		public DBORMInsert(IDB db, IMetaBuilder metaBuilder, IConverter converter)
			: base(db, metaBuilder)
		{
			this.converter = converter;
		}

		protected override IDBInsert BuildCommand(IDBCommandBuilder cb)
		{
			var template = cb.Insert();
			template.Table(cb.Identifier(meta.DBName));
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
			command.Tag(comment);
			return this;
		}

		public IORMInsert<TRecord> WithProvidedID()
		{
			var cb = DB.CommandBuilder;
			var pk = meta.PrimaryKey;
			command.PrimaryKeyInsert(cb.Identifier(pk.DBName), cb.Parameter(pk.DBName));
			return this;
		}

		public int Records(IEnumerable<TRecord> records)
		{
			var getlastid = command.GetLastID;
			var manualid = meta.IsManualID;
			using (var cmdInsert = DB.CreateCommand(command.ToString()))
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

					DBParameterLoader<TRecord>.Run(DB, meta, cmdInsert, record, ref parameters);

					if (cmdGetLastID == null)
					{
						var row = DB.ExecuteQuery(cmdInsert, 1).Single();
						if (!manualid) record.ID = converter.Convert<long>(row[0]);
					}
					else
					{
						DB.ExecuteNonQuery(cmdInsert);
						var row = DB.ExecuteQuery(cmdGetLastID, 1).Single();
						if (!manualid) record.ID = converter.Convert<long>(row[0]);
					}

					count++;
					if (notify != null) result = notify.OnInserted();
					if (result == RecordNotifyResult.Break) break;
				}
				return count;
			}
		}

		public async Task<int> RecordsAsync(IEnumerable<TRecord> records)
		{
			var getlastid = command.GetLastID;
			var manualid = meta.IsManualID;
			using (var cmdInsert = DB.CreateCommand(command.ToString()))
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

					DBParameterLoader<TRecord>.Run(DB, meta, cmdInsert, record, ref parameters);

					if (cmdGetLastID == null)
					{
						var row = await DB.ExecuteQueryFirstAsync(cmdInsert);
						if (!manualid) record.ID = converter.Convert<long>(row[0]);
					}
					else
					{
						await DB.ExecuteNonQueryAsync(cmdInsert);
						var row = await DB.ExecuteQueryFirstAsync(cmdGetLastID);
						if (!manualid) record.ID = converter.Convert<long>(row[0]);
					}

					count++;
					if (notify != null) result = notify.OnInserted();
					if (result == RecordNotifyResult.Break) break;
				}
				return count;
			}
		}

		public override string ToString()
		{
			return command.ToString();
		}
	}
}
