using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.Kontext;
using System.Threading.Tasks;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract partial class InsertTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
		}

		[TestMethod]
		public async Task AsyncInsert()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.ID);
			await record.InsertAsync();
			Assert.AreNotEqual(0, record.ID);
		}

		[TestMethod]
		public void InsertAssignsID()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.ID);
			record.Insert();
			Assert.AreNotEqual(0, record.ID);
		}

		[TestMethod]
		public void MultipleInsertsCreateNewRecords()
		{
			var record = new MainTestTable();
			record.Value = INTMARKER;
			record.Insert();
			record.Insert();
			record.Insert();
			var retrieved = orm.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Execute();
			Assert.AreEqual(3, retrieved.Count());
			Assert.AreEqual(3, retrieved.Select(r => r.ID).Distinct().Count());
		}

		[TestMethod]
		public void BatchInsertsCreateNewRecords()
		{
			var records = new[]
				{
					new MainTestTable() { Value = INTMARKER },
					new MainTestTable() { Value = INTMARKER },
					new MainTestTable() { Value = INTMARKER }
				};
			records.StoreAll();
			var retrieved = orm.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Execute();
			Assert.AreEqual(3, retrieved.Count());
			Assert.AreEqual(3, retrieved.Select(r => r.ID).Distinct().Count());
		}

		[TestMethod]
		public void DefaultValueIsAssigned()
		{
			var template = new MainTestTable();
			Assert.AreEqual(MainTestTable.DEFAULTVALUE, template.DefaultValue);

			var meta = typeof(MainTestTable).Meta();
			var db = Context.Current.Get<IDB>();
			var cb = db.CommandBuilder;
			var insert = cb.Insert();
			insert.Table(cb.Identifier(meta.DBName));
			insert.PrimaryKeyReturn(cb.Identifier(meta.PrimaryKey.DBName));
			insert.Column(cb.Identifier(meta.GetField("Value").DBName), cb.Const(INTMARKER));
			insert.Column(cb.Identifier(meta.GetField("NotNullValue").DBName), cb.Const(""));
			db.ExecuteNonQuery(insert.ToString());

			var retrieved = orm.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(MainTestTable.DEFAULTVALUE, retrieved.DefaultValue);
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDException))]
		public void EmptyNotNullValueIsRejected()
		{
			var record = new MainTestTable();
			record.NotNullValue = null;
			record.Insert();
		}

		[TestMethod]
		public void NonPersistentValueIsNotStored()
		{
			var record = new MainTestTable();
			record.NonPersistent = INTMARKER;
			record.Insert();
			Assert.AreEqual(INTMARKER, record.NonPersistent);
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(0, retrieved.NonPersistent);
		}

		[TestMethod]
		public void ExplicitIDIsRespected()
		{
			var record = new MainTestTable();
			record.ID = INTMARKER;
			orm.Insert<MainTestTable>().WithProvidedID().Records(new[] { record });
			Assert.AreEqual(INTMARKER, record.ID);
			var retrieved = orm.Select<MainTestTable>().ByID(INTMARKER);
			Assert.IsNotNull(retrieved);
		}
	}
}
