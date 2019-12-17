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
			DB.ExecuteNonQuery(insert.ToString());

			var retrieved = ORM.Select<Table>().WhereFieldEquals("Value1", 123).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(123, retrieved.Value2);
		}

		[Table]
		class Table : Record
		{
			[Column("INT NOT NULL DEFAULT 123")]
			public int Value1 { get; set; }

			[Column("INT")]
			public float Value2 { get; set; }
		}
	}
}
