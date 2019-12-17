using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Untyped reference to a record.
	/// </summary>
	public interface IRecordRef : IHasID
	{
		/// <summary>
		/// Primary key of referenced record.
		/// </summary>
		new long ID { get; set; }

		/// <summary>
		/// Type of the referenced record.
		/// </summary>
		Type RecordType { get; }
	}

	/// <summary>
	/// Typed reference to a record.
	/// </summary>
	/// <typeparam name="T">Type of referenced record.</typeparam>
	public interface IRecordRef<T> : IRecordRef where T : IRecord
	{
	}
}
