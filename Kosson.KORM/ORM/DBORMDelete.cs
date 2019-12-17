using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMDelete<TRecord> : DBORMCommandBase<TRecord, IDBDelete>, IORMDelete<TRecord> where TRecord : IRecord
	{
		protected override bool UseFullFieldNames { get { return false; } }

		public DBORMDelete(IDB db, IMetaBuilder metaBuilder)
			: base(db, metaBuilder)
		{
		}

		protected override IDBDelete BuildCommand(IDBCommandBuilder cb)
		{
			var template = cb.Delete();
			template.Table(cb.Identifier(meta.DBName));
			return template;
		}

		public IORMDelete<TRecord> Where(IDBExpression expression)
		{
			command.Where(expression);
			return this;
		}

		public IORMDelete<TRecord> Or()
		{
			command.StartWhereGroup();
			return this;
		}

		public IORMDelete<TRecord> Tag(IDBComment comment)
		{
			command.Tag(comment);
			return this;
		}

		public int Execute()
		{
			return DB.ExecuteNonQuery(command.ToString(), Parameters);
		}

		public int Records(IEnumerable<TRecord> records)
		{
			var idfield = meta.PrimaryKey.DBName;
			var rowVersionField = meta.RowVersion;
			var cb = DB.CommandBuilder;
			command.Where(cb.Equal(cb.Identifier(idfield), cb.Parameter(idfield)));
			if (rowVersionField != null) command.Where(cb.Equal(cb.Identifier(rowVersionField.DBName), cb.Parameter(rowVersionField.DBName)));

			using (var cmdDelete = DB.CreateCommand(command.ToString()))
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

					var rowVersion = record as IRecordWithRowVersion;
					if (rowVersion != null) DB.SetParameter(rowVersionParameter, rowVersion.RowVersion);
					DB.SetParameter(idParameter, record.ID);
					int lcount = DB.ExecuteNonQuery(cmdDelete);
					if (rowVersion != null && lcount == 0) throw new KORMConcurrentModificationException(cmdDelete);

					count += lcount;
					if (notify != null) result = notify.OnDeleted();
					if (result == RecordNotifyResult.Break) break;
				}
				return count;
			}
		}

		public Task<int> ExecuteAsync()
		{
			return DB.ExecuteNonQueryAsync(command.ToString(), Parameters);
		}

		public async Task<int> RecordsAsync(IEnumerable<TRecord> records)
		{
			var idfield = meta.PrimaryKey.DBName;
			var rowVersionField = meta.RowVersion;
			var cb = DB.CommandBuilder;
			command.Where(cb.Equal(cb.Identifier(idfield), cb.Parameter(idfield)));
			if (rowVersionField != null) command.Where(cb.Equal(cb.Identifier(rowVersionField.DBName), cb.Parameter(rowVersionField.DBName)));

			using (var cmdDelete = DB.CreateCommand(command.ToString()))
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

					var rowVersion = record as IRecordWithRowVersion;
					if (rowVersion != null) DB.SetParameter(rowVersionParameter, rowVersion.RowVersion);
					DB.SetParameter(idParameter, record.ID);
					int lcount = await DB.ExecuteNonQueryAsync(cmdDelete);
					if (rowVersion != null && lcount == 0) throw new KORMConcurrentModificationException(cmdDelete);

					count += lcount;
					if (notify != null) result = notify.OnDeleted();
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
