using System;
using System.Collections.Generic;

namespace Kosson.KORM.Backup
{
	class DatabaseEraser : IDatabaseEraser
	{
		private readonly IORM orm;
		private readonly IMetaBuilder metaBuilder;
		private readonly HashSet<string> tablesCleared;
		private readonly HashSet<string> tablesInProgress;

		public DatabaseEraser(IORM orm, IMetaBuilder metaBuilder)
		{
			this.orm = orm;
			this.metaBuilder = metaBuilder;
			tablesCleared = new HashSet<string>();
			tablesInProgress = new HashSet<string>();
		}

		void IDatabaseEraser.Clear(IEnumerable<Type> types)
		{
			foreach (var type in types) Clear(types, type);
		}

		private void Clear(IEnumerable<Type> types, Type type)
		{
			var meta = metaBuilder.Get(type);
			var table = meta.DBName;
			if (tablesCleared.Contains(table)) return;
			if (tablesInProgress.Contains(table))
			{
				NullTable(type);
			}
			else
			{
				tablesInProgress.Add(table);
				ClearReferencingForeignKeys(types, meta.TableType);
				ClearTable(type);
				tablesInProgress.Remove(table);
				tablesCleared.Add(table);
			}
		}

		private void ClearReferencingForeignKeys(IEnumerable<Type> types, Type type)
		{
			foreach (var reftype in types)
			{
				ClearIfReferences(types, reftype, reftype, type);
			}
		}

		private void ClearIfReferences(IEnumerable<Type> types, Type typeToClear, Type typeForMeta, Type typeToReference)
		{
			var meta = metaBuilder.Get(typeForMeta);

			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly) continue;
				if (field.IsInline)
				{
					ClearIfReferences(types, typeToClear, field.Type, typeToReference);
				}
				else if (field.IsForeignKey)
				{
					if (field.IsRecordRef)
					{
						var ftype = field.Type.GetGenericArguments()[0];
						if (ftype != typeToReference) continue;
					}
					else
					{
						if (field.Type != typeToReference) continue;
					}
					Clear(types, typeToClear);
					break;
				}
			}
		}

		private void NullTable(Type type)
		{
			new Action(NullTable<IRecord>).ChangeDelegateGenericArgument(type)();
		}

		private void NullTable<T>()
			where T : IRecord
		{
			var meta = metaBuilder.Get(typeof(T));
			var update = orm.Update<T>();
			BuildNullCommand<T>(update, meta);
			update.Execute();
		}

		private void BuildNullCommand<T>(IORMUpdate<T> update, IMetaRecord meta)
			where T : IRecord
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly) continue;
				if (field.IsNotNull) continue; // this can cause whole process to fail, but no way to work around this at the moment
				if (field.IsInline)
				{
					BuildNullCommand<T>(update, field.InlineRecord);
				}
				else if (field.IsForeignKey)
				{
					update.Set(field.DBName, null);
				}
			}
		}

		private void ClearTable(Type type)
		{
			new Action<IORM>(ClearTable<IRecord>).ChangeDelegateGenericArgument(type)(orm);
		}

		private static void ClearTable<T>(IORM orm)
			where T : IRecord
		{
			orm.Delete<T>().Execute();
		}
	}
}
