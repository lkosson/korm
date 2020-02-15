using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract partial class Record32Tests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Record32Table);
			yield return typeof(Record32ReferencingTable);
			yield return typeof(Record32RefReferencingTable);
		}

		[TestMethod]
		public void Record32InsertAssignsID()
		{
			var record = new Record32Table();
			Assert.AreEqual(0, record.ID);
			ORM.Insert(record);
			Assert.AreNotEqual(0, record.ID);
		}

		[TestMethod]
		public void Record32RetrievedByID()
		{
			var record = new Record32Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Record32Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.ID, retrieved.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void Record32RetrievedByRef()
		{
			var record = new Record32Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Record32Table>().ByRef(record.Ref());
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.ID, retrieved.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void Record32RefReferenceIsStored()
		{
			var record1 = new Record32Table();
			ORM.Insert(record1);
			var record2 = new Record32RefReferencingTable();
			record2.Record32 = record1;
			ORM.Store(record2);
			var retrieved = ORM.Select<Record32RefReferencingTable>().ByID(record2.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.Record32);
			Assert.AreEqual(record1.ID, retrieved.Record32.ID);
		}

		[TestMethod]
		public void Record32ReferenceIsStored()
		{
			var record1 = new Record32Table();
			record1.Value = INTMARKER;
			ORM.Insert(record1);
			var record2 = new Record32ReferencingTable();
			record2.Record32 = record1;
			ORM.Store(record2);
			var retrieved = ORM.Select<Record32ReferencingTable>().ByID(record2.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.Record32);
			Assert.AreEqual(record1.ID, retrieved.Record32.ID);
			Assert.AreEqual(INTMARKER, retrieved.Record32.Value);
		}

		[TestMethod]
		[ExpectedException(typeof(OverflowException))]
		public void Record32OutOfRangeValueIsRejected()
		{
			var record = new Record32Table();
			((IRecord)record).ID = (long)Int32.MaxValue + 1;
		}
	}

	[Table]
	class Record32Table : Record32
	{
		[Column]
		public int Value { get; set; }
	}

	[Table]
	class Record32RefReferencingTable : Record
	{
		[ForeignKey.Cascade]
		[Column]
		public RecordRef<Record32Table> Record32 { get; set; }
	}

	[Table]
	class Record32ReferencingTable : Record
	{
		[ForeignKey.Cascade]
		[Column]
		public Record32Table Record32 { get; set; }
	}
}
