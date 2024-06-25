using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;

namespace Kosson.KORM.Tests
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
			ORM.Insert(record);
			record.Value = INTMARKER;
			await ORM.UpdateAsync(record);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void UpdateStoresValue()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			ORM.Insert(record);
			record.Value = INTMARKER;
			ORM.Update(record);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void MultipleUpdatesStoreValue()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			ORM.Insert(record);
			record.Value = INTMARKER;
			ORM.Update(record);
			record.Value = 0;
			ORM.Update(record);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
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
			ORM.StoreAll(records);
			foreach (var record in records) record.Value = INTMARKER;
			ORM.StoreAll(records);
			var retrieved = ORM.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Execute();
			Assert.AreEqual(3, retrieved.Count());
		}

		[TestMethod]
		public void UpdateByIDStoresValue()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			ORM.Insert(record);
			ORM.Update<MainTestTable>().Set("Value", INTMARKER).ByID(record.ID);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void StoreInsertsAndUpdates()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			ORM.Store(record);
			Assert.AreNotEqual(0, record.ID);
			record.Value = INTMARKER;
			ORM.Store(record);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void UpdateByColumnStoresValue()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.Value);
			ORM.Insert(record);
			var count = ORM.Update<MainTestTable>().Set("Value", INTMARKER).WhereFieldEquals("Value", 0).Execute();
			Assert.AreEqual(1, count);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void ReadOnlyColumnIsNotUpdated()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.ReadOnly);
			ORM.Insert(record);
			record.ReadOnly = INTMARKER;
			ORM.Update(record);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(0, retrieved.Value);
		}

		[TestMethod]
		public void ReadOnlyColumnIsRead()
		{
			var record = new MainTestTable();
			Assert.AreEqual(0, record.ReadOnly);
			ORM.Insert(record);

			var count = ORM.Update<MainTestTable>().Set("ReadOnly", INTMARKER).Execute();
			Assert.AreEqual(1, count);

			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.AreEqual(INTMARKER, retrieved.ReadOnly);
		}

		[TestMethod]
		public void TaggedUpdateUpdatesProvidedRecord()
		{
			var records = new[]
			{
					new MainTestTable(),
					new MainTestTable(),
					new MainTestTable()
			};
			ORM.StoreAll(records);

			var forUpdate = records.Take(1);
			foreach (var record in forUpdate) record.Value = INTMARKER;
			var deleted = ORM.Update<MainTestTable>().Tag("Tagged update").Records(forUpdate);
			var retrieved = ORM.Select<MainTestTable>().Execute();
			Assert.AreEqual(1, retrieved.Count(e => e.Value == INTMARKER));
			Assert.AreEqual(2, retrieved.Count(e => e.Value != INTMARKER));
		}

		[TestMethod]
		public void ClonedIsIndependent()
		{
			var record = new MainTestTable { Value = INTMARKER };
			ORM.Store(record);

			var cmd = ORM.Update<MainTestTable>().Set(nameof(MainTestTable.Value), INTMARKER + 1);
			cmd.Clone().Set(nameof(MainTestTable.Value), INTMARKER + 2);
			cmd.Execute();

			var result = ORM.Select<MainTestTable>().ExecuteFirst();
			Assert.AreEqual(INTMARKER + 1, result.Value);
		}

		[TestMethod]
		public void CloneIsIndependent()
		{
			var record = new MainTestTable { Value = INTMARKER };
			ORM.Store(record);

			var cmd1 = ORM.Update<MainTestTable>();
			var cmd2 = cmd1.Clone().Set(nameof(MainTestTable.Value), INTMARKER + 1);
			cmd1.Set(nameof(MainTestTable.Value), INTMARKER + 2);
			cmd2.Execute();

			var result = ORM.Select<MainTestTable>().ExecuteFirst();
			Assert.AreEqual(INTMARKER + 1, result.Value);
		}

		[TestMethod]
		public void CloneRetainsOriginalState()
		{
			var record = new MainTestTable { Value = INTMARKER };
			ORM.Store(record);

			var cmd1 = ORM.Update<MainTestTable>().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER + 1);
			cmd1.Clone().Set(nameof(MainTestTable.Value), INTMARKER + 2).Execute();

			var result = ORM.Select<MainTestTable>().ExecuteFirst();
			Assert.AreEqual(INTMARKER, result.Value);
		}
	}
}
