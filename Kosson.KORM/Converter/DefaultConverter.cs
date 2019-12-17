using Kosson.KORM;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Kosson.KORM.Converter
{
	class DefaultConverter : IConverter
	{
		private static readonly Type typeObject = typeof(object);
		private static readonly Type typeByte = typeof(byte);
		private static readonly Type typeInt = typeof(int);
		private static readonly Type typeLong = typeof(long);
		private static readonly Type typeFloat = typeof(float);
		private static readonly Type typeDouble = typeof(double);
		private static readonly Type typeDateTime = typeof(DateTime);
		private static readonly Type typeBool = typeof(bool);
		private static readonly Type typeDecimal = typeof(decimal);
		private static readonly Type typeString = typeof(string);
		private static readonly Type typeGuid = typeof(Guid);
		private static readonly Type typeBlob = typeof(byte[]);

		private static readonly object[] defaultByTypeCode;

		protected virtual CultureInfo Culture { get { return System.Globalization.CultureInfo.InvariantCulture; } }

		private readonly IFactory factory;
		private readonly ConcurrentDictionary<Type, object> defaultValues;

		static DefaultConverter()
		{
			defaultByTypeCode = new object[19];
			defaultByTypeCode[(int)TypeCode.Boolean] = false;
			defaultByTypeCode[(int)TypeCode.Byte] = (byte)0;
			defaultByTypeCode[(int)TypeCode.Char] = '\0';
			defaultByTypeCode[(int)TypeCode.DateTime] = DateTime.MinValue;
			defaultByTypeCode[(int)TypeCode.DBNull] = DBNull.Value;
			defaultByTypeCode[(int)TypeCode.Decimal] = (decimal)0;
			defaultByTypeCode[(int)TypeCode.Double] = (double)0;
			defaultByTypeCode[(int)TypeCode.Empty] = null;
			defaultByTypeCode[(int)TypeCode.Int16] = (short)0;
			defaultByTypeCode[(int)TypeCode.Int32] = (int)0;
			defaultByTypeCode[(int)TypeCode.Int64] = (long)0;
			defaultByTypeCode[(int)TypeCode.Object] = null;
			defaultByTypeCode[(int)TypeCode.SByte] = (sbyte)0;
			defaultByTypeCode[(int)TypeCode.Single] = (float)0;
			defaultByTypeCode[(int)TypeCode.String] = null;
			defaultByTypeCode[(int)TypeCode.UInt16] = (ushort)0;
			defaultByTypeCode[(int)TypeCode.UInt32] = (uint)0;
			defaultByTypeCode[(int)TypeCode.UInt64] = (ulong)0;
		}

		public DefaultConverter(IFactory factory)
		{
			this.factory = factory;
			defaultValues = new ConcurrentDictionary<Type, object>();
		}

		object IConverter.Convert(object value, Type type)
		{
			if (value == DBNull.Value) value = null;
			if (value == null) return DefaultValue(type);

			// this also handles converting valuetype to Nullable<valuetype>
			if (type.IsInstanceOfType(value)) return value;
			var from = value.GetType();

			//if (from == type) return value;
			//if (type == null || type == typeObject) return value;
			// obligatory before ToXXX
			if (from == typeString) return ParseString((string)value, type);
			if (type == typeString) return ToString(value, from);
			if (type == typeBool) return ToBool(value, from);
			if (type.IsAssignableFrom(from)) return value;

			// obligatory before FromConvertible
			if (type.IsEnum) return Enum.ToObject(type, value);

			var nullable = Nullable.GetUnderlyingType(type);
			if (nullable != null) return ((IConverter)this).Convert(value, nullable);

			IConvertible convertible = value as IConvertible;
			if (convertible != null) return FromConvertible(convertible, type);

			if (from == typeBlob) return FromBlob((byte[])value, type);
			// handled by IsInstanceOfType
			//var nullable = Nullable.GetUnderlyingType(type);
			//if (nullable != null) return ((IConverter)this).Convert(value, nullable);

			return Fail(value, type);
		}

		private static object Fail(object value, Type type)
		{
			throw new InvalidCastException("Value \"" + value + "\" of type " + value.GetType() + " cannot be converted to type " + type + ".");
		}

		private object DefaultValueForTypeCode(TypeCode typeCode)
		{
			return defaultByTypeCode[(int)typeCode];
		}

		private object DefaultValue(Type type)
		{
			var typeCode = Type.GetTypeCode(type);
			if (typeCode != TypeCode.Object) return DefaultValueForTypeCode(typeCode);
			object result;
			if (defaultValues.TryGetValue(type, out result)) return result;

			if (type.IsArray) result = Array.CreateInstance(type.GetElementType(), 0);
			else if (!type.IsValueType) result = null;
			else result = factory.Create(type);
			defaultValues[type] = result;
			return result;
		}

		private static bool ToBool(object value, Type from)
		{
			if (from == typeInt) return (int)value != 0;
			if (from == typeByte) return (byte)value != 0;
			if (from == typeLong) return (long)value != 0;
			// string handled earlier by ParseString
			return Convert.ToBoolean(value);
		}

		private object ToString(object value, Type from)
		{
			if (from == typeDateTime)
			{
				var dt = (DateTime)value;
				if (dt == DateTime.MinValue) return "";
				return dt.ToString(DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern + ".FFFFFF", Culture);
			}
			if (from == typeDecimal) return ((decimal)value).ToString("G", Culture);
			if (from == typeFloat) return ((float)value).ToString("R", Culture);
			if (from == typeDouble) return ((double)value).ToString("R", Culture);
			if (value is IConvertible) return ((IConvertible)value).ToString(Culture);
			return value.ToString();
		}

		private object ParseString(string value, Type type)
		{
			if (String.IsNullOrWhiteSpace(value)) return DefaultValue(type);

			if (type == typeInt) return Int32.Parse(value, Culture);
			if (type == typeLong) return Int64.Parse(value, Culture);
			if (type == typeDateTime) return DateTime.Parse(value, Culture);
			if (type.IsEnum) return Enum.Parse(type, value);
			if (type == typeBool)
			{
				if (value == "0") return false;
				if (value == "1") return true;
				return Boolean.Parse(value);
			}
			if (type == typeDecimal) return Decimal.Parse(value as string, NumberStyles.Currency, Culture);
			if (type == typeFloat) return Single.Parse(value, Culture);
			if (type == typeDouble) return Double.Parse(value, Culture);
			if (type == typeGuid) return Guid.Parse(value);
			var nullable = Nullable.GetUnderlyingType(type);
			if (nullable != null) return ParseString(value, nullable);
			return FromConvertible(value, type);
		}

		private object FromConvertible(IConvertible value, Type type)
		{
			if (typeof(IRecordRef).IsAssignableFrom(type))
			{
				var recordref = (IRecordRef)factory.Create(type);
				recordref.ID = value.ToInt64(Culture);
				return recordref;
			}
			return value.ToType(type, Culture);
		}

		private static object FromBlob(byte[] value, Type type)
		{
			if (type == typeGuid) return new Guid(value);
			ulong val = 0;
			for (int i = 0; i < value.Length; i++)
			{
				val <<= 8;
				val += value[i];
			}
			if (type == typeInt) return (int)val;
			if (type == typeLong) return (long)val;
			//if (type == typeulong) return (ulong)val;
			return Fail(value, type);
		}
	}
}
