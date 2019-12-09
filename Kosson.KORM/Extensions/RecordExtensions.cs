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
		public static TRecord Clone<TRecord>(this IRecordCloner cloner, TRecord record) where TRecord : IRecord, new()
		{
			return (TRecord)cloner.Clone(record);
		}
	}
}
