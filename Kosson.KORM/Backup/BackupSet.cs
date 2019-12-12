using Kosson.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Kosson.KRUD
{
	class BackupSet : IBackupSet
	{
		private IORM orm;
		private IBackupWriter writer;
		private IMetaBuilder metaBuilder;
		private IPropertyBinder propertyBinder;
		private IConverter converter;
		private IFactory factory;
		private HashSet<Type> tablesCompleted;
		private HashSet<Type> tablesInProgress;
		private Dictionary<Type, HashSet<long>> recordsCompleted;

		public BackupSet(IORM orm, IBackupWriter writer, IMetaBuilder metaBuilder, IPropertyBinder propertyBinder, IConverter converter, IFactory factory)
		{
			this.orm = orm;
			this.writer = writer;
			this.metaBuilder = metaBuilder;
			this.propertyBinder = propertyBinder;
			this.converter = converter;
			this.factory = factory;
			tablesCompleted = new HashSet<Type>();
			tablesInProgress = new HashSet<Type>();
			recordsCompleted = new Dictionary<Type, HashSet<long>>();
		}

		public void AddRecords<T>(IEnumerable<T> records)
			where T : class, IRecord, new()
		{
			AddRecordsInternal(typeof(T), records ?? GetRecords<T>());

		}

		private IEnumerable GetRecords<T>()
			where T : class, IRecord, new()
		{
			return orm.Select<T>().Execute();
		}

		public void AddTable(Type type)
		{
			var records = new Func<IEnumerable>(GetRecords<Record>).ChangeDelegateGenericArgument(type)();
			AddRecordsInternal(type, records);
		}

		private void AddRecordsInternal(Type type, IEnumerable records)
		{
			if (tablesCompleted.Contains(type)) return;
			tablesInProgress.Add(type);
			var leftoverForeigns = AddForeignRecords(type);
			if (leftoverForeigns.Any())
			{
				AddRecordsAndForeign(type, records, leftoverForeigns);
			}
			else
			{
				AddRecordsIgnoreForeign(records);
			}
			tablesCompleted.Add(type);
			tablesInProgress.Remove(type);
			recordsCompleted.Remove(type);
		}

		private IEnumerable<IMetaRecordField> AddForeignRecords(Type type)
		{
			var leftoverForeigns = new List<IMetaRecordField>();
			var meta = metaBuilder.Get(type);
			AddForeignRecords(meta, leftoverForeigns);
			return leftoverForeigns;
		}

		private void AddForeignRecords(IMetaRecord meta, List<IMetaRecordField> leftoverForeigns)
		{
			foreach (var field in meta.Fields)
			{
				var fieldType = field.Type;
				if (field.IsRecordRef) fieldType = fieldType.GetGenericArguments()[0];
				if (field.IsInline)
				{
					AddForeignRecords(field.InlineRecord, leftoverForeigns);
					continue;
				}
				if (!field.IsForeignKey) continue;
				if (tablesCompleted.Contains(fieldType)) continue;
				if (tablesInProgress.Contains(fieldType))
				{
					leftoverForeigns.Add(field);
				}
				else
				{
					AddTable(fieldType);
				}
			}
		}

		private void AddRecordsIgnoreForeign(IEnumerable records)
		{
			// TODO: should check recordsCompleted?
			foreach (IRecord record in records)
			{
				writer.WriteRecord(record);
			}
		}

		private void AddRecordsAndForeign(Type type, IEnumerable records, IEnumerable<IMetaRecordField> foreigns)
		{
			HashSet<long> recordsCompletedForType;
			if (!recordsCompleted.TryGetValue(type, out recordsCompletedForType))
			{
				recordsCompletedForType = new HashSet<long>();
				recordsCompleted[type] = recordsCompletedForType;
			}

			var recordsInProgress = new Stack<IRecord>();
			foreach (IRecord record in records)
			{
				var id = record.ID;
				if (recordsCompletedForType.Contains(id)) continue;
				recordsInProgress.Push(record);
				AddRecordAndForeign(type, record, foreigns, recordsInProgress);
				recordsInProgress.Pop();
				recordsCompletedForType.Add(id);
			}
		}

		private IRecord GetRecordFromRef<T>(IRecordRef recordRef)
			where T : class, IRecord, new()
		{
			return orm.Get<T>((RecordRef<T>)recordRef);
		}

		private IRecord GetRecordFromRef(Type type, IRecordRef recordRef)
		{
			var typed = new Func<IRecordRef, IRecord>(GetRecordFromRef<Record>).ChangeDelegateGenericArgument(type);
			return typed(recordRef);
			//return (IRecord)GetType().GetMethod("GetRecordFromRefTyped", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).MakeGenericMethod(type).Invoke(this, new[] { recordRef });
		}

		private void AddRecordAndForeign(Type type, IRecord record, IEnumerable<IMetaRecordField> foreigns, Stack<IRecord> recordsInProgress)
		{
			foreach (var field in foreigns)
			{
				string inlinePrefix = "";
				IMetaRecordField inline = field.Record.InliningField;
				IRecord foreignRecord = null;
				while (inline != null)
				{
					inlinePrefix = inline.Name + ".";
					inline = inline.Record.InliningField;
				}

				var fieldType = field.Type;
				var rawValue = propertyBinder.Get(record, inlinePrefix + field.Name);
				if (field.IsRecordRef)
				{
					fieldType = fieldType.GetGenericArguments()[0];
					var fieldRef = converter.Convert<IRecordRef>(rawValue);
					// fieldRef (RecordRef) can be null if inlinePrefix-pointed field is null
					if (fieldRef == null || fieldRef.ID == 0) continue;

					foreignRecord = GetRecordFromRef(fieldType, fieldRef);
				}
				else
				{
					foreignRecord = converter.Convert<IRecord>(rawValue);
				}

				if (foreignRecord == null) continue;

				HashSet<long> recordsCompletedForForeign = null;
				if (recordsInProgress.Contains(foreignRecord))
				{
					var id = foreignRecord.ID;
					foreignRecord = (IRecord)factory.Create(fieldType);
					foreignRecord.ID = id;
				}
				else
				{
					if (!recordsCompleted.TryGetValue(fieldType, out recordsCompletedForForeign))
					{
						recordsCompletedForForeign = new HashSet<long>();
						recordsCompleted[type] = recordsCompletedForForeign;
					}

					if (recordsCompletedForForeign.Contains(foreignRecord.ID)) continue;
				}
				recordsInProgress.Push(foreignRecord);
				var foreignForeigns = AddForeignRecords(fieldType); // just to build leftovers
				AddRecordAndForeign(fieldType, foreignRecord, foreignForeigns, recordsInProgress);
				recordsInProgress.Pop();
				if (recordsCompletedForForeign != null) recordsCompletedForForeign.Add(foreignRecord.ID);
			}
			writer.WriteRecord(record);
		}

		public void Dispose()
		{
			writer.Dispose();
		}
	}
}
