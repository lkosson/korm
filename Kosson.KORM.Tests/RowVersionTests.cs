using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.KORM.DB;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract partial class RowVersionTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(RowVersionTable);
		}

		[TestMethod]
		public void SequentialUpdatesComplete()
		{
			var record = new RowVersionTable();
			record.Value = INTMARKER;
			ORM.Store(record);

			record = ORM.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record);
			record.Value++;
			ORM.Store(record);

			record = ORM.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record);
			record.Value++;
			ORM.Store(record);

			Assert.AreEqual(INTMARKER + 2, record.Value);
		}

		[TestMethod]
		public void RowVersionIsUpdated()
		{
			var record = new RowVersionTable();
			record.Value = INTMARKER;
			ORM.Store(record);

			var record1 = ORM.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record1);
			record1.Value++;
			ORM.Store(record1);

			var record2 = ORM.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record2);
			record2.Value++;
			ORM.Store(record2);

			Assert.IsTrue(record1.RowVersion > 0);
			Assert.IsTrue(record2.RowVersion > 0);
			Assert.IsTrue(record1.RowVersion > record.RowVersion);
			Assert.IsTrue(record2.RowVersion > record1.RowVersion);
		}

		[TestMethod]
		public void MultipleUpdatesComplete()
		{
			var record = new RowVersionTable();
			record.Value = INTMARKER;
			ORM.Store(record);

			record = ORM.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record);
			record.Value++;
			ORM.Store(record);
			ORM.Store(record);
			ORM.Store(record);
		}

		[TestMethod]
		[ExpectedException(typeof(KORMConcurrentModificationException))]
		public void InterleavedUpdatesFail()
		{
			var record = new RowVersionTable();
			record.Value = INTMARKER;
			ORM.Store(record);

			var record1 = ORM.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record1);

			var record2 = ORM.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record2);

			record1.Value++;
			record2.Value--;

			ORM.Store(record1);
			ORM.Store(record2);
		}

		[TestMethod]
		public void IndependentInterleavedUpdatesComplete()
		{
			var record1 = new RowVersionTable();
			record1.Value = INTMARKER;
			ORM.Store(record1);

			var record2 = new RowVersionTable();
			record2.Value = INTMARKER + 1;
			ORM.Store(record2);

			record1 = ORM.Select<RowVersionTable>().ByID(record1.ID);
			Assert.IsNotNull(record1);

			record2 = ORM.Select<RowVersionTable>().ByID(record2.ID);
			Assert.IsNotNull(record2);

			record1.Value++;
			record2.Value--;

			ORM.Store(record1);
			ORM.Store(record2);
		}

		[TestMethod]
		[ExpectedException(typeof(KORMConcurrentModificationException))]
		public void DeleteAfterUpdateFails()
		{
			var record = new RowVersionTable();
			record.Value = INTMARKER;
			ORM.Store(record);

			var record1 = ORM.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record1);

			record1.Value++;
			ORM.Store(record1);

			ORM.Delete(record);
		}
	}

	[Table]
	class RowVersionTable : Record, IRecordWithRowVersion
	{
		[Column]
		public long RowVersion { get; set; }

		[Column]
		public int Value { get; set; }
	}
}
