﻿#nullable enable
using System;
using System.ComponentModel;

namespace Kosson.KORM
{
	/// <summary>
	/// Typed reference to a record.
	/// </summary>
	/// <typeparam name="T">Type of referenced record</typeparam>
	[Serializable]
	[TypeConverter(typeof(RecordRefTypeConverter))]
	public struct RecordRef<T> : IRecordRef<T>, IConvertible where T : IRecord
	{
		/// <summary>
		/// Primary key of referenced record.
		/// </summary>
		public long ID { get; set; }

		/// <summary>
		/// Type of the referenced record.
		/// </summary>
		public Type RecordType { get { return typeof(T); } }

		/// <summary>
		/// Determines whether there is no referenced record.
		/// </summary>
		public readonly bool IsNull { get { return ID == 0; } }

		/// <summary>
		/// Determines whether there is a referenced record.
		/// </summary>
		public readonly bool IsNotNull { get { return !IsNull; } }

		/// <summary>
		/// Primary key of referenced record as nullable value.
		/// </summary>
		public long? IDOrNull { readonly get { if (IsNull) return null; return ID; } set { ID = value.GetValueOrDefault(); } }

		/// <summary>
		/// Creates a new record reference from a given primary key (ID) value.
		/// </summary>
		/// <param name="id">Primary key (ID) of a record to create reference to.</param>
		public RecordRef(long id)
		{
			this.ID = id;
		}

		/// <summary>
		/// Creates a new record reference to a given record.
		/// </summary>
		/// <param name="record">Record to create reference to.</param>
		public RecordRef(T record)
		{
			this.ID = record.ID;
		}

		/// <summary>
		/// Tests whether this reference references same record as a given reference.
		/// </summary>
		/// <param name="obj">Record reference to compare to.</param>
		/// <returns>True if given record reference references record of a same type and same primary key (ID) value.</returns>
		public override bool Equals(object? obj)
		{
			if (obj is not IRecordRef) return false;
			var other = (IRecordRef)obj;
			return other.ID == ID && (other.RecordType == RecordType || other.RecordType.IsAssignableFrom(RecordType) || RecordType.IsAssignableFrom(other.RecordType));
		}

		/// <summary>
		/// Tests is given record references reference same record.
		/// </summary>
		/// <param name="rr1">First reference to compare.</param>
		/// <param name="rr2">Second reference to compare.</param>
		/// <returns>True if both record references reference same record.</returns>
		public static bool operator ==(RecordRef<T> rr1, RecordRef<T> rr2)
		{
			// type equality forced by signature
			return rr1.ID == rr2.ID;
		}

		/// <summary>
		/// Tests is given record references reference different records.
		/// </summary>
		/// <param name="rr1">First reference to compare.</param>
		/// <param name="rr2">Second reference to compare.</param>
		/// <returns>True if record references reference different records.</returns>
		public static bool operator !=(RecordRef<T> rr1, RecordRef<T> rr2)
		{
			// type equality forced by signature
			return rr1.ID != rr2.ID;
		}

		/*
		 * RecordRef<T> == RecordRef<T> also handles:
		 *  - RecordRef<T> == long and long == RecordRef<T> by implicit conversion from long to RecordRef<T>
		 *  - RecordRef<T> == T by implicit conversion from T to RecordRef<T>
		 *  - RecordRef<T> == null and null == RecordRef<T> by implicit conversion from null to T and from T to RecordRef<T>
		 */

		/// <summary>
		/// Converts given record reference to 64-bit integer.
		/// </summary>
		/// <param name="r">Record reference to convert.</param>
		/// <returns>64-bit integer of record primary key (ID).</returns>
		public static explicit operator long(RecordRef<T> r)
		{
			return r.ID;
		}

		/// <summary>
		/// Converts given 64-bit integer primary key value to record reference.
		/// </summary>
		/// <param name="id">Primary key (ID) value.</param>
		/// <returns>Reference to a record.</returns>
		public static implicit operator RecordRef<T>(long id)
		{
			return new RecordRef<T>(id);
		}

		/// <summary>
		/// Converts given record to a record reference.
		/// </summary>
		/// <param name="record">Record to convert.</param>
		/// <returns>Reference to a record.</returns>
		public static implicit operator RecordRef<T>(T? record)
		{
			return new RecordRef<T>(record == null ? 0 : record.ID);
		}

		/// <summary>
		/// Returns hash code of the record reference.
		/// </summary>
		/// <returns>Record reference hash code.</returns>
		public override readonly int GetHashCode()
		{
			return ID.GetHashCode();
		}

		/// <summary>
		/// Converts record reference to its string representation.
		/// </summary>
		/// <returns>String representation of a record reference.</returns>
		public override readonly string ToString()
		{
			return typeof(T).Name + "@" + ID;
		}

		/// <summary>
		/// Parses a string to a RecordRef.
		/// </summary>
		/// <param name="value">String to parse</param>
		/// <returns>Parsed RecordRef</returns>
		/// <exception cref="FormatException"></exception>
		public static RecordRef<T> Parse(string value)
		{
			var at = value.IndexOf('@');
			if (at > 0)
			{
				var providedType = value.Substring(0, at);
				var expectedType = typeof(T).Name;
				if (providedType != expectedType) throw new FormatException($"Invalid RecordRef type, expected \"{expectedType}\", got \"{providedType}\".");
				value = value.Substring(at + 1);
			}
			if (!Int64.TryParse(value, out long id)) throw new FormatException($"Invalid RecordRef value.");
			return new RecordRef<T>(id);
		}

		/// <summary>
		/// Attempts to parse provided value as a RecordRef.
		/// </summary>
		/// <param name="value">String to parse</param>
		/// <param name="recordRef">Parsed RecordRef</param>
		/// <returns>true is parsing was successful</returns>
		public static bool TryParse(string value, out RecordRef<T> recordRef)
		{
			recordRef = default;
			var at = value.IndexOf('@');
			if (at > 0)
			{
				var providedType = value.Substring(0, at);
				var expectedType = typeof(T).Name;
				if (providedType != expectedType) return false;
				value = value.Substring(at + 1);
			}
			if (!Int64.TryParse(value, out long id)) return false;
			recordRef = new RecordRef<T>(id);
			return true;
		}

		/// <summary>
		/// Converts this RecordRef to same reference of a derived type.
		/// </summary>
		/// <typeparam name="TDerived">Derived record type</typeparam>
		/// <returns>Converted record reference</returns>
		public readonly RecordRef<TDerived> Convert<TDerived>() where TDerived : T => new RecordRef<TDerived>(ID);

		#region IConvertible implementation
		readonly TypeCode IConvertible.GetTypeCode() => TypeCode.Object;

		private readonly TTargetType ThrowInvalidCast<TTargetType>()
		{
			ThrowInvalidCast(typeof(TTargetType));
			return default!;
		}

		private readonly void ThrowInvalidCast(Type type) => throw new InvalidCastException("Record \"" + this + "\" cannot be converted to type " + type + ".");

		readonly bool IConvertible.ToBoolean(IFormatProvider? provider) => ThrowInvalidCast<bool>();
		readonly byte IConvertible.ToByte(IFormatProvider? provider) => ThrowInvalidCast<byte>();
		readonly char IConvertible.ToChar(IFormatProvider? provider) => ThrowInvalidCast<char>();
		readonly DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ThrowInvalidCast<DateTime>();
		readonly decimal IConvertible.ToDecimal(IFormatProvider? provider) => ThrowInvalidCast<decimal>();
		readonly double IConvertible.ToDouble(IFormatProvider? provider) => ThrowInvalidCast<double>();
		readonly short IConvertible.ToInt16(IFormatProvider? provider) { checked { return (short)ID; } }
		readonly int IConvertible.ToInt32(IFormatProvider? provider) { checked { return (int)ID; } }
		readonly long IConvertible.ToInt64(IFormatProvider? provider) => ID;
		readonly sbyte IConvertible.ToSByte(IFormatProvider? provider) { checked { return (sbyte)ID; } }
		readonly float IConvertible.ToSingle(IFormatProvider? provider) => ThrowInvalidCast<float>();

		readonly string IConvertible.ToString(IFormatProvider? provider)
		{
			// important for Select control's SelectedValue property
			if (ID == 0) return "";
			return ID.ToString();
		}

		readonly object IConvertible.ToType(Type type, IFormatProvider? provider)
		{
			if (typeof(IRecordRef).IsAssignableFrom(type))
			{
				var recordref = (IRecordRef)Activator.CreateInstance(type)!;
				// recordref is boxed struct
				recordref.ID = ID;
				return recordref;
			}
			ThrowInvalidCast(type);
			return default!;
		}

		readonly ushort IConvertible.ToUInt16(IFormatProvider? provider) { checked { return (ushort)ID; } }
		readonly uint IConvertible.ToUInt32(IFormatProvider? provider) { checked { return (uint)ID; } }
		readonly ulong IConvertible.ToUInt64(IFormatProvider? provider) { checked { return (ulong)ID; } }
		#endregion
	}
}
