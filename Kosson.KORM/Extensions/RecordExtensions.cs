using Kosson.Interfaces;
using Kosson.KRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IRecord.
	/// </summary>
	public static class RecordExtensions
	{
		/// <summary>
		/// Adds the record to a database and assigns primary key (ID) to it.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to add.</typeparam>
		/// <param name="record">Record to add to database.</param>
		public static void Insert<TRecord>(this TRecord record) where TRecord : IRecord
		{
			InsertAll((IEnumerable<TRecord>)new[] { record });
		}

		/// <summary>
		/// Asynchronous version of Insert.
		/// Adds the record to a database and assigns primary key (ID) to it.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to add.</typeparam>
		/// <param name="record">Record to add to database.</param>
		public static Task InsertAsync<TRecord>(this TRecord record) where TRecord : IRecord
		{
			return InsertAllAsync((IEnumerable<TRecord>)new[] { record });
		}

		/// <summary>
		/// Adds a set of records to a database and assigns primary key (ID) to all of them.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to add.</typeparam>
		/// <param name="records">Records to add to database.</param>
		public static void InsertAll<TRecord>(this IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var count = KORMContext.Current.ORM.Insert<TRecord>().Records(records);
			if (count != records.Count()) throw new ORMInsertFailedException();
		}

		/// <summary>
		/// Asynchronous version of Insert.
		/// Adds a set of records to a database and assigns primary key (ID) to all of them.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to add.</typeparam>
		/// <param name="records">Records to add to database.</param>
		public async static Task InsertAllAsync<TRecord>(this IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var count = await KORMContext.Current.ORM.Insert<TRecord>().RecordsAsync(records);
			if (count != records.Count()) throw new ORMInsertFailedException();
		}

		/// <summary>
		/// Updates all columns of exising database backing record based on the given record.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to update.</typeparam>
		/// <param name="record">Record to update in database.</param>
		public static void Update<TRecord>(this TRecord record) where TRecord : IRecord
		{
			UpdateAll((IEnumerable<TRecord>)new[] { record });
		}

		/// <summary>
		/// Asynchronous version of Update.
		/// Updates all columns of exising database backing record based on the given record.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to update.</typeparam>
		/// <param name="record">Record to update in database.</param>
		public static Task UpdateAsync<TRecord>(this TRecord record) where TRecord : IRecord
		{
			return UpdateAllAsync((IEnumerable<TRecord>)new[] { record });
		}

		/// <summary>
		/// Updates all columns of all existing database backing records based on the given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to update.</typeparam>
		/// <param name="records">Records to update in database.</param>
		public static void UpdateAll<TRecord>(this IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var count = KORMContext.Current.ORM.Update<TRecord>().Records(records);
			if (count != records.Count()) throw new ORMUpdateFailedException();
		}

		/// <summary>
		/// Asynchronous version of UpdateAll.
		/// Updates all columns of all existing database backing records based on the given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to update.</typeparam>
		/// <param name="records">Records to update in database.</param>
		public async static Task UpdateAllAsync<TRecord>(this IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var count = await KORMContext.Current.ORM.Update<TRecord>().RecordsAsync(records);
			if (count != records.Count()) throw new ORMUpdateFailedException();
		}

		/// <summary>
		/// Adds or updates a record in a database depending on whether it has primary key (ID) already assigned.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to store.</typeparam>
		/// <param name="record">Record to store in database.</param>
		public static void Store<TRecord>(this TRecord record) where TRecord : IRecord
		{
			if (record.ID == 0)
				Insert(record);
			else
				Update(record);
		}

		/// <summary>
		/// Asynchronous version of Store.
		/// Adds or updates a record in a database depending on whether it has primary key (ID) already assigned.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to store.</typeparam>
		/// <param name="record">Record to store in database.</param>
		public static Task StoreAsync<TRecord>(this TRecord record) where TRecord : IRecord
		{
			if (record.ID == 0)
				return InsertAsync(record);
			else
				return UpdateAsync(record);
		}

		/// <summary>
		/// Adds or updates records in database depending on whether they have primary key (ID) already assigned.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to store.</typeparam>
		/// <param name="records">Records to store in database.</param>
		public static void StoreAll<TRecord>(this IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var toInsert = records.Where(r => r.ID == 0).ToArray();
			var toUpdate = records.Where(r => r.ID != 0).ToArray();
			InsertAll(toInsert);
			UpdateAll(toUpdate);
		}

		/// <summary>
		/// Asynchronous version of StoreAll.
		/// Adds or updates records in database depending on whether they have primary key (ID) already assigned.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to store.</typeparam>
		/// <param name="records">Records to store in database.</param>
		public async static Task StoreAllAsync<TRecord>(this IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var toInsert = records.Where(r => r.ID == 0).ToArray();
			var toUpdate = records.Where(r => r.ID != 0).ToArray();
			await InsertAllAsync(toInsert);
			await UpdateAllAsync(toUpdate);
		}

		/// <summary>
		/// Deletes existing database backing record based on the primary key (ID) value of a given record.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to delete.</typeparam>
		/// <param name="record">Record to delete.</param>
		public static void Delete<TRecord>(this TRecord record) where TRecord : IRecord
		{
			DeleteAll((IEnumerable<TRecord>)new[] { record });
		}

		/// <summary>
		/// Asynchronous version of Delete.
		/// Deletes existing database backing record based on the primary key (ID) value of a given record.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to delete.</typeparam>
		/// <param name="record">Record to delete.</param>
		public static Task DeleteAsync<TRecord>(this TRecord record) where TRecord : IRecord
		{
			return DeleteAllAsync((IEnumerable<TRecord>)new[] { record });
		}

		/// <summary>
		/// Deletes all existing database records based on the primary keys (ID) values of a given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to delete.</typeparam>
		/// <param name="records">Record to delete.</param>
		public static void DeleteAll<TRecord>(this IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var count = KORMContext.Current.ORM.Delete<TRecord>().Records(records);
			if (count != records.Count()) throw new ORMDeleteFailedException();
		}

		/// <summary>
		/// Asynchronous version of DeleteAll.
		/// Deletes all existing database records based on the primary keys (ID) values of a given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to delete.</typeparam>
		/// <param name="records">Record to delete.</param>
		public async static Task DeleteAllAsync<TRecord>(this IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var count = await KORMContext.Current.ORM.Delete<TRecord>().RecordsAsync(records);
			if (count != records.Count()) throw new ORMDeleteFailedException();
		}

		/// <summary>
		/// Creates a new dictionary containing all provided records addressable by record reference.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to process.</typeparam>
		/// <param name="records">Records to create dictionary from.</param>
		/// <returns>Dictionary containing provided records.</returns>
		public static Dictionary<RecordRef<TRecord>, TRecord> ToDictionaryByRef<TRecord>(this IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var dictionary = new Dictionary<RecordRef<TRecord>, TRecord>();
			foreach (var record in records)
			{
				var recordref = (RecordRef<TRecord>)record;
				dictionary[recordref] = record;
			}
			return dictionary;
		}

		/// <summary>
		/// Creates a record reference to the record.
		/// </summary>
		/// <typeparam name="TRecord">Type of the record.</typeparam>
		/// <param name="record">Record to create reference to.</param>
		/// <returns>Reference to a record.</returns>
		public static RecordRef<TRecord> Ref<TRecord>(this TRecord record) where TRecord : IRecord
		{
			if (record == null) return default(RecordRef<TRecord>);
			return new RecordRef<TRecord>(record.ID);
		}

		/// <summary>
		/// Finds a record with a given ID in a given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of the record.</typeparam>
		/// <param name="records">Records to search.</param>
		/// <param name="id">Record ID value to search for.</param>
		/// <returns>Found record or null if no record matches given ID.</returns>
		public static TRecord ByID<TRecord>(this IEnumerable<TRecord> records, long id) where TRecord : IHasID
		{
			foreach (var record in records)
			{
				if (record.ID == id) return record;
			}
			return default(TRecord);
		}

		/// <summary>
		/// Finds a record matching a given reference in a given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of the record.</typeparam>
		/// <param name="records">Records to search.</param>
		/// <param name="recordRef">Record reference to match.</param>
		/// <returns>Found record or null if no record matches given reference.</returns>
		public static TRecord ByRef<TRecord>(this IEnumerable<TRecord> records, RecordRef<TRecord> recordRef) where TRecord : IRecord
		{
			return ByID<TRecord>(records, recordRef.ID);
		}

		/// <summary>
		/// Creates a semi-shallow copy of a given record. Inlines are copied; referenced records are kept same.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to copy.</typeparam>
		/// <param name="record">Record to copy.</param>
		/// <returns>Copy of a given record.</returns>
		public static TRecord Clone<TRecord>(this TRecord record) where TRecord : IRecord, new()
		{
			return (TRecord)CloneImpl(record);
		}

		private static object CloneImpl(object source)
		{
			if (source == null) return null;
			var type = source.GetType();
			var clone = KORMContext.Current.Factory.Create(type);
			var fields = type.Meta().Fields;
			foreach (var field in fields)
			{
				var value = field.Property.GetMethod.Invoke(source, null);
				if (field.IsInline)
				{
					value = CloneImpl(value);
				}
				field.Property.SetMethod.Invoke(clone, new[] { value });
			}
			return clone;
		}
	}
}
