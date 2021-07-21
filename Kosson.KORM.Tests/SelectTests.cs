using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract partial class SelectTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
			yield return typeof(DivideByZeroTable);
		}

		[TestMethod]
		public async Task AsyncSelect()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			var retrieved = await ORM.Select<MainTestTable>().ExecuteAsync();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
		}

		[TestMethod]
		public void RetrievalByID()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			var retrieved = ORM.Select<MainTestTable>().ByID(inserted.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(inserted.Value, retrieved.Value);
		}

		[TestMethod]
		public void RetrievalByIntValue()
		{
			ORM.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			var other = new MainTestTable();
			other.Value = INTMARKER + 1;
			other.VarLenString = STRINGMARKER;
			ORM.Insert(other);
			var retrieved = ORM.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void RetrievalByStringValue()
		{
			ORM.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			ORM.Insert(other);
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			inserted.VarLenString = STRINGMARKER;
			ORM.Insert(inserted);
			var retrieved = ORM.Select<MainTestTable>().WhereFieldEquals("VarLenString", STRINGMARKER).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(STRINGMARKER, retrieved.VarLenString);
		}

		[TestMethod]
		public void RetrievalByNull()
		{
			ORM.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			ORM.Insert(inserted);
			var other = new MainTestTable();
			other.VarLenString = STRINGMARKER;
			ORM.Insert(other);
			var retrieved = ORM.Select<MainTestTable>().WhereFieldIsNull("VarLenString").Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.IsNull(retrieved.VarLenString);
		}

		[TestMethod]
		public void RetrieveByComparison()
		{
			ORM.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			ORM.Insert(other);
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			ORM.Insert(inserted);
			var retrieved = ORM.Select<MainTestTable>().WhereField("Value", DBExpressionComparison.Greater, INTMARKER).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(inserted.Value, retrieved.Value);
		}

		[TestMethod]
		public void RetrieveByFieldCondition()
		{
			ORM.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			ORM.Insert(other);
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			ORM.Insert(inserted);
			var retrieved = ORM.Select<MainTestTable>().WhereField("Value", "{0} > {1}", INTMARKER).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(inserted.Value, retrieved.Value);
		}

		[TestMethod]
		public void RetrieveByRawCondition()
		{
			ORM.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			ORM.Insert(other);
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			ORM.Insert(inserted);
			var retrieved = ORM.Select<MainTestTable>().WhereRaw("0 < {0}", INTMARKER).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2, retrieved.Count);
		}

		[TestMethod]
		public void RetrieveByFormattedCondition()
		{
			ORM.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			ORM.Insert(other);
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			ORM.Insert(inserted);
			var retrieved = ORM.Select<MainTestTable>().Where($"mtt_Value = {INTMARKER}").Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
		}

		[TestMethod]
		public void RetrieveByArray()
		{
			ORM.Delete<MainTestTable>().Execute();
			for (int i = 0; i < 10; i++)
			{
				var item = new MainTestTable();
				item.Value = INTMARKER + i;
				ORM.Insert(item);
			}
			var retrieved = ORM.Select<MainTestTable>().WhereFieldIn("Value", INTMARKER + 5, INTMARKER + 8, INTMARKER + 12).Execute();
			Assert.AreEqual(2, retrieved.Count);
			Assert.AreEqual(INTMARKER + 5, retrieved.Min(v => v.Value));
			Assert.AreEqual(INTMARKER + 8, retrieved.Max(v => v.Value));
		}

		[TestMethod]
		public void RetrieveBySubquery()
		{
			ORM.Delete<MainTestTable>().Execute();
			for (int i = 0; i < 10; i++)
			{
				var item = new MainTestTable();
				item.Value = INTMARKER + i - 5;
				ORM.Insert(item);
			}
			var retrieved = ORM.Select<MainTestTable>().WhereIDIn("SELECT mtt_ID FROM MainTestTable WHERE mtt_Value < {0}", INTMARKER).Execute();
			Assert.AreEqual(5, retrieved.Count);
			Assert.AreEqual(INTMARKER - 5, retrieved.Min(v => v.Value));
			Assert.AreEqual(INTMARKER - 1, retrieved.Max(v => v.Value));
		}

		[TestMethod]
		public void RetrieveByOr()
		{
			ORM.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			inserted2.VarLenString = STRINGMARKER;
			ORM.Insert(inserted2);
			var other = new MainTestTable();
			other.Value = INTMARKER + 2;
			other.VarLenString = STRINGMARKER;
			ORM.Insert(other);
			var retrieved = ORM.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).Or().WhereFieldEquals("ID", inserted2.ID).Execute();
			Assert.AreEqual(2, retrieved.Count());
			Assert.IsNotNull(retrieved.FirstOrDefault(i => i.Value == INTMARKER));
			Assert.IsNotNull(retrieved.FirstOrDefault(i => i.ID == inserted2.ID));
		}

		[TestMethod]
		public void RetrieveByOrAndAnd()
		{
			ORM.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			inserted.VarLenString = STRINGMARKER;
			ORM.Insert(inserted);
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			inserted2.VarLenString = STRINGMARKER;
			ORM.Insert(inserted2);
			var other = new MainTestTable();
			other.Value = INTMARKER + 2;
			other.VarLenString = STRINGMARKER;
			ORM.Insert(other);
			var retrieved = ORM.Select<MainTestTable>()
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
			ORM.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			inserted2.VarLenString = STRINGMARKER;
			ORM.Insert(inserted2);
			var inserted3 = new MainTestTable();
			inserted3.Value = INTMARKER + 2;
			inserted3.VarLenString = STRINGMARKER;
			ORM.Insert(inserted3);
			var retrieved = ORM.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).WhereFieldEquals("ID", inserted2.ID).Or().WhereFieldEquals("ID", inserted.ID).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
		}

		[TestMethod]
		public void RetrieveByOrOperatorPrecedence()
		{
			ORM.Delete<MainTestTable>().Execute();
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			ORM.Insert(inserted2);
			var inserted3 = new MainTestTable();
			inserted3.Value = INTMARKER + 2;
			ORM.Insert(inserted3);
			var retrieved = ORM.Select<MainTestTable>().WhereFieldEquals("Value", inserted3.Value).WhereFieldEquals("ID", inserted2.ID).Or().WhereFieldEquals("ID", inserted.ID).Execute().Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			var retrieved2 = ORM.Select<MainTestTable>().WhereFieldEquals("ID", inserted2.ID).Or().WhereFieldEquals("Value", inserted3.Value).WhereFieldEquals("ID", inserted.ID).Execute().Single();
			Assert.IsNotNull(retrieved2);
			Assert.AreEqual(inserted2.ID, retrieved2.ID);
		}

		[TestMethod]
		public void SelectForUpdateRetrievesRecord()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			var retrieved = ORM.Select<MainTestTable>().ForUpdate().ByID(inserted.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.ID, retrieved.ID);
			Assert.AreEqual(inserted.Value, retrieved.Value);
		}

		[TestMethod]
		public void LimitLimitsRowCount()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			ORM.Insert(inserted);
			ORM.Insert(inserted);
			var retrieved = ORM.Select<MainTestTable>().Limit(2).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2, retrieved.Count);
		}

		[TestMethod]
		[ExpectedException(typeof(KORMException))]
		public virtual void FailedSelectThrowsException()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			ORM.Select<DivideByZeroTable>().Execute();
		}

		[TestMethod]
		[ExpectedException(typeof(KORMException))]
		public virtual async Task FailedSelectAsyncThrowsException()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			await ORM.Select<DivideByZeroTable>().ExecuteAsync();
		}

		[TestMethod]
		public virtual void FailedSelectDisposesReader()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			Assert.ThrowsException<KORMException>(ORM.Select<DivideByZeroTable>().Execute);
			ORM.Select<MainTestTable>().Execute();
		}

		[TestMethod]
		public virtual async Task AsyncFailedSelectDisposesReader()
		{
			var record = new MainTestTable();
			ORM.Insert(record);
			await Assert.ThrowsExceptionAsync<KORMException>(ORM.Select<DivideByZeroTable>().ExecuteAsync);
			await ORM.Select<MainTestTable>().ExecuteAsync();
		}

		[Table(Prefix = "dbzt", Query = "SELECT 1/0 as \"dbzt_ID\" FROM \"MainTestTable\"")]
		class DivideByZeroTable : Record
		{
		}
	}
}
