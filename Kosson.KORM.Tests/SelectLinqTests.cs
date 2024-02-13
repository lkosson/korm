using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract partial class SelectLinqTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(LinqTestTable);
			yield return typeof(LinqReferencedTable);
		}

		private LinqTestTable[] PrepareTestTables()
		{
			var referenced1 = new LinqReferencedTable();
			referenced1.Value = INTMARKER - 1;
			ORM.Insert(referenced1);

			var referenced2 = new LinqReferencedTable();
			referenced2.Value = INTMARKER - 2;
			ORM.Insert(referenced2);

			var inserted1 = new LinqTestTable();
			inserted1.Value = INTMARKER;
			inserted1.Referenced = referenced1;
			inserted1.InlinedDirectly.InlinedReferenced = referenced1;
			inserted1.InlinedDirectly.DoubleInline.Value = "A";
			inserted1.DirectBoolean = true;
			ORM.Insert(inserted1);

			var inserted2 = new LinqTestTable();
			inserted2.Value = INTMARKER + 1;
			inserted2.Referenced = referenced2;
			inserted2.InlinedDirectly.InlinedReferenced = referenced1;
			inserted2.InlinedDirectly.DoubleInline.Value = "AA";
			inserted1.DirectBoolean = false;
			ORM.Insert(inserted2);

			var inserted3 = new LinqTestTable();
			inserted3.Value = INTMARKER + 2;
			inserted3.Referenced = referenced1;
			inserted3.SelfReferencedRef = inserted1;
			inserted3.InlinedDirectly.InlinedReferenced = referenced2;
			inserted3.InlinedDirectly.DoubleInline.Value = "AAA";
			inserted1.DirectBoolean = false;
			ORM.Insert(inserted3);

			var inserted4 = new LinqTestTable();
			inserted4.Value = INTMARKER + 3;
			inserted4.Referenced = referenced2;
			inserted4.SelfReferencedRef = inserted2;
			inserted4.InlinedDirectly.InlinedReferenced = referenced2;
			inserted4.InlinedDirectly.DoubleInline.Value = "AAAA";
			inserted1.DirectBoolean = true;
			ORM.Insert(inserted4);

			return new[] { inserted1, inserted2, inserted3, inserted4 };
		}

		[TestMethod]
		public void SelectLinqByTrueCondition()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => 1 == 1).Execute();
			Assert.AreEqual(inserted.Length, retrieved.Count);
		}

		[TestMethod]
		public void SelectLinqByFalseCondition()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => 1 != 1).Execute();
			Assert.AreEqual(0, retrieved.Count);
		}

		[TestMethod]
		public void SelectLinqByConstant()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.Value == INTMARKER).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void SelectLinqByDirectBoolean()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.DirectBoolean).Execute();
			Assert.AreEqual(inserted.Count(t => t.DirectBoolean), retrieved.Count);
			Assert.IsTrue(retrieved.All(t => t.DirectBoolean));
		}

		[TestMethod]
		public void SelectLinqByNegatedBoolean()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => !t.DirectBoolean).Execute();
			Assert.AreEqual(inserted.Count(t => !t.DirectBoolean), retrieved.Count);
			Assert.IsTrue(retrieved.All(t => !t.DirectBoolean));
		}
		[TestMethod]
		public void SelectLinqByNull()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.SelfReferencedRef == null).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2, retrieved.Count);
			Assert.IsTrue(retrieved.All(t => t.SelfReferencedRef.IsNull));
		}

		[TestMethod]
		public void SelectLinqByNotNull()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.SelfReferencedRef != null).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2, retrieved.Count);
			Assert.IsTrue(retrieved.All(t => t.SelfReferencedRef.IsNotNull));
		}

		[TestMethod]
		public void SelectLinqByVariable()
		{
			var inserted = PrepareTestTables();
			var id = inserted[1].ID;
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.ID == id).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(id, retrieved.ID);
		}

		[TestMethod]
		public void SelectLinqByLocalComputedValue()
		{
			var inserted = PrepareTestTables();
			var id = inserted[1].ID;
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.ID < id + 100).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.Length, retrieved.Count);
		}

		[TestMethod]
		public void SelectLinqByStaticMethodCallValue()
		{
			var inserted = PrepareTestTables();
			var id = inserted[2].ID;
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.ID == Math.Abs(-id)).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
			Assert.AreEqual(id, retrieved.Single().ID);
		}

		[TestMethod]
		public void SelectLinqByInstanceMethodCallValue()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.ID == inserted[2].InstanceMethod()).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
			Assert.AreEqual(retrieved.Single().InstanceMethod(), retrieved.Single().ID);
		}

		[TestMethod]
		public void SelectLinqByDatabaseComputedValue()
		{
			var inserted = PrepareTestTables();
			var id = inserted[1].ID;
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.Value - 2 == INTMARKER).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
			Assert.AreEqual(INTMARKER, retrieved.Single().Value - 2);
		}

		[TestMethod]
		public void SelectLinqOperatorPrecedence()
		{
			var inserted = PrepareTestTables();
			var id = inserted[1].ID;
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.Value - 2 * (t.Value - t.Value) == INTMARKER).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
			Assert.AreEqual(INTMARKER, retrieved.Single().Value);
		}

		[TestMethod]
		public void SelectLinqByArrayValue()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.ID == inserted[1].ID).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted[1].ID, retrieved.ID);
		}

		[TestMethod]
		public void SelectLinqByLocalComplexProperty()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.Value != inserted[2].InlinedDirectly.InlinedReferenced.ID).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(inserted.Length, retrieved.Count);
		}

		[TestMethod]
		public void SelectLinqByRecordRef()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.SelfReferencedRef == inserted[0]).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
			Assert.AreEqual(inserted[0].ID, retrieved.Single().SelfReferencedRef);
		}

		[TestMethod]
		public void SelectLinqByForeignRef()
		{
			var inserted = PrepareTestTables();
			var foreign = inserted[2].Referenced;
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.Referenced == foreign).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2, retrieved.Count);
			Assert.IsTrue(retrieved.All(t => t.Referenced == foreign));
		}

		[TestMethod]
		public void SelectLinqByInlinedValue()
		{
			var inserted = PrepareTestTables();
			var foreign = inserted[2].Referenced;
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.InlinedDirectly.InlinedReferenced == foreign).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2, retrieved.Count);
			Assert.IsTrue(retrieved.All(t => t.InlinedDirectly.InlinedReferenced == foreign));
		}

		[TestMethod]
		public void SelectLinqByDoubleInlinedValue()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.InlinedDirectly.DoubleInline.Value == "AAA").Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
			Assert.AreEqual("AAA", retrieved.Single().InlinedDirectly.DoubleInline.Value);
		}
		/*
		[TestMethod]
		public void SelectLinqByForeignField()
		{
			var inserted = PrepareTestTables();
			var value = inserted[3].Referenced.Value;
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.Referenced.Value == value).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
			Assert.IsTrue(retrieved.All(t => t.Referenced.Value == value));
		}
		*/
		[TestMethod]
		public void SelectLinqByAlternative()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.Value == INTMARKER || t.Value == INTMARKER + 2).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(2, retrieved.Count);
			Assert.IsTrue(retrieved.All(r => r.Value == INTMARKER || r.Value == INTMARKER + 2));
		}

		[TestMethod]
		public void SelectLinqByConjunction()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => t.Value == INTMARKER && t.Value == INTMARKER + 2).Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(0, retrieved.Count);
		}

		[TestMethod]
		public void SelectLinqByNegation()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<LinqTestTable>().Where(t => !(t.Value == INTMARKER)).Execute();
			Assert.IsNotNull(retrieved);
			Assert.IsTrue(retrieved.All(t => t.Value != INTMARKER));
		}

		[Table]
		class LinqTestTable : Record
		{
			[Column]
			public int Value { get; set; }

			[Column]
			[ForeignKey.None]
			public RecordRef<LinqTestTable> SelfReferencedRef { get; set; }

			[Column]
			[ForeignKey.None]
			public LinqReferencedTable Referenced { get; set; }

			[Column]
			public bool DirectBoolean { get; set; }

			[Inline]
			public LinqInline InlinedDirectly { get; set; } = new LinqInline();

			public long InstanceMethod() => ID;
		}

		[Table]
		class LinqReferencedTable : Record
		{
			[Column]
			public int Value { get; set; }

			[Inline]
			public LinqInline InlinedInirectly { get; set; } = new LinqInline();
		}

		class LinqInline
		{
			[Column]
			[ForeignKey.None]
			public RecordRef<LinqReferencedTable> InlinedReferenced { get; set; }

			[Inline]
			public LinqDoubleInline DoubleInline { get; set; } = new LinqDoubleInline();
		}

		class LinqDoubleInline
		{
			[Column]
			public string Value { get; set; }
		}
	}
}
