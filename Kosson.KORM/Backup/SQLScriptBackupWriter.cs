﻿using Kosson.KORM.ORM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kosson.KORM.Backup
{
	class SQLScriptBackupWriter : IBackupWriter
	{
		private readonly StreamWriter sw;
		private readonly Dictionary<Type, TableState> tableStates;
		private readonly IMetaBuilder metaBuilder;
		private readonly IDBCommandBuilder cb;
		private readonly DBTableCreator tc;

		/// <summary>
		/// Determines whether SQL commands creating database structure should be emitted.
		/// </summary>
		public bool CreateStructure { get; set; }

		public SQLScriptBackupWriter(IDB db, IMetaBuilder metaBuilder, Stream stream)
		{
			this.metaBuilder = metaBuilder;
			sw = new StreamWriter(stream, Encoding.UTF8, 65536, true);
			tableStates = new Dictionary<Type, TableState>();
			cb = db.CommandBuilder;
			tc = new ORM.DBTableCreator(db, metaBuilder, null, WriteCreateTableCommand);
			CreateStructure = true;
			sw.WriteLine("BEGIN TRANSACTION");
		}

		void IBackupWriter.WriteRecord(IRecord record)
		{
			var type = record.GetType();
			TableState? tableState;
			if (!tableStates.TryGetValue(type, out tableState))
			{
				var meta = metaBuilder.Get(type);
				var tableType = meta.TableType;
				if (tableType == null) return;
				if (!tableStates.TryGetValue(tableType, out tableState))
				{
					tableState = new TableState(meta);
					if (CreateStructure) WriteCreateTable(meta);
				}
				tableStates[type] = tableState;
			}

			if (tableState.recordsWritten.Contains(record.ID))
			{
				WriteUpdate(tableState.meta, record);
			}
			else
			{
				// following assumes IDs are assigned sequentially after last explicitly set ID
				WriteInsert(tableState.meta, record.ID != tableState.maxWrittenId + 1, record);
				tableState.recordsWritten.Add(record.ID);
				if (record.ID > tableState.maxWrittenId) tableState.maxWrittenId = record.ID;
			}
		}

		private void WriteCreateTable(IMetaRecord meta)
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsForeignKey || !(field.IsEagerLookup || field.IsRecordRef)) continue;
				if (tableStates.ContainsKey(field.ForeignType)) continue;
				tc.CreateTable(metaBuilder.Get(field.ForeignType));
			}
			tc.Create(meta);
		}

		private void WriteCreateTableCommand(IDBCommand command)
		{
			sw.WriteLine(command.ToString());
		}

		private void WriteInsert(IMetaRecord meta, bool includePK, IRecord record)
		{
			var insert = cb.Insert();
			insert.Table(cb.Identifier(meta.DBSchema, meta.DBName));
			BuildInsert(insert, meta, includePK, record);
			sw.WriteLine(insert.ToString());
		}

		private void BuildInsert(IDBInsert insert, IMetaRecord meta, bool includePK, object record)
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly && !field.IsPrimaryKey) continue;
				var value = field.Property.GetValue(record);
				if (field.IsInline)
				{
					if (value != null) BuildInsert(insert, field.InlineRecord, false, value);
				}
				else
				{
					if (value is IRecord) value = ((IRecord)value).ID;
					if (field.IsPrimaryKey)
					{
						if (includePK) insert.PrimaryKeyInsert(cb.Identifier(field.DBName), cb.Const(value));
					}
					else
					{
						insert.Column(cb.Identifier(field.DBName), cb.Const(value));
					}
				}
			}
		}

		private void WriteUpdate(IMetaRecord meta, IRecord record)
		{
			var update = cb.Update();
			update.Table(cb.Identifier(meta.DBSchema, meta.DBName));
			BuildUpdate(update, meta, record);
			sw.WriteLine(update.ToString());
		}

		private void BuildUpdate(IDBUpdate update, IMetaRecord meta, object record)
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly && !field.IsPrimaryKey) continue;

				var value = field.Property.GetValue(record);
				if (field.IsPrimaryKey)
				{
					update.Where(cb.Equal(cb.Identifier(field.DBName), cb.Const(value)));
				}
				else if (field.IsInline)
				{
					if (value != null) BuildUpdate(update, field.InlineRecord, value);
				}
				else
				{
					if (value is IRecord) value = ((IRecord)value).ID;
					update.Set(cb.Identifier(field.DBName), cb.Const(value));
				}
			}
		}

		void IDisposable.Dispose()
		{
			sw.WriteLine("COMMIT");
			sw.Dispose();
		}

		class TableState
		{
			public HashSet<long> recordsWritten;
			public IMetaRecord meta;
			public long maxWrittenId;

			public TableState(IMetaRecord meta)
			{
				this.meta = meta;
				recordsWritten = new HashSet<long>();
			}
		}
	}
}
