using System;
using System.Linq;

namespace Kosson.KORM
{
	/// <summary>
	/// Base type for IRecord providing equality comparison based on 8-bit primary key (ID) property.
	/// </summary>
	[Serializable]
	public class Record8 : RecordCore, IConvertible
	{
		/// <summary>
		/// Primary key of the record.
		/// </summary>
		[Column]
		public byte ID { get; set; }

		internal override long FullRangeID { get => ID; set => ID = checked((byte)value); }
	}
}
