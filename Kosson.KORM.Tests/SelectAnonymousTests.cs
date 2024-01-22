using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract partial class SelectAnonymousTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(TestTable);
			yield return typeof(ReferencedTable);
		}

		private TestTable PrepareTestTables()
		{
			var referenced = new ReferencedTable();
			referenced.Value = INTMARKER - 1;
			ORM.Insert(referenced);

			var inserted = new TestTable();
			inserted.Value = INTMARKER;
			inserted.Referenced = referenced;
			inserted.InlinedDirectly.InlinedReferenced = referenced;
			inserted.InlinedDirectly.DoubleInline.Value = "A";
			ORM.Insert(inserted);

			inserted.SelfReferencedRef = inserted;
			ORM.Update(inserted);

			return inserted;
		}

		[TestMethod]
		public void SelectIDField()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.ID).ExecuteFirst();
			Assert.AreEqual(inserted.ID, retrieved);
		}

		[TestMethod]
		public void SelectValueField()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.Value).ExecuteFirst();
			Assert.AreEqual(inserted.Value, retrieved);
		}

		[TestMethod]
		public void SelectRefField()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.SelfReferencedRef).ExecuteFirst();
			Assert.AreEqual(inserted.SelfReferencedRef, retrieved);
		}

		[TestMethod]
		public void SelectInlinedField()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.InlinedDirectly.InlinedReferenced).ExecuteFirst();
			Assert.AreEqual(inserted.InlinedDirectly.InlinedReferenced, retrieved);
		}

		[TestMethod]
		public void SelectReferencedID()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.Referenced.ID).ExecuteFirst();
			Assert.AreEqual(inserted.Referenced.ID, retrieved);
		}

		[TestMethod]
		public void SelectReferencedField()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.Referenced.Value).ExecuteFirst();
			Assert.AreEqual(inserted.Referenced.Value, retrieved);
		}

		[TestMethod]
		public void SelectReferencedInlinedField()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.Referenced.InlinedInirectly.InlinedReferenced).ExecuteFirst();
			Assert.AreEqual(inserted.Referenced.InlinedInirectly.InlinedReferenced, retrieved);
		}

		[TestMethod]
		public void SelectReferencedDoubleInlinedField()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.Referenced.InlinedInirectly.DoubleInline.Value).ExecuteFirst();
			Assert.AreEqual(inserted.Referenced.InlinedInirectly.DoubleInline.Value, retrieved);
		}

		[TestMethod]
		public void SelectRecord()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t).ExecuteFirst();
			Assert.AreEqual(inserted, retrieved);
		}

		[TestMethod]
		public void SelectInlinedObject()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.InlinedDirectly).ExecuteFirst();
			Assert.AreEqual(inserted.InlinedDirectly, retrieved);
		}

		[TestMethod]
		public void SelectInlinedReferencedObject()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.InlinedDirectly.InlinedReferenced).ExecuteFirst();
			Assert.AreEqual(inserted.InlinedDirectly.InlinedReferenced, retrieved);
		}

		[TestMethod]
		public void SelectReferencedObject()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => t.Referenced).ExecuteFirst();
			Assert.AreEqual(inserted.Referenced, retrieved);
		}

		[TestMethod]
		public void SelectValueFieldAsItem()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => new { t.Value }).ExecuteFirst();
			Assert.AreEqual(inserted.Value, retrieved.Value);
		}

		[TestMethod]
		public void SelectValueAndID()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => new { t.Value, t.ID }).ExecuteFirst();
			Assert.AreEqual(inserted.Value, retrieved.Value);
			Assert.AreEqual(inserted.ID, retrieved.ID);
		}

		[TestMethod]
		public void SelectValueAndInline()
		{
			var inserted = PrepareTestTables();
			var retrieved = ORM.Select<TestTable>().Select(t => new { t.Value, t.InlinedDirectly }).ExecuteFirst();
			Assert.AreEqual(inserted.Value, retrieved.Value);
			Assert.AreEqual(inserted.InlinedDirectly, retrieved.InlinedDirectly);
		}

		[Table]
		class TestTable : Record
		{
			[Column]
			public int Value { get; set; }

			[Column]
			[ForeignKey.None]
			public RecordRef<TestTable> SelfReferencedRef { get; set; }

			[Column]
			[ForeignKey.None]
			public ReferencedTable Referenced { get; set; }

			[Inline]
			public Inline InlinedDirectly { get; set; } = new Inline();

			public long InstanceMethod() => ID;

			public override bool Equals(object obj)
			{
				if (obj is not TestTable other) return false;
				return other.Value == Value && other.SelfReferencedRef == SelfReferencedRef && other.InlinedDirectly.Equals(InlinedDirectly);
			}

			public override int GetHashCode() => base.GetHashCode();
		}

		[Table]
		class ReferencedTable : Record
		{
			[Column]
			public int Value { get; set; }

			[Inline]
			public Inline InlinedInirectly { get; set; } = new Inline();

			public override bool Equals(object obj)
			{
				if (obj is not ReferencedTable other) return false;
				return other.Value == Value && other.InlinedInirectly.Equals(InlinedInirectly);
			}

			public override int GetHashCode() => base.GetHashCode();

		}

		class Inline
		{
			[Column]
			[ForeignKey.None]
			public RecordRef<ReferencedTable> InlinedReferenced { get; set; }

			[Inline]
			public DoubleInline DoubleInline { get; set; } = new DoubleInline();

			public override bool Equals(object obj)
			{
				if (obj is not Inline other) return false;
				return other.InlinedReferenced == InlinedReferenced && other.DoubleInline.Equals(DoubleInline);
			}

			public override int GetHashCode() => base.GetHashCode();
		}

		class DoubleInline
		{
			[Column]
			public string Value { get; set; }

			public override bool Equals(object obj)
			{
				if (obj is not DoubleInline other) return false;
				return other.Value == Value;
			}

			public override int GetHashCode() => base.GetHashCode();
		}
	}
}
