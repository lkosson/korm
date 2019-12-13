using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Kosson.KORM.Backup
{
	class BackupRestorer : IBackupRestorer
	{
		private readonly IMetaBuilder metaBuilder;
		private readonly IORM orm;
		private readonly IPropertyBinder propertyBinder;
		private readonly IFactory factory;

		private readonly bool supportsPKInsert;
		private readonly Dictionary<string, TableState> tableStates;

		private Action<IRecord> storeRecordDelegate;
		private Action<IRecord> insertRecordPKDelegate;
		private Type storeRecordDelegateType;
		private Type insertRecordPKDelegateType;

		public BackupRestorer(IMetaBuilder metaBuilder, IORM orm, IDB db, IPropertyBinder propertyBinder, IFactory factory)
		{
			this.metaBuilder = metaBuilder;
			this.orm = orm;
			this.propertyBinder = propertyBinder;
			this.factory = factory;
			tableStates = new Dictionary<string, TableState>();
			supportsPKInsert = db.CommandBuilder.SupportsPrimaryKeyInsert;
		}

		public void Restore(IBackupReader reader)
		{
			var previousCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				IRecord record;
				while ((record = reader.ReadRecord()) != null)
				{
					ProcessRecord(record);
				}
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = previousCulture;
			}
		}

		private TableState GetTableState(Type type)
		{
			TableState tableState;
			var typeName = type.FullName;
			if (!tableStates.TryGetValue(typeName, out tableState))
			{
				var tableName = metaBuilder.Get(type).DBName;
				if (!tableStates.TryGetValue(tableName, out tableState))
				{
					tableState = new TableState(orm, factory, type);
				}
				tableStates[typeName] = tableState;
				tableStates[tableName] = tableState;
			}
			return tableState;
		}

		internal void ProcessRecord(IRecord record)
		{
			var type = record.GetType();

			FixForeignKeys(record);

			var tableState = GetTableState(type);
			var rowVersion = tableState.RowVersions;
			var idMapping = tableState.IDMappings;

			bool insert;
			bool insertPK;
			long oldId;
			if (idMapping.ContainsKey(record.ID))
			{
				oldId = 0;
				insert = false;
				insertPK = false;
				record.ID = idMapping[record.ID];
			}
			else
			{
				oldId = record.ID;
				insert = true;
				insertPK = supportsPKInsert && tableState.MaxWrittenID + 1 != record.ID && tableState.MaxExistingID < record.ID;
				if (!insertPK) record.ID = 0;
			}

			var rv = record as IRecordWithRowVersion;
			if (rv != null && rowVersion.ContainsKey(record.ID))
			{
				rv.RowVersion = rowVersion[record.ID];
			}

			if (insertPK)
			{
				InsertRecordPK(type, record);
			}
			else
			{
				StoreRecord(type, record);
			}

			if (tableState.MaxWrittenID < record.ID) tableState.MaxWrittenID = record.ID;
			if (insert) idMapping[oldId] = record.ID;
			if (rv != null) rowVersion[record.ID] = rv.RowVersion;
		}

		private void FixForeignKeys(object target)
		{
			var meta = metaBuilder.Get(target.GetType());
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly) continue;
				if (field.IsInline)
				{
					var value = field.Property.GetValue(target);
					if (value != null) FixForeignKeys(value);
				}
				else if (field.IsForeignKey)
				{
					if (field.IsRecordRef)
					{
						var value = (IRecordRef)field.Property.GetValue(target);
						if (value == null || value.ID == 0) continue;
						var targetType = value.GetType();
						targetType = targetType.GetGenericArguments()[0];
						var newId = GetTableState(targetType).IDMappings[value.ID];
						value.ID = newId;
						propertyBinder.Set(target, field.Name, value);
					}
					else
					{
						var value = (IRecord)field.Property.GetValue(target);
						if (value == null || value.ID == 0) continue;
						var targetType = value.GetType();
						var newId = GetTableState(targetType).IDMappings[value.ID];
						value.ID = newId;
					}
				}
			}
		}

		private void StoreRecord(Type type, IRecord record)
		{
			if (type != storeRecordDelegateType)
			{
				storeRecordDelegate = new Action<IRecord>(StoreRecord<IRecord>).ChangeDelegateGenericArgument(type);
				storeRecordDelegateType = type;
			}
			storeRecordDelegate(record);
		}

		private void StoreRecord<T>(IRecord record)
			where T : IRecord
		{
			orm.Store((T)record);
		}

		private void InsertRecordPK(Type type, IRecord record)
		{
			if (type != insertRecordPKDelegateType)
			{
				insertRecordPKDelegate = new Action<IRecord>(InsertRecordPK<IRecord>).ChangeDelegateGenericArgument(type);
				insertRecordPKDelegateType = type;
			}
			insertRecordPKDelegate(record);
		}

		private void InsertRecordPK<T>(IRecord record)
			where T : IRecord
		{
			orm.Insert<T>().WithProvidedID().Records(new[] { ((T)record) });
		}

		class TableState
		{
			private IORM orm;
			private IFactory factory;
			public Dictionary<long, long> IDMappings { get; private set; }
			public Dictionary<long, long> RowVersions { get; private set; }
			public long MaxWrittenID { get; set; }
			public long MaxExistingID { get; set; }

			public TableState(IORM orm, IFactory factory, Type type)
			{
				this.orm = orm;
				this.factory = factory;
				IDMappings = new Dictionary<long, long>();
				RowVersions = new Dictionary<long, long>();
				FindMaxID(type);
				FindNextID(type);
				if (MaxExistingID == 0 && MaxWrittenID == 1)
				{
					// Workaround for SQLite - if there are no records, first record will always have ID=1, even after probing with FindNextID gets ID=1 and would normally force next record to have ID=2.
					MaxWrittenID = Int64.MinValue;
				}
			}

			private void FindNextID(Type type)
			{
				new Action(FindNextID<Record>).ChangeDelegateGenericArgument(type)();
			}

			private void FindNextID<TRecord>()
				where TRecord : class, IRecord, new()
			{
				var record = factory.Create<TRecord>();
				orm.Store(record);
				MaxWrittenID = record.ID;
				orm.Delete(record);
			}

			private void FindMaxID(Type type)
			{
				new Action(FindMaxID<Record>).ChangeDelegateGenericArgument(type)();
			}

			private void FindMaxID<TRecord>()
				where TRecord : class, IRecord, new()
			{
				var last = orm.Select<TRecord>().OrderByDescending("ID").ExecuteFirst();
				if (last == null) return;
				MaxExistingID = last.ID;
			}
		}
	}
}
