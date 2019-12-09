using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;

namespace Kosson.KRUD.Tests
{
	public abstract class StringStorageTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(TrimmedStringTable);
			yield return typeof(LimitedStringTable);
			yield return typeof(UnlimitedStringTable);
			yield return typeof(AnsiStringTable);
			yield return typeof(FixedLengthStringTable);
			yield return typeof(XmlStringTable);
		}

		private string StoreAndRetrieve<T>(string value) where T : class, IHasStringValue, IRecord, new()
		{
			var record = new T();
			record.Value = value;
			record.Insert();
			var retrieved = orm.Select<T>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.ID, retrieved.ID);
			Assert.IsNotNull(retrieved.Value);
			return retrieved.Value;
		}

		[TestMethod]
		public void LimitedFittingStringIsStored()
		{
			var stored = "abc def ghi";
			var retrieved = StoreAndRetrieve<LimitedStringTable>(stored);
			Assert.AreEqual(stored, retrieved);
		}

		[TestMethod]
		public void LeadingSpacesArePreserved()
		{
			var stored = "   abc def";
			var retrieved = StoreAndRetrieve<LimitedStringTable>(stored);
			Assert.AreEqual(stored, retrieved);
		}

		[TestMethod]
		public void TrailingSpacesArePreserved()
		{
			var stored = "abc def    ";
			var retrieved = StoreAndRetrieve<LimitedStringTable>(stored);
			Assert.AreEqual(stored, retrieved);
		}

		[TestMethod]
		public void EmptyStringIsNotNull()
		{
			var stored = "";
			var retrieved = StoreAndRetrieve<LimitedStringTable>(stored);
			Assert.AreEqual(stored, retrieved);
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDDataLengthException))]
		public void LimitedTooLongStringThrowsError()
		{
			var stored = "abc def ghi jkl mno";
			StoreAndRetrieve<LimitedStringTable>(stored);
		}

		[TestMethod]
		public void TrimmedStringIsTrimmed()
		{
			var stored = "abcdefghijkl";
			var retrieved = StoreAndRetrieve<TrimmedStringTable>(stored);
			Assert.AreEqual(stored.Substring(0, 5), retrieved);
		}

		[TestMethod]
		public void AnsiStringIsStored()
		{
			var stored = "abc def gh";
			var retrieved = StoreAndRetrieve<AnsiStringTable>(stored);
			Assert.AreEqual(stored, retrieved);
		}

		[TestMethod]
		public void UnicodeCharactersArePreserved()
		{
			var stored = "Ź\u72FC\u1F43A\uFD8D\u041B";
			var retrieved = StoreAndRetrieve<LimitedStringTable>(stored);
			Assert.AreEqual(stored, retrieved);
		}

		[TestMethod]
		public void UnlimitedStringIsStored()
		{
			var stored = "abc";
			for (int i = 0; i < 2000; i++) stored += " " + i;
			var retrieved = StoreAndRetrieve<UnlimitedStringTable>(stored);
			Assert.AreEqual(stored, retrieved);
		}

		[TestMethod]
		public void XmlStringIsStored()
		{
			var stored = "<root><element attribute=\"value\">content źźź</element><element /></root>";
			var retrieved = StoreAndRetrieve<XmlStringTable>(stored);
			Assert.AreEqual(stored, retrieved);
		}
	}

	interface IHasStringValue
	{
		string Value { get; set; }
	}

	[Table]
	class TrimmedStringTable : Record, IHasStringValue
	{
		[Column(5, Trim = true)]
		public string Value { get; set; }
	}

	[Table]
	class LimitedStringTable : Record, IHasStringValue
	{
		[Column(12)]
		public string Value { get; set; }
	}

	[Table]
	class UnlimitedStringTable : Record, IHasStringValue
	{
		[Column]
		public string Value { get; set; }
	}

	[Table]
	class AnsiStringTable : Record, IHasStringValue
	{
		[Column(System.Data.DbType.AnsiString)]
		public string Value { get; set; }
	}

	[Table]
	class FixedLengthStringTable : Record, IHasStringValue
	{
		[Column(System.Data.DbType.StringFixedLength, Length=10)]
		public string Value { get; set; }
	}

	[Table]
	class XmlStringTable : Record, IHasStringValue
	{
		[Column(System.Data.DbType.Xml)]
		public string Value { get; set; }
	}
}
