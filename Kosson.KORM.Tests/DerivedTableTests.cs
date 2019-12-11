using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;

namespace Kosson.KRUD.Tests
{
	public abstract class DerivedTableTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
			yield return typeof(Table);
		}

		[TestMethod]
		public void DerivedRetrieve()
		{
			var record = new Table();
			record.Value = INTMARKER + 1;
			record.Value2 = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(INTMARKER, retrieved.Value2);
		}

		[TestMethod]
		public void DerivedSeparateStoreRetrieve()
		{
			var recordBase = new MainTestTable();
			ORM.Insert(recordBase);

			var record = new Table();
			record.Value = INTMARKER;
			ORM.Insert(record);

			var retrieved = ORM.Select<Table>().Execute();
			Assert.AreEqual(1, retrieved.Count);
			Assert.AreEqual(INTMARKER, retrieved.First().Value);
		}

		[TestMethod]
		public void DerivedHidingFieldStoreRetrieve()
		{
			var record = new DerivedHidingField();
			record.Value2 = DayOfWeek.Friday;
			ORM.Insert(record);

			var recordAsBase = record as Table;
			Assert.AreEqual(0, recordAsBase.Value2);

			var retrieved = ORM.Select<DerivedHidingField>().ByID(record.ID);
			var retrievedAsBase = retrieved as Table;

			Assert.IsNotNull(retrieved);
			Assert.AreEqual(DayOfWeek.Friday, retrieved.Value2);

			Assert.IsNotNull(retrievedAsBase);
			Assert.AreEqual(0, retrievedAsBase.Value2);
		}

		[TestMethod]
		public void DerivedHidingStoreRetrieve()
		{
			var record = new DerivedHidingField();
			record.Value2 = DayOfWeek.Friday;
			ORM.Insert(record);

			var retrievedBase = ORM.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrievedBase);
			Assert.AreEqual((int)DayOfWeek.Friday, retrievedBase.Value2);

			retrievedBase.Value2 = (int)DayOfWeek.Tuesday;
			ORM.Store(retrievedBase);

			var retrievedDerived = ORM.Select<DerivedHidingField>().ByID(record.ID);
			Assert.IsNotNull(retrievedDerived);
			Assert.AreEqual(DayOfWeek.Tuesday, retrievedDerived.Value2);
		}

		[TestMethod]
		public void BaseOfDerivedHidingStoreRetrieve()
		{
			Table /* note type change */ record = new DerivedHidingField();
			record.Value2 = INTMARKER;
			ORM.Insert(record);

			var retrievedBase = ORM.Select<Table>().ByID(record.ID);
			var retrievedDerived = ORM.Select<DerivedHidingField>().ByID(record.ID);

			Assert.IsNotNull(retrievedBase);
			Assert.IsNotNull(retrievedDerived);
			Assert.AreEqual((DayOfWeek)INTMARKER, retrievedDerived.Value2);
			Assert.AreEqual(INTMARKER, retrievedBase.Value2);
		}

		[TestMethod]
		public void DerivedLinkedStoreRetrieve()
		{
			var referenced = new Table();
			referenced.Value = INTMARKER;
			ORM.Insert(referenced);

			var record = new DerivedLinked();
			record.Value2Record = referenced;
			ORM.Insert(record);

			var retrievedBase = ORM.Select<Table>().ByID(record.ID);
			var retrievedDerived = ORM.Select<DerivedLinked>().ByID(record.ID);
			var retrievedDerivedAsBase = retrievedDerived as Table;

			Assert.IsNotNull(retrievedBase);
			Assert.IsNotNull(retrievedDerived);
			Assert.IsNotNull(retrievedDerivedAsBase);
			Assert.IsNotNull(retrievedDerived.Value2Record);
			Assert.AreEqual(referenced.ID, retrievedDerived.Value2Record.ID);
			Assert.AreEqual(referenced.Value, retrievedDerived.Value2Record.Value);
			Assert.AreEqual(referenced.ID, retrievedDerived.Value2);
			Assert.AreEqual(referenced.ID, retrievedBase.Value2);
			Assert.AreEqual(referenced.ID, retrievedDerivedAsBase.Value2);
		}

		[TestMethod]
		public void DerivedLinkedBaseStoreRetrieve()
		{
			var referenced = new Table();
			referenced.Value = INTMARKER;
			ORM.Insert(referenced);

			Table /* note type change */ record = new DerivedLinked();
			record.Value2 = (int)referenced.ID;
			ORM.Insert(record);

			var retrievedBase = ORM.Select<Table>().ByID(record.ID);
			var retrievedDerived = ORM.Select<DerivedLinked>().ByID(record.ID);
			var retrievedDerivedAsBase = retrievedDerived as Table;

			Assert.IsNotNull(retrievedBase);
			Assert.IsNotNull(retrievedDerived);
			Assert.IsNotNull(retrievedDerivedAsBase);
			Assert.IsNotNull(retrievedDerived.Value2Record);
			Assert.AreEqual(referenced.ID, retrievedDerived.Value2Record.ID);
			Assert.AreEqual(referenced.Value, retrievedDerived.Value2Record.Value);
			Assert.AreEqual(referenced.ID, retrievedDerived.Value2);
			Assert.AreEqual(referenced.ID, retrievedBase.Value2);
			Assert.AreEqual(referenced.ID, retrievedDerivedAsBase.Value2);
		}

		[TestMethod]
		public void DerivedSameStoreRetrieve()
		{
			var record = new DerivedSameStoreTable();
			record.Value = INTMARKER + 1;
			record.Value2 = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<DerivedSameStoreTable>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2 * INTMARKER + 1, retrieved.ValuesAdded);
		}

		[TestMethod]
		public void DerivedReadOnlyUpdate()
		{
			var record = new DerivedReadOnly();
			Assert.AreEqual(0, record.ReadOnly);
			record.ReadOnly = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<DerivedReadOnly>().ByID(record.ID);
			Assert.AreEqual(0, retrieved.Value);

			var retrievedBase = ORM.Select<Table>().ByID(record.ID);
			Assert.AreEqual(0, retrievedBase.Value);
		}

		[Table]
		class Table : MainTestTable
		{
			[Column]
			public virtual int Value2 { get; set; }
		}

		class DerivedSameStoreTable : Table
		{
			public int ValuesAdded { get { return Value + Value2; } }
		}

		class DerivedHidingField : Table
		{
			[Column]
			public new DayOfWeek Value2 { get; set; }
		}

		class DerivedReadOnly : Table
		{
			[Column(IsReadOnly = true)]
			public override int Value2 { get { return base.Value2; } set { base.Value2 = value; } }
		}

		class DerivedLinked : Table
		{
			[Column(IsReadOnly = true)]
			public override int Value2 { get { return (int)(RecordRef<Table>)Value2Record; } set { Value2Record = new Table() { ID = value }; } }

			[Column]
			[DBName("dttt_Value2")]
			public Table Value2Record { get; set; }
		}
	}
}
