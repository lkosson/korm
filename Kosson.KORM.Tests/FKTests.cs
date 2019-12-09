﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;

namespace Kosson.KRUD.Tests
{
	public abstract class FKTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Table1);
			yield return typeof(Table2);
		}

		[TestMethod]
		public void ForeignKeyAcceptsNulls()
		{
			var record = new Table1();
			record.Insert();
		}

		[TestMethod]
		public void ForeignKeyRefAcceptsZero()
		{
			var record = new Table1();
			record.FKRef = 0;
			record.Insert();
		}

		[TestMethod]
		public void ForeignKeyRefAcceptsNull()
		{
			var record = new Table1();
			record.FKRef = null;
			record.Insert();
		}

		[TestMethod]
		public void ForeignKeyCascades()
		{
			var record1 = new Table1();
			var record2 = new Table2();
			record2.Insert();
			record1.FKCascade = record2;
			record1.Insert();
			record2.Delete();
			var retrieved = orm.Select<Table1>().ByID(record1.ID);
			Assert.IsNull(retrieved);
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDForeignKeyException), AllowDerivedTypes = true)]
		public void ForeignKeyPreventsDelete()
		{
			var record1 = new Table1();
			var record2 = new Table2();
			record2.Insert();
			record1.FKNone = record2;
			record1.Insert();
			record2.Delete();
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDException), AllowDerivedTypes = true)]
		public void ForeignKeyViolationThrowsException()
		{
			var record = new Table1();
			record.FKRef = -123;
			record.Insert();
		}

		[TestMethod]
		public void ForeignKeyRefAcceptsID()
		{
			var record1 = new Table1();
			var record2 = new Table2();
			record2.Insert();
			record1.FKRef = record2.ID;
			record1.Insert();
		}

		[TestMethod]
		public void ForeignKeyRetrieved()
		{
			var record1 = new Table1();
			var record2 = new Table2();
			record2.Value = INTMARKER;
			record2.Insert();
			record1.FKCascade = record2;
			record1.Insert();
			var retrieved = orm.Select<Table1>().ByID(record1.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.FKCascade);
			Assert.IsNull(retrieved.FKNone);
			Assert.AreEqual(record2.ID, retrieved.FKCascade.ID);
			Assert.AreEqual(record2.Value, retrieved.FKCascade.Value);
		}

		[TestMethod]
		public void ForeignKeyRetrievesNull()
		{
			var record1 = new Table1();
			record1.Insert();
			var retrieved = orm.Select<Table1>().ByID(record1.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNull(retrieved.FKCascade);
			Assert.IsNull(retrieved.FKNone);
		}

		[TestMethod]
		public void ForeignKeyRefRetrieved()
		{
			var record1 = new Table1();
			var record2 = new Table2();
			record2.Value = INTMARKER;
			record2.Insert();
			record1.FKRef = record2.ID;
			record1.Insert();
			var retrieved = orm.Select<Table1>().ByID(record1.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.FKRef);
			Assert.AreEqual(record2.ID, retrieved.FKRef.ID);
		}


		[Table]
		class Table1 : Record
		{
			[Column]
			[ForeignKey.Cascade]
			public Table2 FKCascade { get; set; }

			[Column]
			[ForeignKey.None]
			public Table2 FKNone { get; set; }

			[Column]
			[ForeignKey.None]
			public RecordRef<Table2> FKRef { get; set; }
		}

		[Table]
		class Table2 : Record
		{
			[Column]
			public int Value { get; set; }
		}
	}
}
