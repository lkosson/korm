using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.Kontext;

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
			record.Store();

			record = orm.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record);
			record.Value++;
			record.Store();

			record = orm.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record);
			record.Value++;
			record.Store();

			Assert.AreEqual(INTMARKER + 2, record.Value);
		}

		[TestMethod]
		public void RowVersionIsUpdated()
		{
			var record = new RowVersionTable();
			record.Value = INTMARKER;
			record.Store();

			var record1 = orm.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record1);
			record1.Value++;
			record1.Store();

			var record2 = orm.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record2);
			record2.Value++;
			record2.Store();

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
			record.Store();

			record = orm.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record);
			record.Value++;
			record.Store();
			record.Store();
			record.Store();
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDConcurrentModificationException))]
		public void InterleavedUpdatesFail()
		{
			var record = new RowVersionTable();
			record.Value = INTMARKER;
			record.Store();

			var record1 = orm.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record1);

			var record2 = orm.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record2);

			record1.Value++;
			record2.Value--;

			record1.Store();
			record2.Store();
		}

		[TestMethod]
		public void IndependentInterleavedUpdatesComplete()
		{
			var record1 = new RowVersionTable();
			record1.Value = INTMARKER;
			record1.Store();

			var record2 = new RowVersionTable();
			record2.Value = INTMARKER + 1;
			record2.Store();

			record1 = orm.Select<RowVersionTable>().ByID(record1.ID);
			Assert.IsNotNull(record1);

			record2 = orm.Select<RowVersionTable>().ByID(record2.ID);
			Assert.IsNotNull(record2);

			record1.Value++;
			record2.Value--;

			record1.Store();
			record2.Store();
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDConcurrentModificationException))]
		public void DeleteAfterUpdateFails()
		{
			var record = new RowVersionTable();
			record.Value = INTMARKER;
			record.Store();

			var record1 = orm.Select<RowVersionTable>().ByID(record.ID);
			Assert.IsNotNull(record1);

			record1.Value++;
			record1.Store();

			record.Delete();
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
