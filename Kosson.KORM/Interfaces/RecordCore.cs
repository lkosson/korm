using System;
using System.Linq;

namespace Kosson.KORM
{
	/// <summary>
	/// Base type for IRecord providing equality comparison based on primary key (ID) property.
	/// </summary>
	[Serializable]
	public abstract class RecordCore : IRecord, IConvertible
	{
		internal abstract long FullRangeID { get; set; }

		long IRecord.ID { get => FullRangeID; set => FullRangeID = value; }
		long IHasID.ID => FullRangeID;

		/// <summary>
		/// Determines whether this record is of same type and has same primary key (ID) as a given record.
		/// </summary>
		/// <param name="obj">Record to compare this record to.</param>
		/// <returns>True if given record is of same type and has same primary key (ID) as this record.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			var otherObj = obj as RecordCore;
			if (otherObj != null)
			{
				if (!otherObj.GetType().IsAssignableFrom(GetType()) && !GetType().IsAssignableFrom(otherObj.GetType())) return false;
				return otherObj.FullRangeID == FullRangeID;
			}

			var otherRef = obj as IRecordRef;
			if (otherRef != null)
			{
				var refType = otherRef.GetType().GetGenericArguments().FirstOrDefault();
				if (refType == null || (!refType.IsAssignableFrom(GetType()) && !GetType().IsAssignableFrom(refType))) return false;
				return otherRef.ID == FullRangeID;
			}

			var otherID = obj as IHasID;
			if (otherID != null) return otherID.ID == FullRangeID;
			return false;
		}

		/// <summary>
		/// Returns hash code of primary key (ID) of the record.
		/// </summary>
		/// <returns>Primary key (ID) of the record hash code.</returns>
		public override int GetHashCode()
		{
			return FullRangeID.GetHashCode();
		}

		/// <summary>
		/// Tests for equality between primary keys (ID) two given records of same type.
		/// </summary>
		/// <param name="r1">First record to compare.</param>
		/// <param name="r2">Second record to compare.</param>
		/// <returns>True if both records are of the same type and have same primary key (ID) value.</returns>
		public static bool operator ==(RecordCore r1, RecordCore r2)
		{
			if (ReferenceEquals(r1, null)) return ReferenceEquals(r2, null);
			return r1.Equals(r2);
		}

		/// <summary>
		/// Tests for inequality between primary keys (ID) two given records of same type.
		/// </summary>
		/// <param name="r1">First record to compare.</param>
		/// <param name="r2">Second record to compare.</param>
		/// <returns>True if records are of different types or have different primary key (ID) values.</returns>
		public static bool operator !=(RecordCore r1, RecordCore r2)
		{
			return !(r1 == r2);
		}

		#region IConvertible implementation
		TypeCode IConvertible.GetTypeCode()
		{
			return TypeCode.Object;
		}

		private T ThrowInvalidCast<T>()
		{
			ThrowInvalidCast(typeof(T));
			return default(T);
		}

		private void ThrowInvalidCast(Type type)
		{
			throw new InvalidCastException("Record \"" + this + "\" cannot be converted to type " + type + ".");
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return ThrowInvalidCast<bool>();
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return ThrowInvalidCast<byte>();
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return ThrowInvalidCast<char>();
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return ThrowInvalidCast<DateTime>();
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return ThrowInvalidCast<decimal>();
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return ThrowInvalidCast<double>();
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return (short)FullRangeID;
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return (int)FullRangeID;
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return FullRangeID;
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return (sbyte)FullRangeID;
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return ThrowInvalidCast<float>();
		}

		string IConvertible.ToString(IFormatProvider provider)
		{
			return ToString();
		}

		object IConvertible.ToType(Type type, IFormatProvider provider)
		{
			//if (typeof(IRecordRef).IsAssignableFrom(type))
			//{
			//	var recordref = (IRecordRef)KORMContext.Current.Factory.Create(type);
			//	// recordref is boxed struct
			//	recordref.ID = ID;
			//	return recordref;
			//}
			ThrowInvalidCast(type);
			return null;
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return (ushort)FullRangeID;
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return (uint)FullRangeID;
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return (ulong)FullRangeID;
		}
		#endregion
	}
}
