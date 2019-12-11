using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;

namespace Kosson.KRUD.Tests
{
	public abstract class InlineTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Table);
			yield return typeof(Foreign);
		}

		[TestMethod]
		public void NullInlinesAreInserted()
		{
			var record = new Table();
			record.Inline1 = null;
			record.Inline2.Nested = null;
			ORM.Insert(record);
		}

		[TestMethod]
		public void InlinesAreRetrieved()
		{
			var record = new Table();
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.Inline1);
			Assert.IsNotNull(retrieved.Inline2);
			Assert.IsNotNull(retrieved.RenamedInline);
			Assert.IsNotNull(retrieved.Inline1.Nested);
			Assert.IsNotNull(retrieved.Inline2.Nested);
		}

		[TestMethod]
		public void InlinesAreIndependent()
		{
			var record = new Table();
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.AreNotEqual(retrieved.Inline1, retrieved.Inline2);
			Assert.AreNotEqual(retrieved.Inline1, retrieved.RenamedInline);
			Assert.AreNotEqual(retrieved.Inline2, retrieved.RenamedInline);
			Assert.AreNotEqual(retrieved.Inline1.Nested, retrieved.Inline2.Nested);
			Assert.AreNotEqual(retrieved.Inline1.Nested, retrieved.RenamedInline.Nested);
			Assert.AreNotEqual(retrieved.Inline2.Nested, retrieved.RenamedInline.Nested);
		}

		[TestMethod]
		public void InlinesAreInserted()
		{
			var record = new Table();
			record.Inline1.InlinedValue = 1;
			record.Inline2.InlinedValue = 2;
			record.RenamedInline.InlinedValue = 3;
			record.Inline1.Nested.NestedValue = "1";
			record.Inline2.Nested.NestedValue = "2";
			record.RenamedInline.Nested.NestedValue = "3";
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.AreEqual(record.Inline1.InlinedValue, retrieved.Inline1.InlinedValue);
			Assert.AreEqual(record.Inline2.InlinedValue, retrieved.Inline2.InlinedValue);
			Assert.AreEqual(record.RenamedInline.InlinedValue, retrieved.RenamedInline.InlinedValue);
			Assert.AreEqual(record.Inline1.Nested.NestedValue, retrieved.Inline1.Nested.NestedValue);
			Assert.AreEqual(record.Inline2.Nested.NestedValue, retrieved.Inline2.Nested.NestedValue);
			Assert.AreEqual(record.RenamedInline.Nested.NestedValue, retrieved.RenamedInline.Nested.NestedValue);
		}

		[TestMethod]
		public void InlinesAreUpdated()
		{
			var record = new Table();
			ORM.Insert(record);
			record.Inline1.InlinedValue = 1;
			record.Inline2.InlinedValue = 2;
			record.RenamedInline.InlinedValue = 3;
			record.Inline1.Nested.NestedValue = "1";
			record.Inline2.Nested.NestedValue = "2";
			record.RenamedInline.Nested.NestedValue = "3";
			ORM.Update(record);
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.AreEqual(record.Inline1.InlinedValue, retrieved.Inline1.InlinedValue);
			Assert.AreEqual(record.Inline2.InlinedValue, retrieved.Inline2.InlinedValue);
			Assert.AreEqual(record.RenamedInline.InlinedValue, retrieved.RenamedInline.InlinedValue);
			Assert.AreEqual(record.Inline1.Nested.NestedValue, retrieved.Inline1.Nested.NestedValue);
			Assert.AreEqual(record.Inline2.Nested.NestedValue, retrieved.Inline2.Nested.NestedValue);
			Assert.AreEqual(record.RenamedInline.Nested.NestedValue, retrieved.RenamedInline.Nested.NestedValue);
		}

		[TestMethod]
		public void InlineFieldIsRecognizedInQuery()
		{
			var record = new Table();
			record.Inline1.InlinedValue = 1;
			record.Inline2.InlinedValue = 2;
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().WhereFieldEquals("Inline1.InlinedValue", record.Inline1.InlinedValue).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.Inline1.InlinedValue, retrieved.Inline1.InlinedValue);
			Assert.AreEqual(record.ID, retrieved.ID);
		}

		[TestMethod]
		public void InlineRenamedFieldIsRecognizedInQuery()
		{
			var record = new Table();
			record.RenamedInline.InlinedValue = INTMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().WhereFieldEquals("RenamedInline.InlinedValue", record.RenamedInline.InlinedValue).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.RenamedInline.InlinedValue, retrieved.RenamedInline.InlinedValue);
			Assert.AreEqual(record.ID, retrieved.ID);
		}

		[TestMethod]
		public void InlineNestedFieldIsRecognizedInQuery()
		{
			var record = new Table();
			record.RenamedInline.Nested.NestedValue = STRINGMARKER;
			ORM.Insert(record);
			var retrieved = ORM.Select<Table>().WhereFieldEquals("RenamedInline.Nested.NestedValue", record.RenamedInline.Nested.NestedValue).ExecuteFirst();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(record.RenamedInline.Nested.NestedValue, retrieved.RenamedInline.Nested.NestedValue);
			Assert.AreEqual(record.ID, retrieved.ID);
		}

		[TestMethod]
		public void InlineForeignIsRetrieved()
		{
			var foreign1 = new Foreign();
			foreign1.ForeignValue = INTMARKER;
			ORM.Insert(foreign1);

			var foreign2 = new Foreign();
			foreign2.ForeignValue = INTMARKER + 1;
			ORM.Insert(foreign2);

			var record = new Table();
			record.Inline1.Foreign = foreign1;
			record.Inline2.Foreign = foreign2;
			ORM.Insert(record);

			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved.Inline1.Foreign);
			Assert.IsNotNull(retrieved.Inline2.Foreign);
			Assert.IsNull(retrieved.Inline1.Nested.NestedForeign);
			Assert.IsNull(retrieved.Inline2.Nested.NestedForeign);
			Assert.AreEqual(INTMARKER, retrieved.Inline1.Foreign.ForeignValue);
			Assert.AreEqual(INTMARKER + 1, retrieved.Inline2.Foreign.ForeignValue);
		}

		[TestMethod]
		public void InlineNestedForeignIsRetrieved()
		{
			var foreign = new Foreign();
			foreign.ForeignValue = INTMARKER;
			ORM.Insert(foreign);

			var record = new Table();
			record.Inline1.Nested.NestedForeign = foreign;
			ORM.Insert(record);

			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsNotNull(retrieved.Inline1.Nested.NestedForeign);
			Assert.IsNull(retrieved.Inline2.Nested.NestedForeign);
			Assert.IsNull(retrieved.Inline1.Foreign);
			Assert.IsNull(retrieved.Inline2.Foreign);
			Assert.AreEqual(INTMARKER, retrieved.Inline1.Nested.NestedForeign.ForeignValue);
		}

		[Table]
		class Table : Record
		{
			[Inline]
			public Inlined Inline1 { get; set; }

			[Inline]
			public Inlined Inline2 { get; set; }

			[Inline("Renamed")]
			public Inlined RenamedInline { get; set; }

			public Table()
			{
				Inline1 = new Inlined();
				Inline2 = new Inlined();
				RenamedInline = new Inlined();
			}
		}

		class Inlined
		{
			[Inline]
			public NestedInline Nested { get; set; }

			[Column]
			public int InlinedValue { get; set; }

			[Column]
			[ForeignKey.None]
			public Foreign Foreign { get; set; }

			public Inlined()
			{
				Nested = new NestedInline();
			}
		}

		class NestedInline
		{
			[Column]
			public string NestedValue { get; set; }

			[Column]
			[ForeignKey.None]
			public Foreign NestedForeign { get; set; }
		}

		[Table]
		class Foreign : Record
		{
			[Column]
			public int ForeignValue { get; set; }
		}
	}
}
