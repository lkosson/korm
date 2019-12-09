using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.Kontext;
using System.Threading.Tasks;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract partial class SelectTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
		}

		[TestMethod]
		public async Task AsyncSelect()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			inserted.Insert();
			var retrieved = await orm.Select<MainTestTable>().ExecuteAsync();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
		}

		[TestMethod]
		public void RetrievalByID()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			inserted.Insert();
			var retrieved = orm.Select<MainTestTable>().ByID(inserted.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(inserted.Value, retrieved.Value);
		}

		[TestMethod]
		public void RetrievalByIntValue()
		{
			orm.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			inserted.Insert();
			var other = new MainTestTable();
			other.Value = INTMARKER + 1;
			other.VarLenString = STRINGMARKER;
			other.Insert();
			var retrieved = orm.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void RetrievalByStringValue()
		{
			orm.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			other.Insert();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			inserted.VarLenString = STRINGMARKER;
			inserted.Insert();
			var retrieved = orm.Select<MainTestTable>().WhereFieldEquals("VarLenString", STRINGMARKER).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(STRINGMARKER, retrieved.VarLenString);
		}

		[TestMethod]
		public void RetrievalByNull()
		{
			orm.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Insert();
			var other = new MainTestTable();
			other.VarLenString = STRINGMARKER;
			other.Insert();
			var retrieved = orm.Select<MainTestTable>().WhereFieldIsNull("VarLenString").Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.IsNull(retrieved.VarLenString);
		}

		[TestMethod]
		public void RetrieveByComparison()
		{
			orm.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			other.Insert();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			inserted.Insert();
			var retrieved = orm.Select<MainTestTable>().WhereField("Value", DBExpressionComparison.Greater, INTMARKER).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(inserted.Value, retrieved.Value);
		}

		[TestMethod]
		public void RetrieveByFieldCondition()
		{
			orm.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			other.Insert();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			inserted.Insert();
			var retrieved = orm.Select<MainTestTable>().WhereField("Value", "{0} > {1}", INTMARKER).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(inserted.Value, retrieved.Value);
		}

		[TestMethod]
		public void RetrieveByRawCondition()
		{
			orm.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			other.Insert();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			inserted.Insert();
			var retrieved = orm.Select<MainTestTable>().Where("0 < {0}", INTMARKER).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2, retrieved.Count);
		}

		[TestMethod]
		public void RetrieveByArray()
		{
			orm.Delete<MainTestTable>().Execute();
			for (int i = 0; i < 10; i++)
			{
				var item = new MainTestTable();
				item.Value = INTMARKER + i;
				item.Insert();
			}
			var retrieved = orm.Select<MainTestTable>().WhereFieldIn("Value", INTMARKER + 5, INTMARKER + 8, INTMARKER + 12).Execute();
			Assert.AreEqual(2, retrieved.Count);
			Assert.AreEqual(INTMARKER + 5, retrieved.Min(v => v.Value));
			Assert.AreEqual(INTMARKER + 8, retrieved.Max(v => v.Value));
		}

		[TestMethod]
		public void RetrieveByOr()
		{
			orm.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			inserted.Insert();
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			inserted2.VarLenString = STRINGMARKER;
			inserted2.Insert();
			var other = new MainTestTable();
			other.Value = INTMARKER + 2;
			other.VarLenString = STRINGMARKER;
			other.Insert();
			var retrieved = orm.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Or().WhereFieldEquals("ID", inserted2.ID).Execute();
			Assert.AreEqual(2, retrieved.Count());
			Assert.IsNotNull(retrieved.FirstOrDefault(i => i.Value == INTMARKER));
			Assert.IsNotNull(retrieved.FirstOrDefault(i => i.ID == inserted2.ID));
		}

		[TestMethod]
		public void RetrieveByOrAndAnd()
		{
			orm.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			inserted.VarLenString = STRINGMARKER;
			inserted.Insert();
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			inserted2.VarLenString = STRINGMARKER;
			inserted2.Insert();
			var other = new MainTestTable();
			other.Value = INTMARKER + 2;
			other.VarLenString = STRINGMARKER;
			other.Insert();
			var retrieved = orm.Select<MainTestTable>()
				.WhereFieldEquals("Value", INTMARKER)
				.WhereFieldEquals("VarLenString", STRINGMARKER)
				.Or()
				.WhereFieldEquals("Value", INTMARKER + 1)
				.WhereFieldEquals("VarLenString", STRINGMARKER)
				.Execute();
			Assert.AreEqual(2, retrieved.Count());
			Assert.IsNotNull(retrieved.FirstOrDefault(i => i.Value == INTMARKER));
			Assert.IsNotNull(retrieved.FirstOrDefault(i => i.Value == INTMARKER + 1));
		}

		[TestMethod]
		public void RetrieveByOrWithMultipleClauses()
		{
			orm.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			inserted.Insert();
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			inserted2.VarLenString = STRINGMARKER;
			inserted2.Insert();
			var inserted3 = new MainTestTable();
			inserted3.Value = INTMARKER + 2;
			inserted3.VarLenString = STRINGMARKER;
			inserted3.Insert();
			var retrieved = orm.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).WhereFieldEquals("ID", inserted2.ID).Or().WhereFieldEquals("ID", inserted.ID).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
		}

		[TestMethod]
		public void SelectForUpdateRetrievesRecord()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			inserted.Insert();
			var retrieved = orm.Select<MainTestTable>().ForUpdate().ByID(inserted.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(inserted.Value, retrieved.Value);
		}

		[TestMethod]
		public void LimitLimitsRowCount()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			inserted.Insert();
			inserted.Insert();
			inserted.Insert();
			var retrieved = orm.Select<MainTestTable>().Limit(2).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2, retrieved.Count);
		}
	}
}
