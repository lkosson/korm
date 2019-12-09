using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.Kontext;
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
			record.Insert();
			await record.DeleteAsync();
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.IsNull(retrieved);
		}

		[TestMethod]
		public void DeleteByRecordDeletes()
		{
			var record = new MainTestTable();
			record.Insert();
			record.Delete();
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
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
			records.StoreAll();
			var retrieved1 = orm.Select<MainTestTable>().Execute();
			Assert.AreEqual(3, retrieved1.Count());
			records.DeleteAll();
			var retrieved2 = orm.Select<MainTestTable>().Execute();
			Assert.AreEqual(0, retrieved2.Count());
		}

		[TestMethod]
		public void DeleteByIDDeletes()
		{
			var record = new MainTestTable();
			record.Value = INTMARKER;
			record.Insert();
			orm.Delete<MainTestTable>().ByID(record.ID);
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.IsNull(retrieved);
		}

		[TestMethod]
		public void DeleteByColumnDeletes()
		{
			var record = new MainTestTable();
			record.Value = INTMARKER;
			record.Insert();
			var count = orm.Delete<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Execute();
			var retrieved = orm.Select<MainTestTable>().ByID(record.ID);
			Assert.IsNull(retrieved);
			Assert.AreNotEqual(0, count);
		}

		[TestMethod]
		[ExpectedException(typeof(ORMDeleteFailedException))]
		public void DeleteByInvalidIDFails()
		{
			var record = new MainTestTable();
			record.Insert();
			record.ID = -1;
			record.Delete();
		}
	}
}
