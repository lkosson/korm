using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;

namespace Kosson.KRUD.Tests
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

		[Table(Prefix = "cqtt", Query = "SELECT \"mtt_ID\" as \"cqtt_ID\", 123 as \"cqtt_Value\" FROM \"MainTestTable\"")]
		class Table : Record
		{
			[Column]
			public int Value { get; set; }
		}
	}
}
