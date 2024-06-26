using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract partial class DeleteTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
		}

		[TestMethod]
		public async Task AsyncDelete()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			await ORM.DeleteAsync(record);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.IsNull(retrieved);
		}

		[TestMethod]
		public void DeleteByRecordDeletes()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			ORM.Delete(record);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.IsNull(retrieved);
		}

		[TestMethod]
		public void BatchDeleteDeletes()
		{
			var records = new[]
				{
					new MainTestTable(),
					new MainTestTable(),
					new MainTestTable()
				};
			ORM.StoreAll(records);
			var retrieved1 = ORM.Select<MainTestTable>().Execute();
			Assert.AreEqual(3, retrieved1.Count());
			ORM.DeleteAll(records);
			var retrieved2 = ORM.Select<MainTestTable>().Execute();
			Assert.AreEqual(0, retrieved2.Count());
		}

		[TestMethod]
		public void LargeBatchDeleteDeletes()
		{
			var count = DB.CommandBuilder.MaxParameterCount + 1;
			var records = Enumerable.Range(1, count).Select(e => new MainTestTable { Value = e }).ToList();
			ORM.StoreAll(records);
			var retrieved1 = ORM.Select<MainTestTable>().Execute();
			Assert.AreEqual(records.Count, retrieved1.Count());
			ORM.DeleteAll(records);
			var retrieved2 = ORM.Select<MainTestTable>().Execute();
			Assert.AreEqual(0, retrieved2.Count());
		}

		[TestMethod]
		public void DeleteByIDDeletes()
		{
			var record = new MainTestTable();
			record.Value = INTMARKER;
			ORM.Insert(record);
			ORM.Delete<MainTestTable>().ByID(record.ID);
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.IsNull(retrieved);
		}

		[TestMethod]
		public void DeleteByColumnDeletes()
		{
			var record = new MainTestTable();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var count = ORM.Delete<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Execute();
			var retrieved = ORM.Select<MainTestTable>().ByID(record.ID);
			Assert.IsNull(retrieved);
			Assert.AreNotEqual(0, count);
		}

		[TestMethod]
		[ExpectedException(typeof(KORMDeleteFailedException))]
		public void DeleteByInvalidIDFails()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			record.ID = -1;
			ORM.Delete(record);
		}

		[TestMethod]
		public void TaggedDeleteDeletesProvidedRecord()
		{
			var records = new[]
			{
					new MainTestTable(),
					new MainTestTable(),
					new MainTestTable()
			};
			ORM.StoreAll(records);

			var deleted = ORM.Delete<MainTestTable>().Tag("Tagged delete").Records(records.Take(1));
			var remaining = ORM.Select<MainTestTable>().Execute();
			Assert.AreEqual(1, deleted);
			Assert.AreEqual(2, remaining.Count);
		}

		[TestMethod]
		public void ClonedIsIndependent()
		{
			var records = new[]
			{
				new MainTestTable { Value = INTMARKER },
				new MainTestTable { Value = INTMARKER + 1 }
			};
			ORM.StoreAll(records);

			var cmd = ORM.Delete<MainTestTable>().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER);
			cmd.Clone().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER + 1);
			cmd.Execute();

			var result = ORM.Select<MainTestTable>().Execute();
			Assert.AreEqual(1, result.Count);
		}

		[TestMethod]
		public void CloneIsIndependent()
		{
			var records = new[]
			{
				new MainTestTable { Value = INTMARKER },
				new MainTestTable { Value = INTMARKER + 1 }
			};
			ORM.StoreAll(records);

			var cmd1 = ORM.Delete<MainTestTable>();
			var cmd2 = cmd1.Clone().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER + 1);
			cmd1.WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER);
			cmd2.Execute();

			var result = ORM.Select<MainTestTable>().Execute();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(records[0].Value, result.Single().Value);
		}

		[TestMethod]
		public void CloneRetainsOriginalState()
		{
			var records = new[]
			{
				new MainTestTable { Value = INTMARKER },
				new MainTestTable { Value = INTMARKER + 1 }
			};
			ORM.StoreAll(records);

			var cmd1 = ORM.Delete<MainTestTable>().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER);
			cmd1.Clone().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER + 1).Execute();

			var result = ORM.Select<MainTestTable>().Execute();
			Assert.AreEqual(2, result.Count);
		}
	}
}
