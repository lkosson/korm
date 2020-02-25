using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;

namespace Kosson.KORM.Tests
{
	public abstract class RenamedTableTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Table);
		}

		[TestMethod]
		public void RenamedColumnExists()
		{
			var cb = DB.CommandBuilder;
			var select = cb.Select();
			select.From(cb.Identifier("NewNameTestTable"));
			select.Column(cb.Identifier("RenamedColumn"));
			DB.ExecuteQuery(select.ToString());
		}

		[TestMethod]
		public void RenamedRetrieve()
		{
			var record = new Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void RenamedUpdate()
		{
			var record = new Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			record.Value++;
			ORM.Update(record);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.Value, retrieved.Value);
		}

		[TestMethod]
		public void RenamedUpdateByID()
		{
			var record = new Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			ORM.Update<Table>().Set("Value", record.Value + 1).ByID(record.ID);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.Value + 1, retrieved.Value);
		}

		[TestMethod]
		public void RenamedDeleteByColumn()
		{
			var record = new Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var count = ORM.Delete<Table>().WhereFieldEquals("Value", INTMARKER).Execute();
			Assert.AreEqual(1, count);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsNull(retrieved);
		}

		[TestMethod]
		public void RenamedTableExists()
		{
			var cb = DB.CommandBuilder;
			var select = cb.Select();
			select.From(cb.Identifier("NewNameTestTable"));
			select.Column(cb.Const(1));
			DB.ExecuteQuery(select.ToString());
		}

		[TestMethod]
		public void AliasColumnRetrievesValue()
		{
			var record = new Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.AreEqual(retrieved.ID, retrieved.IDAlias);
			Assert.AreEqual(retrieved.Value, retrieved.ValueAlias);
		}

		[TestMethod]
		public void AliasColumnIgnoresUpdate()
		{
			var record = new Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			record.ValueAlias = INTMARKER + 1;
			ORM.Update(record);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
			Assert.AreEqual(retrieved.Value, retrieved.ValueAlias);
		}

		[TestMethod]
		[ExpectedException(typeof(KORMException))]
		public virtual void AliasColumnDoesNotExist()
		{
			var cb = DB.CommandBuilder;
			var select = cb.Select();
			select.From(cb.Identifier("NewNameTestTable"));
			select.Column(cb.Identifier("rttt_ValueAlias"));
			DB.ExecuteQuery(select.ToString());
		}

		[Table]
		[DBName("NewNameTestTable")]
		class Table : Record
		{
			[Column]
			[DBName("RenamedColumn")]
			public int Value { get; set; }

			[DBAlias(nameof(ID))]
			public long IDAlias { get; set; }

			[DBAlias(nameof(Value))]
			public int ValueAlias { get; set; }
		}
	}
}
