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
			yield return typeof(BaseConvertedTable);
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
			var record = new BaseConvertedTable
			{
				Int = INTMARKER,
				String = (INTMARKER + 1).ToString()
			};
			ORM.Store(record);
			var retrieved = ORM.Select<TableWithConvertedField>().ByID(record.ID);
			Assert.AreEqual(record.Int.ToString(), retrieved.IntAsString);
			Assert.AreEqual(record.String, retrieved.StringAsInt.ToString());
		}

		[TestMethod]
		public void MarkedTableIsConverted()
		{
			var record = new BaseConvertedTable
			{
				Int = INTMARKER,
				String = (INTMARKER + 1).ToString()
			};
			ORM.Store(record);
			var retrieved = ORM.Select<ConvertedTable>().ByID(record.ID);
			Assert.AreEqual(record.Int.ToString(), retrieved.IntAsString);
			Assert.AreEqual(record.String, retrieved.StringAsInt.ToString());
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))]
		public void NonConvertedTableReadFails()
		{
			var record = new BaseConvertedTable
			{
				Int = INTMARKER,
				String = (INTMARKER + 1).ToString()
			};
			ORM.Store(record);
			ORM.Select<NonConvertedTable>().ByID(record.ID);
		}

		[Table]
		class Table : Record
		{
			[Column("INT NOT NULL DEFAULT 123")]
			public int Value1 { get; set; }

			[Column("INT")]
			public object Value2 { get; set; }
		}

		[Table(Prefix = "")]
		[DBName("BaseConvertedTable")]
		class BaseConvertedTable : Record
		{
			[Column]
			[DBName("IntField")]
			public int Int { get; set; }

			[Column(10)]
			[DBName("TextField")]
			public string String { get; set; }
		}

		[Table(Prefix = "")]
		[DBName("BaseConvertedTable")]
		class TableWithConvertedField : Record
		{
			[Column("INT", IsConverted = true)]
			[DBName("IntField")]
			public string IntAsString { get; set; }

			[Column("VARCHAR(10)", IsConverted = true)]
			[DBName("TextField")]
			public int StringAsInt { get; set; }
		}

		[Table(IsConverted = true, Prefix = "")]
		[DBName("BaseConvertedTable")]
		class ConvertedTable : Record
		{
			[Column("INT")]
			[DBName("IntField")]
			public string IntAsString { get; set; }

			[Column("VARCHAR(10)")]
			[DBName("TextField")]
			public int StringAsInt { get; set; }
		}

		[Table(IsConverted = false, Prefix = "")]
		[DBName("BaseConvertedTable")]
		class NonConvertedTable : Record
		{
			[Column("INT")]
			[DBName("IntField")]
			public string IntAsString { get; set; }

			[Column("VARCHAR(10)")]
			[DBName("TextField")]
			public int StringAsInt { get; set; }
		}
	}
}
