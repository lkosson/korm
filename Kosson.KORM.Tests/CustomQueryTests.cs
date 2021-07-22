using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;

namespace Kosson.KORM.Tests
{
	public abstract class CustomQueryTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
			yield return typeof(Table);
		}

		[TestMethod]
		public void CustomQuerySelect()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			var all = ORM.Select<Table>().Execute();
			Assert.IsNotNull(all);
			var first = all.First();
			Assert.AreNotEqual(0, first.ID);
			Assert.AreEqual(123, first.Value);
		}

		[TestMethod]
		public void CustomQueryWhereSelect()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().WhereFieldEquals("Value", 123).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.ID, retrieved.ID);
		}

		[TestMethod]
		public void CustomQueryOrderSelect()
		{
			var inserted1 = new MainTestTable();
			ORM.Insert(inserted1);
			var inserted2 = new MainTestTable();
			ORM.Insert(inserted2);
			var retrieved = ORM.Select<Table>().OrderByDescending("ID").ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted2.ID, retrieved.ID);
		}

		[Table(Prefix = "cqtt", Query = "SELECT \"mtt_ID\" as \"cqtt_ID\", 123 as \"cqtt_Value\" FROM \"MainTestTable\"")]
		class Table : Record
		{
			[Column]
			public int Value { get; set; }
		}
	}
}
