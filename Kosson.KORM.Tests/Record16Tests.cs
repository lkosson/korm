using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract partial class Record16Tests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Record16Table);
			yield return typeof(Record16ReferencingTable);
			yield return typeof(Record16RefReferencingTable);
		}

		[TestMethod]
		public void Record16InsertAssignsID()
		{
			var record = new Record16Table();
			Assert.AreEqual(0, record.ID);
			ORM.Insert(record);
			Assert.AreNotEqual(0, record.ID);
		}

		[TestMethod]
		public void Record16RetrievedByID()
		{
			var record = new Record16Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Record16Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.ID, retrieved.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void Record16RetrievedByRef()
		{
			var record = new Record16Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Record16Table>().ByRef(record.Ref());
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.ID, retrieved.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void Record16RefReferenceIsStored()
		{
			var record1 = new Record16Table();
			ORM.Insert(record1);
			var record2 = new Record16RefReferencingTable();
			record2.Record16 = record1;
			ORM.Store(record2);
			var retrieved = ORM.Select<Record16RefReferencingTable>().ByID(record2.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.Record16);
			Assert.AreEqual(record1.ID, retrieved.Record16.ID);
		}

		[TestMethod]
		public void Record16ReferenceIsStored()
		{
			var record1 = new Record16Table();
			record1.Value = INTMARKER;
			ORM.Insert(record1);
			var record2 = new Record16ReferencingTable();
			record2.Record16 = record1;
			ORM.Store(record2);
			var retrieved = ORM.Select<Record16ReferencingTable>().ByID(record2.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.Record16);
			Assert.AreEqual(record1.ID, retrieved.Record16.ID);
			Assert.AreEqual(INTMARKER, retrieved.Record16.Value);
		}

		[TestMethod]
		[ExpectedException(typeof(OverflowException))]
		public void Record16OutOfRangeValueIsRejected()
		{
			var record = new Record16Table();
			((IRecord)record).ID = (long)Int16.MaxValue + 1;
		}
	}

	[Table]
	class Record16Table : Record16
	{
		[Column]
		public int Value { get; set; }
	}

	[Table]
	class Record16RefReferencingTable : Record
	{
		[ForeignKey.Cascade]
		[Column]
		public RecordRef<Record16Table> Record16 { get; set; }
	}

	[Table]
	class Record16ReferencingTable : Record
	{
		[ForeignKey.Cascade]
		[Column]
		public Record16Table Record16 { get; set; }
	}
}
