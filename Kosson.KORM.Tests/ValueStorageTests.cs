using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;

namespace Kosson.KORM.Tests
{
	public abstract class ValueStorageTests : ORMTestsBase
	{
		protected virtual bool SupportsInfinity => true;

		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(SingleValueTableEnum);
			yield return typeof(SingleValueTableBool);
			yield return typeof(SingleValueTableByte);
			yield return typeof(SingleValueTableShort);
			yield return typeof(SingleValueTableInt);
			yield return typeof(SingleValueTableLong);
			yield return typeof(SingleValueTableFloat);
			yield return typeof(SingleValueTableDouble);
			yield return typeof(SingleValueTableDecimal);
			yield return typeof(SingleValueTableDateTime);
			yield return typeof(SingleValueTableGuid);
			yield return typeof(SingleValueTableString);
			yield return typeof(SingleValueTableBlob);
			yield return typeof(SingleValueTableRecordRef);
		}

		private void RetrievedValueIsEqualToStored<TValue, TRecord>(TValue value) where TRecord : SingleValueTable<TValue>, new()
		{
			var record = new TRecord();
			record.Value = value;
			ORM.Insert(record);
			var retrieved = ORM.Select<TRecord>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.Value, retrieved.Value);

			using (var reader = ORM.Select<TRecord>().WhereID(record.ID).ExecuteReader())
			{
				reader.MoveNext();
				var retrievedByReader = reader.Read();
				Assert.IsNotNull(retrievedByReader);
				Assert.AreEqual(record.Value, retrievedByReader.Value);
			}
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredEnum()
		{
			RetrievedValueIsEqualToStored<DayOfWeek, SingleValueTableEnum>(DayOfWeek.Sunday);
			RetrievedValueIsEqualToStored<DayOfWeek, SingleValueTableEnum>(DayOfWeek.Thursday);
			RetrievedValueIsEqualToStored<DayOfWeek, SingleValueTableEnum>((DayOfWeek)42);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredBool()
		{
			RetrievedValueIsEqualToStored<bool, SingleValueTableBool>(true);
			RetrievedValueIsEqualToStored<bool, SingleValueTableBool>(false);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredByte()
		{
			RetrievedValueIsEqualToStored<byte, SingleValueTableByte>(0);
			RetrievedValueIsEqualToStored<byte, SingleValueTableByte>(1);
			RetrievedValueIsEqualToStored<byte, SingleValueTableByte>(142);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredShort()
		{
			RetrievedValueIsEqualToStored<short, SingleValueTableShort>(0);
			RetrievedValueIsEqualToStored<short, SingleValueTableShort>(1);
			RetrievedValueIsEqualToStored<short, SingleValueTableShort>(-42);
			RetrievedValueIsEqualToStored<short, SingleValueTableShort>(12345);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredInt()
		{
			RetrievedValueIsEqualToStored<int, SingleValueTableInt>(0);
			RetrievedValueIsEqualToStored<int, SingleValueTableInt>(1);
			RetrievedValueIsEqualToStored<int, SingleValueTableInt>(-42);
			RetrievedValueIsEqualToStored<int, SingleValueTableInt>(12356789);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredLong()
		{
			RetrievedValueIsEqualToStored<long, SingleValueTableLong>(0);
			RetrievedValueIsEqualToStored<long, SingleValueTableLong>(1);
			RetrievedValueIsEqualToStored<long, SingleValueTableLong>(-42);
			RetrievedValueIsEqualToStored<long, SingleValueTableLong>(123456789123456);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredFloat()
		{
			RetrievedValueIsEqualToStored<float, SingleValueTableFloat>(0f);
			RetrievedValueIsEqualToStored<float, SingleValueTableFloat>(1f);
			RetrievedValueIsEqualToStored<float, SingleValueTableFloat>(-42f);
			if (SupportsInfinity) RetrievedValueIsEqualToStored<float, SingleValueTableFloat>(Single.PositiveInfinity);
			RetrievedValueIsEqualToStored<float, SingleValueTableFloat>(12345.6789f);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredDouble()
		{
			RetrievedValueIsEqualToStored<double, SingleValueTableDouble>(0);
			RetrievedValueIsEqualToStored<double, SingleValueTableDouble>(1);
			RetrievedValueIsEqualToStored<double, SingleValueTableDouble>(-42);
			if (SupportsInfinity) RetrievedValueIsEqualToStored<double, SingleValueTableDouble>(Double.PositiveInfinity);
			RetrievedValueIsEqualToStored<double, SingleValueTableDouble>(12345.6789123456);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredDecimal()
		{
			RetrievedValueIsEqualToStored<decimal, SingleValueTableDecimal>(0);
			RetrievedValueIsEqualToStored<decimal, SingleValueTableDecimal>(1);
			RetrievedValueIsEqualToStored<decimal, SingleValueTableDecimal>(-42);
			RetrievedValueIsEqualToStored<decimal, SingleValueTableDecimal>(123456.789123m);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredDateTime()
		{
			RetrievedValueIsEqualToStored<DateTime, SingleValueTableDateTime>(new DateTime(1900, 1, 1, 0, 0, 0));
			RetrievedValueIsEqualToStored<DateTime, SingleValueTableDateTime>(new DateTime(2017, 6, 22, 7, 48, 51));
			RetrievedValueIsEqualToStored<DateTime, SingleValueTableDateTime>(new DateTime(2100, 12, 31, 23, 59, 59));
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredGuid()
		{
			RetrievedValueIsEqualToStored<Guid, SingleValueTableGuid>(Guid.Empty);
			RetrievedValueIsEqualToStored<Guid, SingleValueTableGuid>(Guid.NewGuid());
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredBlob()
		{
			var record = new SingleValueTableBlob();
			record.Value = new byte[15000];
			for (int i = 0; i < record.Value.Length; i++) record.Value[i] = (byte)i;
			ORM.Insert(record);
			var retrieved = ORM.Select<SingleValueTableBlob>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.Value);
			Assert.AreEqual(record.Value.Length, retrieved.Value.Length);
			for (int i = 0; i < record.Value.Length; i++) Assert.AreEqual(record.Value[i], retrieved.Value[i]);
		}

		[TestMethod]
		public void RetrievedValueIsEqualToStoredRecordRef()
		{
			RetrievedValueIsEqualToStored<RecordRef<MainTestTable>, SingleValueTableRecordRef>(new RecordRef<MainTestTable>(1234567890));
		}
	}

	class SingleValueTable<T> : Record
	{
		[Column]
		public T Value { get; set; }
	}

	[Table]
	class SingleValueTableEnum : SingleValueTable<DayOfWeek>
	{
	}

	[Table]
	class SingleValueTableBool : SingleValueTable<bool>
	{
	}

	[Table]
	class SingleValueTableByte : SingleValueTable<byte>
	{
	}

	[Table]
	class SingleValueTableShort : SingleValueTable<short>
	{
	}

	[Table]
	class SingleValueTableInt : SingleValueTable<int>
	{
	}

	[Table]
	class SingleValueTableLong : SingleValueTable<long>
	{
	}

	[Table]
	class SingleValueTableFloat : SingleValueTable<float>
	{
	}

	[Table]
	class SingleValueTableDouble : SingleValueTable<double>
	{
	}

	[Table]
	class SingleValueTableDecimal : SingleValueTable<decimal>
	{
	}

	[Table]
	class SingleValueTableDateTime : SingleValueTable<DateTime>
	{
	}

	[Table]
	class SingleValueTableGuid : SingleValueTable<Guid>
	{
	}

	[Table]
	class SingleValueTableString : SingleValueTable<string>
	{
	}

	[Table]
	class SingleValueTableBlob : SingleValueTable<byte[]>
	{
	}

	[Table]
	class SingleValueTableRecordRef : SingleValueTable<RecordRef<MainTestTable>>
	{
	}
}
