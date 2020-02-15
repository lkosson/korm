using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract partial class Record8Tests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Record8Table);
			yield return typeof(Record8ReferencingTable);
			yield return typeof(Record8RefReferencingTable);
		}

		[TestMethod]
		public void Record8InsertAssignsID()
		{
			var record = new Record8Table();
			Assert.AreEqual(0, record.ID);
			ORM.Insert(record);
			Assert.AreNotEqual(0, record.ID);
		}

		[TestMethod]
		public void Record8RetrievedByID()
		{
			var record = new Record8Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Record8Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.ID, retrieved.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void Record8RetrievedByRef()
		{
			var record = new Record8Table();
			record.Value = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Record8Table>().ByRef(record.Ref());
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.ID, retrieved.ID);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void Record8RefReferenceIsStored()
		{
			var record1 = new Record8Table();
			ORM.Insert(record1);
			var record2 = new Record8RefReferencingTable();
			record2.Record8 = record1;
			ORM.Store(record2);
			var retrieved = ORM.Select<Record8RefReferencingTable>().ByID(record2.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.Record8);
			Assert.AreEqual(record1.ID, retrieved.Record8.ID);
		}

		[TestMethod]
		public void Record8ReferenceIsStored()
		{
			var record1 = new Record8Table();
			record1.Value = INTMARKER;
			ORM.Insert(record1);
			var record2 = new Record8ReferencingTable();
			record2.Record8 = record1;
			ORM.Store(record2);
			var retrieved = ORM.Select<Record8ReferencingTable>().ByID(record2.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.Record8);
			Assert.AreEqual(record1.ID, retrieved.Record8.ID);
			Assert.AreEqual(INTMARKER, retrieved.Record8.Value);
		}

		[TestMethod]
		[ExpectedException(typeof(OverflowException))]
		public void Record8OutOfRangeValueIsRejected()
		{
			var record = new Record8Table();
			((IRecord)record).ID = (long)Byte.MaxValue + 1;
		}
	}

	[Table]
	class Record8Table : Record8
	{
		[Column]
		public int Value { get; set; }
	}

	[Table]
	class Record8RefReferencingTable : Record
	{
		[ForeignKey.Cascade]
		[Column]
		public RecordRef<Record8Table> Record8 { get; set; }
	}

	[Table]
	class Record8ReferencingTable : Record
	{
		[ForeignKey.Cascade]
		[Column]
		public Record8Table Record8 { get; set; }
	}
}
