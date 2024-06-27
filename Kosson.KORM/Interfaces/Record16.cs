using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Base type for IRecord providing equality comparison based on 16-bit primary key (ID) property.
	/// </summary>
	[Serializable]
	public class Record16 : RecordCore, IConvertible
	{
		/// <summary>
		/// Primary key of the record.
		/// </summary>
		[Column]
		public short ID { get; set; }

		internal override long FullRangeID { get => ID; set => ID = checked((short)value); }
	}
}
