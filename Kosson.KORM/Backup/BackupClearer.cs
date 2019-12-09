using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD
{
	class BackupClearer
	{
		private IORM orm;
		private IEnumerable<Type> types;
		private HashSet<string> tablesCleared;
		private HashSet<string> tablesInProgress;

		public BackupClearer(IORM orm, IEnumerable<Type> types)
		{
			this.orm = orm;
			this.types = types;
			tablesCleared = new HashSet<string>();
			tablesInProgress = new HashSet<string>();
		}

		public void Clear()
		{
			foreach (var type in types) Clear(type);
		}

		private void Clear(Type type)
		{
			var meta = type.Meta();
			var table = meta.DBName;
			if (tablesCleared.Contains(table)) return;
			if (tablesInProgress.Contains(table))
			{
				NullTable(type);
			}
			else
			{
				tablesInProgress.Add(table);
				ClearReferencingForeignKeys(meta.TableType);
				ClearTable(type);
				tablesInProgress.Remove(table);
				tablesCleared.Add(table);
			}
		}

		private void ClearReferencingForeignKeys(Type type)
		{
			foreach (var reftype in types)
			{
				ClearIfReferences(reftype, reftype, type);
			}
		}

		private void ClearIfReferences(Type typeToClear, Type typeForMeta, Type typeToReference)
		{
			var meta = typeForMeta.Meta();

			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly) continue;
				if (field.IsInline)
				{
					ClearIfReferences(typeToClear, field.Type, typeToReference);
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
					Clear(typeToClear);
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
			var meta = typeof(T).Meta();
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
