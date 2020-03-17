using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;

namespace Kosson.KORM.Tests
{
	public abstract class CustomColumnDefinitionTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Table);
			yield return typeof(TableWithConvertedField);
			yield return typeof(ConvertedTable);
			yield return typeof(NonConvertedTable);
		}

		[TestMethod]
		public void ColumnDefinitionIsRespected()
		{
			var meta = MetaBuilder.Get(typeof(Table));
			var cb = DB.CommandBuilder;
			var insert = cb.Insert();
			insert.Table(cb.Identifier(meta.DBName));
			insert.PrimaryKeyReturn(cb.Identifier(meta.PrimaryKey.DBName));
			insert.Column(cb.Identifier(meta.GetField("Value2").DBName), cb.Const(123.456));
			DB.ExecuteNonQueryRaw(insert.ToString());

			var retrieved = ORM.Select<Table>().WhereFieldEquals("Value1", 123).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(123, retrieved.Value2);
		}

		[TestMethod]
		public void MarkedFieldsAreConverted()
		{
			var record = new TableWithConvertedField
			{
				IntAsString = INTMARKER.ToString(),
				StringAsInt = INTMARKER + 1
			};
			ORM.Store(record);
			var retrieved = ORM.Get(record.Ref());
			Assert.AreEqual(record.IntAsString, retrieved.IntAsString);
			Assert.AreEqual(record.StringAsInt, retrieved.StringAsInt);
		}

		[TestMethod]
		public void MarkedTableIsConverted()
		{
			var record = new ConvertedTable
			{
				IntAsString = INTMARKER.ToString(),
				StringAsInt = INTMARKER + 1
			};
			ORM.Store(record);
			var retrieved = ORM.Get(record.Ref());
			Assert.AreEqual(record.IntAsString, retrieved.IntAsString);
			Assert.AreEqual(record.StringAsInt, retrieved.StringAsInt);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))]
		public void NonConvertedTableReadFails()
		{
			var record = new NonConvertedTable
			{
				IntAsString = INTMARKER.ToString(),
				StringAsInt = INTMARKER + 1
			};
			ORM.Store(record);
			ORM.Get(record.Ref());
		}

		[Table]
		class Table : Record
		{
			[Column("INT NOT NULL DEFAULT 123")]
			public int Value1 { get; set; }

			[Column("INT")]
			public object Value2 { get; set; }
		}

		[Table]
		class TableWithConvertedField : Record
		{
			[Column("INT", IsConverted = true)]
			public string IntAsString { get; set; }

			[Column("VARCHAR(10)", IsConverted = true)]
			public int StringAsInt { get; set; }
		}

		[Table(IsConverted = true)]
		class ConvertedTable : Record
		{
			[Column("INT")]
			public string IntAsString { get; set; }

			[Column("VARCHAR(10)")]
			public int StringAsInt { get; set; }
		}

		[Table(IsConverted = false)]
		class NonConvertedTable : Record
		{
			[Column("INT")]
			public string IntAsString { get; set; }

			[Column("VARCHAR(10)")]
			public int StringAsInt { get; set; }
		}
	}
}
