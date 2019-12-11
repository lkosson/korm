using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using System.Threading.Tasks;

namespace Kosson.KRUD.Tests
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
		[ExpectedException(typeof(ORMDeleteFailedException))]
		public void DeleteByInvalidIDFails()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			record.ID = -1;
			ORM.Delete(record);
		}
	}
}
