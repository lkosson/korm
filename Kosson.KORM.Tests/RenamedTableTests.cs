using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.Kontext;

namespace Kosson.KRUD.Tests
{
	public abstract class RenamedTableTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Table);
		}

		[TestMethod]
		public void RenamedRetrieve()
		{
			var record = new Table();
			record.Value = INTMARKER;
			record.Insert();
			var retrieved = orm.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void RenamedUpdate()
		{
			var record = new Table();
			record.Value = INTMARKER;
			record.Insert();
			record.Value++;
			record.Update();
			var retrieved = orm.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.Value, retrieved.Value);
		}

		[TestMethod]
		public void RenamedUpdateByID()
		{
			var record = new Table();
			record.Value = INTMARKER;
			record.Insert();
			orm.Update<Table>().Set("Value", record.Value + 1).ByID(record.ID);
			var retrieved = orm.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.Value + 1, retrieved.Value);
		}

		[TestMethod]
		public void RenamedDeleteByColumn()
		{
			var record = new Table();
			record.Value = INTMARKER;
			record.Insert();
			var count = orm.Delete<Table>().WhereFieldEquals("Value", INTMARKER).Execute();
			Assert.AreEqual(1, count);
			var retrieved = orm.Select<Table>().ByID(record.ID);
			Assert.IsNull(retrieved);
		}

		[TestMethod]
		public void RenamedTableExists()
		{
			var db = Context.Current.Get<IDB>();
			var cb = db.CommandBuilder;
			var select = cb.Select();
			select.From(cb.Identifier("NewNameTestTable"));
			select.Column(cb.Const(1));
			db.ExecuteQuery(select.ToString());
		}

		[Table]
		[DBName("NewNameTestTable")]
		class Table : Record
		{
			[Column]
			[DBName("RenamedColumn")]
			public int Value { get; set; }
		}
	}
}
