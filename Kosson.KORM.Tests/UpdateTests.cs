using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.Kontext;
using System.Threading.Tasks;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract partial class UpdateTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
		}

		[TestMethod]
		public async Task AsyncUpdate()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			record.Insert();
			record.Value = INTMARKER;
			await record.UpdateAsync();
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void UpdateStoresValue()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			record.Insert();
			record.Value = INTMARKER;
			record.Update();
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void MultipleUpdatesStoreValue()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			record.Insert();
			record.Value = INTMARKER;
			record.Update();
			record.Value = 0;
			record.Update();
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(0, retrieved.Value);
		}

		[TestMethod]
		public void BatchUpdateStoresValue()
		{
			var records = new[]
				{
					new MainTestTable(),
					new MainTestTable(),
					new MainTestTable()
				};
			records.StoreAll();
			foreach (var record in records) record.Value = INTMARKER;
			records.StoreAll();
			var retrieved = orm.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Execute();
			Assert.AreEqual(3, retrieved.Count());
		}

		[TestMethod]
		public void UpdateByIDStoresValue()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			record.Insert();
			orm.Update<MainTestTable>().Set("Value", INTMARKER).ByID(record.ID);
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void StoreInsertsAndUpdates()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			record.Store();
			Assert.AreNotEqual(0, record.ID);
			record.Value = INTMARKER;
			record.Store();
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void UpdateByColumnStoresValue()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			record.Insert();
			var count = orm.Update<MainTestTable>().Set("Value", INTMARKER).WhereFieldEquals("Value", 0).Execute();
			Assert.AreEqual(1, count);
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void ReadOnlyColumnIsNotUpdated()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.ReadOnly);
			record.Insert();
			record.ReadOnly = INTMARKER;
			record.Update();
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(0, retrieved.Value);
		}

		[TestMethod]
		public void ReadOnlyColumnIsRead()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.ReadOnly);
			record.Insert();

			var count = orm.Update<MainTestTable>().Set("ReadOnly", INTMARKER).Execute();
			Assert.AreEqual(1, count);

			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.ReadOnly);
		}
	}
}
