using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IRecord.
	/// </summary>
	public static class RecordExtensions
	{
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

		private static IEnumerable<TResult> Join<TRecordLocal, TRecordForeign, TResult>(this IEnumerable<TRecordLocal> records, IORMSelect<TRecordForeign> foreignRecordSelect, Func<TRecordLocal, RecordRef<TRecordForeign>> foreignKeySelector, Func<TRecordLocal, TRecordForeign, TResult> resultConstructor) where TRecordForeign : IRecord
		{
			var foreignRefs = records.Select(foreignKeySelector).Distinct().ToList();
			var foreignRecords = foreignRecordSelect.ByRefs(foreignRefs);
			var foreignLookup = foreignRecords.ToDictionaryByRef();
			var joinedRecords = records.Select(record1 => foreignLookup.TryGetValue(foreignKeySelector(record1), out var record2) ? resultConstructor(record1, record2) : resultConstructor(record1, default));
			return joinedRecords;
		}

		private static async Task<IEnumerable<TResult>> JoinAsync<TRecordLocal, TRecordForeign, TResult>(this IEnumerable<TRecordLocal> records, IORMSelect<TRecordForeign> foreignRecordSelect, Func<TRecordLocal, RecordRef<TRecordForeign>> foreignKeySelector, Func<TRecordLocal, TRecordForeign, TResult> resultConstructor) where TRecordForeign : IRecord
		{
			var foreignRefs = records.Select(foreignKeySelector).Distinct().ToList();
			var foreignRecords = await foreignRecordSelect.ByRefsAsync(foreignRefs);
			var foreignLookup = foreignRecords.ToDictionaryByRef();
			var joinedRecords = records.Select(record1 => foreignLookup.TryGetValue(foreignKeySelector(record1), out var record2) ? resultConstructor(record1, record2) : resultConstructor(record1, default));
			return joinedRecords;
		}

		/// <summary>
		/// Performs in-memory left outer join operation by fetching foreign records based on values found in provided records using a given SELECT query.
		/// </summary>
		/// <param name="records">Local records</param>
		/// <param name="foreignRecordSelect">Base SELECT query to use for fetching foreign records.</param>
		/// <param name="foreignKeySelector">Foreign key reference</param>
		/// <returns>Tuples of joined local and remote records</returns>
		public static IEnumerable<(TRecordLocal, TRecordForeign)> Join<TRecordLocal, TRecordForeign>(this IEnumerable<TRecordLocal> records, IORMSelect<TRecordForeign> foreignRecordSelect, Func<TRecordLocal, RecordRef<TRecordForeign>> foreignKeySelector) 
			where TRecordForeign : IRecord 
			=> Join(records, foreignRecordSelect, foreignKeySelector, (record1, record2) => (record1, record2));

		/// <summary>
		/// Performs in-memory left outer join operation by fetching foreign records based on values found in provided records using a given SELECT query.
		/// </summary>
		/// <param name="records">Local records</param>
		/// <param name="foreignRecordSelect">Base SELECT query to use for fetching foreign records.</param>
		/// <param name="foreignKeySelector">Foreign key reference</param>
		/// <returns>Tuples of joined local and remote records</returns>
		public static IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordForeign)> Join<TRecordLocal1, TRecordLocal2, TRecordForeign>(this IEnumerable<(TRecordLocal1, TRecordLocal2)> tuples, IORMSelect<TRecordForeign> foreignRecordSelect, Func<(TRecordLocal1, TRecordLocal2), RecordRef<TRecordForeign>> foreignKeySelector) 
			where TRecordForeign : IRecord
			=> Join(tuples, foreignRecordSelect, foreignKeySelector, (tuple, record) => (tuple.Item1, tuple.Item2, record));

		/// <summary>
		/// Performs in-memory left outer join operation by fetching foreign records based on values found in provided records using a given SELECT query.
		/// </summary>
		/// <param name="records">Local records</param>
		/// <param name="foreignRecordSelect">Base SELECT query to use for fetching foreign records.</param>
		/// <param name="foreignKeySelector">Foreign key reference</param>
		/// <returns>Tuples of joined local and remote records</returns>
		public static IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordForeign)> Join<TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordForeign>(this IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordLocal3)> tuples, IORMSelect<TRecordForeign> foreignRecordSelect, Func<(TRecordLocal1, TRecordLocal2, TRecordLocal3), RecordRef<TRecordForeign>> foreignKeySelector)
			where TRecordForeign : IRecord
			=> Join(tuples, foreignRecordSelect, foreignKeySelector, (tuple, record) => (tuple.Item1, tuple.Item2, tuple.Item3, record));

		/// <summary>
		/// Performs in-memory left outer join operation by fetching foreign records based on values found in provided records using a given SELECT query.
		/// </summary>
		/// <param name="records">Local records</param>
		/// <param name="foreignRecordSelect">Base SELECT query to use for fetching foreign records.</param>
		/// <param name="foreignKeySelector">Foreign key reference</param>
		/// <returns>Tuples of joined local and remote records</returns>
		public static IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordLocal4, TRecordForeign)> Join<TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordLocal4, TRecordForeign>(this IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordLocal4)> tuples, IORMSelect<TRecordForeign> foreignRecordSelect, Func<(TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordLocal4), RecordRef<TRecordForeign>> foreignKeySelector)
			where TRecordForeign : IRecord
			=> Join(tuples, foreignRecordSelect, foreignKeySelector, (tuple, record) => (tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, record));

		/// <summary>
		/// Performs in-memory left outer join operation by fetching foreign records based on values found in provided records using a given SELECT query.
		/// </summary>
		/// <param name="records">Local records</param>
		/// <param name="foreignRecordSelect">Base SELECT query to use for fetching foreign records.</param>
		/// <param name="foreignKeySelector">Foreign key reference</param>
		/// <returns>Tuples of joined local and remote records</returns>
		public static Task<IEnumerable<(TRecordLocal, TRecordForeign)>> JoinAsync<TRecordLocal, TRecordForeign>(this IEnumerable<TRecordLocal> records, IORMSelect<TRecordForeign> foreignRecordSelect, Func<TRecordLocal, RecordRef<TRecordForeign>> foreignKeySelector)
			where TRecordForeign : IRecord
			=> JoinAsync(records, foreignRecordSelect, foreignKeySelector, (record1, record2) => (record1, record2));

		/// <summary>
		/// Performs in-memory left outer join operation by fetching foreign records based on values found in provided records using a given SELECT query.
		/// </summary>
		/// <param name="records">Local records</param>
		/// <param name="foreignRecordSelect">Base SELECT query to use for fetching foreign records.</param>
		/// <param name="foreignKeySelector">Foreign key reference</param>
		/// <returns>Tuples of joined local and remote records</returns>
		public static Task<IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordForeign)>> JoinAsync<TRecordLocal1, TRecordLocal2, TRecordForeign>(this IEnumerable<(TRecordLocal1, TRecordLocal2)> tuples, IORMSelect<TRecordForeign> foreignRecordSelect, Func<(TRecordLocal1, TRecordLocal2), RecordRef<TRecordForeign>> foreignKeySelector)
			where TRecordForeign : IRecord
			=> JoinAsync(tuples, foreignRecordSelect, foreignKeySelector, (tuple, record) => (tuple.Item1, tuple.Item2, record));

		/// <summary>
		/// Performs in-memory left outer join operation by fetching foreign records based on values found in provided records using a given SELECT query.
		/// </summary>
		/// <param name="records">Local records</param>
		/// <param name="foreignRecordSelect">Base SELECT query to use for fetching foreign records.</param>
		/// <param name="foreignKeySelector">Foreign key reference</param>
		/// <returns>Tuples of joined local and remote records</returns>
		public static Task<IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordForeign)>> JoinAsync<TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordForeign>(this IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordLocal3)> tuples, IORMSelect<TRecordForeign> foreignRecordSelect, Func<(TRecordLocal1, TRecordLocal2, TRecordLocal3), RecordRef<TRecordForeign>> foreignKeySelector)
			where TRecordForeign : IRecord
			=> JoinAsync(tuples, foreignRecordSelect, foreignKeySelector, (tuple, record) => (tuple.Item1, tuple.Item2, tuple.Item3, record));

		/// <summary>
		/// Performs in-memory left outer join operation by fetching foreign records based on values found in provided records using a given SELECT query.
		/// </summary>
		/// <param name="records">Local records</param>
		/// <param name="foreignRecordSelect">Base SELECT query to use for fetching foreign records.</param>
		/// <param name="foreignKeySelector">Foreign key reference</param>
		/// <returns>Tuples of joined local and remote records</returns>
		public static Task<IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordLocal4, TRecordForeign)>> JoinAsync<TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordLocal4, TRecordForeign>(this IEnumerable<(TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordLocal4)> tuples, IORMSelect<TRecordForeign> foreignRecordSelect, Func<(TRecordLocal1, TRecordLocal2, TRecordLocal3, TRecordLocal4), RecordRef<TRecordForeign>> foreignKeySelector)
			where TRecordForeign : IRecord
			=> JoinAsync(tuples, foreignRecordSelect, foreignKeySelector, (tuple, record) => (tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, record));

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
			return default;
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
		/// <param name="cloner">Implementation of IRecordCloner to use.</param>
		/// <param name="record">Record to copy.</param>
		/// <returns>Copy of a given record.</returns>
		public static TRecord Clone<TRecord>(this IRecordCloner cloner, TRecord record) where TRecord : IRecord, new()
		{
			return cloner.Clone(record);
		}

		/// <summary>
		/// Converts a record to string representation containing values of all database fields.
		/// </summary>
		/// <typeparam name="TRecord">Type of the record.</typeparam>
		/// <param name="record">The record to convert.</param>
		/// <param name="meta">Record metadata.</param>
		/// <returns>Record string representation.</returns>
		public static string ToStringByFields<TRecord>(this TRecord record, IMetaRecord meta) where TRecord : IRecord
		{
			var sb = new StringBuilder();
			AppendFields(sb, record, meta);
			return sb.ToString();
		}

		private static void AppendFields(StringBuilder sb, object record, IMetaRecord meta)
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly && !field.IsPrimaryKey) continue;
				var value = field.Property.GetValue(record);
				if (field.IsInline)
				{
					AppendFields(sb, value, field.InlineRecord);
				}
				else
				{
					if (sb.Length > 0) sb.Append(", ");
					sb.Append(field.Name);
					sb.Append("=");
					if (value == null)
					{
						sb.Append("null");
					}
					else
					{
						if (field.Type == typeof(string)) sb.Append("\"");
						sb.Append(value);
						if (field.Type == typeof(string)) sb.Append("\"");
					}
				}
			}
		}
	}
}
