using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;
using System.Collections.Generic;

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
		public virtual void RetrieveByFormattedCondition()
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
		public void RetrieveByMultipleIDsOneBatchSmall()
		{
			RetrieveByMultipleIDsImpl(10, 3);
		}

		[TestMethod]
		public void RetrieveByMultipleIDsOneBatchLarge()
		{
			RetrieveByMultipleIDsImpl(DB.CommandBuilder.MaxParameterCount, DB.CommandBuilder.MaxParameterCount * 9 / 10);
		}

		[TestMethod]
		public void RetrieveByMultipleIDsByBlocks()
		{
			RetrieveByMultipleIDsImpl(DB.CommandBuilder.MaxParameterCount * 25, DB.CommandBuilder.MaxParameterCount * 2);
		}

		[TestMethod]
		public void RetrieveByMultipleIDsWholeTable()
		{
			RetrieveByMultipleIDsImpl(DB.CommandBuilder.MaxParameterCount * 3, DB.CommandBuilder.MaxParameterCount * 2);
		}

		private void RetrieveByMultipleIDsImpl(int insertCount, int retrieveCount)
		{
			ORM.Delete<MainTestTable>().Execute();
			var ids = new List<long>();
			for (int i = 0; i < insertCount; i++)
			{
				var item = new MainTestTable();
				item.Value = i;
				ORM.Insert(item);
				if (i < (insertCount - retrieveCount) / 2 && ids.Count < retrieveCount) ids.Add(item.ID);
			}
			var retrieved = ORM.Select<MainTestTable>().ByIDs(ids);
			Assert.AreEqual(ids.Count, retrieved.Count);
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
			var retrieved = ORM.Select<MainTestTable>().WhereIDIn("SELECT " + DB.CommandBuilder.IdentifierQuoteLeft + "mtt_ID" + DB.CommandBuilder.IdentifierQuoteRight + " FROM " + DB.CommandBuilder.IdentifierQuoteLeft + "MainTestTable" + DB.CommandBuilder.IdentifierQuoteRight + " WHERE " + DB.CommandBuilder.IdentifierQuoteLeft + "mtt_Value" + DB.CommandBuilder.IdentifierQuoteRight+ " < {0}", INTMARKER).Execute();
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
		public void ExecuteCountReturnsCount()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			ORM.Insert(inserted);
			ORM.Insert(inserted);
			var count = ORM.Select<MainTestTable>().ExecuteCount();
			Assert.AreEqual(3, count);
		}

		[TestMethod]
		public async Task ExecuteCountAsyncReturnsCount()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			ORM.Insert(inserted);
			ORM.Insert(inserted);
			var count = await ORM.Select<MainTestTable>().ExecuteCountAsync();
			Assert.AreEqual(3, count);
		}

		[TestMethod]
		public void ExecuteCountRespectsNarrows()
		{
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER;
			ORM.Insert(inserted);
			inserted.Value = INTMARKER + 1;
			ORM.Insert(inserted);
			inserted.Value = INTMARKER + 2;
			ORM.Insert(inserted);
			var count = ORM.Select<MainTestTable>().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER).ExecuteCount();
			Assert.AreEqual(1, count);
		}

		[TestMethod]
		public void ExecuteCountReturnsZeroIfEmpty()
		{
			var count = ORM.Select<MainTestTable>().ExecuteCount();
			Assert.AreEqual(0, count);
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

		[TestMethod]
		public void ClonedIsIndependent()
		{
			var inserted1 = new MainTestTable();
			inserted1.Value = INTMARKER;
			ORM.Insert(inserted1);
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			ORM.Insert(inserted2);

			var q1 = ORM.Select<MainTestTable>().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER);
			q1.Clone().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER + 1);

			var result = q1.Execute();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(inserted1.Value, result.Single().Value);
		}

		[TestMethod]
		public void CloneIsIndependent()
		{
			var inserted1 = new MainTestTable();
			inserted1.Value = INTMARKER;
			ORM.Insert(inserted1);
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			ORM.Insert(inserted2);

			var q1 = ORM.Select<MainTestTable>();
			var q2 = q1.Clone().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER + 1);
			q1.WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER);

			var result = q2.Execute();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(inserted2.Value, result.Single().Value);
		}

		[TestMethod]
		public void CloneRetainsOriginalState()
		{
			var inserted1 = new MainTestTable();
			inserted1.Value = INTMARKER;
			ORM.Insert(inserted1);
			var inserted2 = new MainTestTable();
			inserted2.Value = INTMARKER + 1;
			ORM.Insert(inserted2);

			var q1 = ORM.Select<MainTestTable>().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER);
			var q2 = q1.Clone().WhereFieldEquals(nameof(MainTestTable.Value), INTMARKER + 1);

			var result = q2.Execute();
			Assert.AreEqual(0, result.Count);
		}

		[Table(Prefix = "dbzt", Query = "SELECT 1/0 as \"dbzt_ID\" FROM \"MainTestTable\"")]
		class DivideByZeroTable : Record
		{
		}
	}
}
