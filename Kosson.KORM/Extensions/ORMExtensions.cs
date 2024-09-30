using Kosson.KORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for Kosson.KORM.IORM.
	/// </summary>
	public static class ORMExtensions
	{
		/// <summary>
		/// Adds the record to a database and assigns primary key (ID) to it.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to add.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="record">Record to add to database.</param>
		public static void Insert<TRecord>(this IORM orm, TRecord record) where TRecord : IRecord
			=> orm.InsertAll(new[] { record });

		/// <summary>
		/// Asynchronous version of Insert.
		/// Adds the record to a database and assigns primary key (ID) to it.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to add.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="record">Record to add to database.</param>
		public static Task InsertAsync<TRecord>(this IORM orm, TRecord record) where TRecord : IRecord
			=> orm.InsertAllAsync(new[] { record });

		/// <summary>
		/// Adds a set of records to a database and assigns primary key (ID) to all of them.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to add.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="records">Records to add to database.</param>
		public static void InsertAll<TRecord>(this IORM orm, IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var expectedCount = records.Count();
			if (expectedCount == 0) return;
			var actualCount = orm.Insert<TRecord>().Records(records);
			if (actualCount != expectedCount && records.First() is not IRecordNotifyInsert) throw new KORMInsertFailedException();
		}

		/// <summary>
		/// Asynchronous version of Insert.
		/// Adds a set of records to a database and assigns primary key (ID) to all of them.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to add.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="records">Records to add to database.</param>
		public async static Task InsertAllAsync<TRecord>(this IORM orm, IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var expectedCount = records.Count();
			if (expectedCount == 0) return;
			var actualCount = await orm.Insert<TRecord>().RecordsAsync(records);
			if (actualCount != expectedCount && records.First() is not IRecordNotifyInsert) throw new KORMInsertFailedException();
		}

		/// <summary>
		/// Updates all columns of exising database backing record based on the given record.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to update.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="record">Record to update in database.</param>
		public static void Update<TRecord>(this IORM orm, TRecord record) where TRecord : IRecord
			=> orm.UpdateAll(new[] { record });

		/// <summary>
		/// Asynchronous version of Update.
		/// Updates all columns of exising database backing record based on the given record.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to update.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="record">Record to update in database.</param>
		public static Task UpdateAsync<TRecord>(this IORM orm, TRecord record) where TRecord : IRecord
			=> orm.UpdateAllAsync(new[] { record });

		/// <summary>
		/// Updates all columns of all existing database backing records based on the given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to update.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="records">Records to update in database.</param>
		public static void UpdateAll<TRecord>(this IORM orm, IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var expectedCount = records.Count();
			if (expectedCount == 0) return;
			var actualCount = orm.Update<TRecord>().Records(records);
			if (actualCount != expectedCount && records.First() is not IRecordNotifyUpdate) throw new KORMUpdateFailedException();
		}

		/// <summary>
		/// Asynchronous version of UpdateAll.
		/// Updates all columns of all existing database backing records based on the given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to update.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="records">Records to update in database.</param>
		public async static Task UpdateAllAsync<TRecord>(this IORM orm, IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var expectedCount = records.Count();
			if (expectedCount == 0) return;
			var actualCount = await orm.Update<TRecord>().RecordsAsync(records);
			if (actualCount != expectedCount && records.First() is not IRecordNotifyUpdate) throw new KORMUpdateFailedException();
		}

		/// <summary>
		/// Adds or updates a record in a database depending on whether it has primary key (ID) already assigned.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to store.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="record">Record to store in database.</param>
		public static void Store<TRecord>(this IORM orm, TRecord record) where TRecord : IRecord
		{
			if (record.ID == 0) orm.Insert(record);
			else orm.Update(record);
		}

		/// <summary>
		/// Asynchronous version of Store.
		/// Adds or updates a record in a database depending on whether it has primary key (ID) already assigned.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to store.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="record">Record to store in database.</param>
		public static Task StoreAsync<TRecord>(this IORM orm, TRecord record) where TRecord : IRecord
		{
			if (record.ID == 0) return orm.InsertAsync(record);
			else return orm.UpdateAsync(record);
		}

		/// <summary>
		/// Adds or updates records in database depending on whether they have primary key (ID) already assigned.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to store.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="records">Records to store in database.</param>
		public static void StoreAll<TRecord>(this IORM orm, IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var toInsert = records.Where(r => r.ID == 0).ToList();
			var toUpdate = records.Where(r => r.ID != 0).ToList();
			orm.InsertAll(toInsert);
			orm.UpdateAll(toUpdate);
		}

		/// <summary>
		/// Asynchronous version of StoreAll.
		/// Adds or updates records in database depending on whether they have primary key (ID) already assigned.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to store.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="records">Records to store in database.</param>
		public async static Task StoreAllAsync<TRecord>(this IORM orm, IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var toInsert = records.Where(r => r.ID == 0).ToList();
			var toUpdate = records.Where(r => r.ID != 0).ToList();
			await orm.InsertAllAsync(toInsert);
			await orm.UpdateAllAsync(toUpdate);
		}

		/// <summary>
		/// Deletes existing database backing record based on the primary key (ID) value of a given record.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to delete.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="record">Record to delete.</param>
		public static void Delete<TRecord>(this IORM orm, TRecord record) where TRecord : IRecord
			=> orm.DeleteAll(new[] { record });

		/// <summary>
		/// Asynchronous version of Delete.
		/// Deletes existing database backing record based on the primary key (ID) value of a given record.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to delete.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="record">Record to delete.</param>
		public static Task DeleteAsync<TRecord>(this IORM orm, TRecord record) where TRecord : IRecord
			=> orm.DeleteAllAsync(new[] { record });

		/// <summary>
		/// Deletes all existing database records based on the primary keys (ID) values of a given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to delete.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="records">Record to delete.</param>
		public static void DeleteAll<TRecord>(this IORM orm, IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var expectedCount = records.Count();
			if (expectedCount == 0) return;
			var actualCount = orm.Delete<TRecord>().Records(records);
			if (actualCount != expectedCount && records.First() is not IRecordNotifyDelete) throw new KORMDeleteFailedException();
		}

		/// <summary>
		/// Asynchronous version of DeleteAll.
		/// Deletes all existing database records based on the primary keys (ID) values of a given records.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to delete.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="records">Record to delete.</param>
		public async static Task DeleteAllAsync<TRecord>(this IORM orm, IEnumerable<TRecord> records) where TRecord : IRecord
		{
			var count = await orm.Delete<TRecord>().RecordsAsync(records);
			if (count != records.Count() && records.First() is not IRecordNotifyDelete) throw new KORMDeleteFailedException();
		}

		/// <summary>
		/// Retrieves a record for a given record reference.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="recordref">Primary key (ID) reference to a record to retrieve.</param>
		/// <returns>Record for a given record reference.</returns>
		public static T? Get<T>(this IORM orm, RecordRef<T> recordref) where T : class, IRecord, new()
			=> orm.Select<T>().ByRef(recordref);

		/// <summary>
		/// Asynchronous version of Get.
		/// Retrieves a record for a given record reference.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="recordref">Primary key (ID) reference to a record to retrieve.</param>
		/// <returns>Task representing asynchronous operation returning record for a given record reference.</returns>
		public static Task<T?> GetAsync<T>(this IORM orm, RecordRef<T> recordref) where T : class, IRecord, new()
			=> orm.Select<T>().ByRefAsync(recordref);

		/// <summary>
		/// Retrieves all records matching all provided conditions.
		/// </summary>
		/// <typeparam name="T">Type of records to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="predicates">Set of predicates determining which records to retrieve.</param>
		/// <returns>Records matching all provided conditions.</returns>
		public static IReadOnlyCollection<T> Get<T>(this IORM orm, params Expression<Func<T, bool>>[] predicates) where T : class, IRecord, new()
			=> orm.Select<T>().WhereAll(predicates).Execute();

		/// <summary>
		/// Asynchronous version of Get.
		/// Retrieves all records matching all provided conditions.
		/// </summary>
		/// <typeparam name="T">Type of records to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="predicates">Set of predicates determining which records to retrieve.</param>
		/// <returns>Records matching all provided conditions.</returns>
		public static Task<IReadOnlyCollection<T>> GetAsync<T>(this IORM orm, params Expression<Func<T, bool>>[] predicates) where T : class, IRecord, new()
			=> orm.Select<T>().WhereAll(predicates).ExecuteAsync();

		/// <summary>
		/// Retrieves all records matching provided SQL condition.
		/// </summary>
		/// <typeparam name="T">Type of records to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="where">Expression to use as WHERE clause in SQL SELECT command.</param>
		/// <returns>Records matching provided condition.</returns>
		public static IReadOnlyCollection<T> Get<T>(this IORM orm, FormattableString where) where T : class, IRecord, new()
			=> orm.Select<T>().Where(where).Execute();

		/// <summary>
		/// Asynchronous version of Get.
		/// Retrieves all records matching provided SQL condition.
		/// </summary>
		/// <typeparam name="T">Type of records to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="where">Expression to use as WHERE clause in SQL SELECT command.</param>
		/// <returns>Records matching provided condition.</returns>
		public static Task<IReadOnlyCollection<T>> GetAsync<T>(this IORM orm, FormattableString where) where T : class, IRecord, new()
			=> orm.Select<T>().Where(where).ExecuteAsync();

		/// <summary>
		/// Retrieves records for a given record references.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="recordRefs">Primary key (ID) references to records to retrieve.</param>
		/// <returns>Records for a given record references.</returns>
		public static IReadOnlyCollection<T> Get<T>(this IORM orm, IEnumerable<RecordRef<T>> recordRefs) where T : class, IRecord, new()
			=> orm.Select<T>().ByRefs(recordRefs);

		/// <summary>
		/// Asynchronous version of Get.
		/// Retrieves records for a given record references.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="recordRefs">Primary key (ID) references to records to retrieve.</param>
		/// <returns>Task representing records for a given record references.</returns>
		public static Task<IReadOnlyCollection<T>> GetAsync<T>(this IORM orm, IEnumerable<RecordRef<T>> recordRefs) where T : class, IRecord, new()
			=> orm.Select<T>().ByRefsAsync(recordRefs);

		/// <summary>
		/// Retrieves first record matching all provided conditions.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="predicates">Set of predicates determining which record to retrieve.</param>
		/// <returns>First record matching provided conditions.</returns>
		public static T? GetFirst<T>(this IORM orm, params Expression<Func<T, bool>>[] predicates) where T : class, IRecord, new()
			=> orm.Select<T>().WhereAll(predicates).ExecuteFirst();

		/// <summary>
		/// Asynchronous version of GetFirst.
		/// Retrieves first record matching all provided conditions.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="predicates">Set of predicates determining which record to retrieve.</param>
		/// <returns>First record matching provided conditions.</returns>
		public static Task<T?> GetFirstAsync<T>(this IORM orm, params Expression<Func<T, bool>>[] predicates) where T : class, IRecord, new()
			=> orm.Select<T>().WhereAll(predicates).ExecuteFirstAsync();

		/// <summary>
		/// Retrieves first record matching provided SQL condition.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="where">Expression to use as WHERE clause in SQL SELECT command.</param>
		/// <returns>First record matching provided condition.</returns>
		public static T? GetFirst<T>(this IORM orm, FormattableString where) where T : class, IRecord, new()
			=> orm.Select<T>().Where(where).ExecuteFirst();

		/// <summary>
		/// Asynchronous version of GetFirst.
		/// Retrieves first record matching provided SQL condition.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="where">Expression to use as WHERE clause in SQL SELECT command.</param>
		/// <returns>First record matching provided condition.</returns>
		public static Task<T?> GetFirstAsync<T>(this IORM orm, FormattableString where) where T : class, IRecord, new()
			=> orm.Select<T>().Where(where).ExecuteFirstAsync();

		/// <summary>
		/// Acquires a database lock for given record reference and retrieves associated record.
		/// </summary>
		/// <typeparam name="T">Type of record to lock and retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="recordRef">Primary key (ID) reference to a record to lock and retrieve.</param>
		/// <returns>Retrieved record</returns>
		public static T? Lock<T>(this IORM orm, RecordRef<T> recordRef) where T : class, IRecord, new()
			=> orm.Select<T>().ForUpdate().ByRef(recordRef);

		/// <summary>
		/// Asynchronous version of Lock.
		/// Acquires a database lock for given record reference and retrieves associated record.
		/// </summary>
		/// <typeparam name="T">Type of record to lock and retrieve.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="recordRef">Primary key (ID) reference to a record to lock and retrieve.</param>
		/// <returns>Retrieved record</returns>
		public static Task<T?> LockAsync<T>(this IORM orm, RecordRef<T> recordRef) where T : class, IRecord, new()
			=> orm.Select<T>().ForUpdate().ByRefAsync(recordRef);

		/// <summary>
		/// Counts database records matching specified predicates.
		/// </summary>
		/// <typeparam name="T">Type of record to count.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="predicates">Set of predicates determining which record to count.</param>
		/// <returns>Number of matching records</returns>
		public static int Count<T>(this IORM orm, params Expression<Func<T, bool>>[] predicates) where T : class, IRecord, new()
			=> orm.Select<T>().WhereAll(predicates).ExecuteCount();

		/// <summary>
		/// Asynchronous version of Count.
		/// Counts database records matching specified predicates.
		/// </summary>
		/// <typeparam name="T">Type of record to count.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="predicates">Set of predicates determining which record to count.</param>
		/// <returns>Number of matching records</returns>
		public static Task<int> CountAsync<T>(this IORM orm, params Expression<Func<T, bool>>[] predicates) where T : class, IRecord, new()
			=> orm.Select<T>().WhereAll(predicates).ExecuteCountAsync();

		/// <summary>
		/// Counts database records matching specified SQL condition.
		/// </summary>
		/// <typeparam name="T">Type of record to count.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="where">Expression to use as WHERE clause in SQL SELECT command.</param>
		/// <returns>Number of matching records</returns>
		public static int Count<T>(this IORM orm, FormattableString where) where T : class, IRecord, new()
			=> orm.Select<T>().Where(where).ExecuteCount();

		/// <summary>
		/// Asynchronous version of Count.
		/// Counts database records matching specified SQL condition.
		/// </summary>
		/// <typeparam name="T">Type of record to count.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="where">Expression to use as WHERE clause in SQL SELECT command.</param>
		/// <returns>Number of matching records</returns>
		public static Task<int> CountAsync<T>(this IORM orm, FormattableString where) where T : class, IRecord, new()
			=> orm.Select<T>().Where(where).ExecuteCountAsync();

		/// <summary>
		/// Checks whether there exist a record matching specified predicates.
		/// </summary>
		/// <typeparam name="T">Type of record to look for.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="predicates">Set of predicates determining which record to count.</param>
		/// <returns>True if at least one record matches the condition.</returns>
		public static bool Exists<T>(this IORM orm, params Expression<Func<T, bool>>[] predicates) where T : class, IRecord, new()
			=> orm.Select<T>().WhereAll(predicates).Select(e => e.ID).ExecuteFirst() > 0;

		/// <summary>
		/// Asynchronous version of Exists.
		/// Checks whether there exist a record matching specified predicates.
		/// </summary>
		/// <typeparam name="T">Type of record to look for.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="predicates">Set of predicates determining which record to count.</param>
		/// <returns>True if at least one record matches the condition.</returns>
		public async static Task<bool> ExistsAsync<T>(this IORM orm, params Expression<Func<T, bool>>[] predicates) where T : class, IRecord, new()
			=> (await orm.Select<T>().WhereAll(predicates).Select(e => e.ID).ExecuteFirstAsync()) > 0;

		/// <summary>
		/// Checks whether there exist a record matching specified SQL condition.
		/// </summary>
		/// <typeparam name="T">Type of record to look for.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="where">Expression to use as WHERE clause in SQL SELECT command.</param>
		/// <returns>True if at least one record matches the condition.</returns>
		public static bool Exists<T>(this IORM orm, FormattableString where) where T : class, IRecord, new()
			=> orm.Select<T>().Where(where).Select(e => e.ID).ExecuteFirst() > 0;

		/// <summary>
		/// Asynchronous version of Exists.
		/// Checks whether there exist a record matching specified SQL condition.
		/// </summary>
		/// <typeparam name="T">Type of record to look for.</typeparam>
		/// <param name="orm">ORM instance.</param>
		/// <param name="where">Expression to use as WHERE clause in SQL SELECT command.</param>
		/// <returns>True if at least one record matches the condition.</returns>
		public async static Task<bool> ExistsAsync<T>(this IORM orm, FormattableString where) where T : class, IRecord, new()
			=> (await orm.Select<T>().Where(where).Select(e => e.ID).ExecuteFirstAsync()) > 0;
	}
}
