using System;
using System.Linq;

namespace Kosson.KORM
{
	/// <summary>
	/// Base type for IRecord providing equality comparison based on 64-bit primary key (ID) property.
	/// </summary>
	[Serializable]
	public class Record64 : RecordCore, IConvertible
	{
		/// <summary>
		/// Primary key of the record.
		/// </summary>
		[Column]
		public long ID { get; set; }

		internal override long FullRangeID { get => ID; set => ID = value; }
	}
}
