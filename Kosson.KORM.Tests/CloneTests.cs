using Kosson.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD.Tests
{
	// Not an abstract class - no need for per-provider tests.
	[TestClass]
	public class CloneTests : ORMTestsBase
	{
		protected override bool NeedsDatabase { get { return false; } }
		protected override string Provider { get { return "empty"; } }

		protected override IEnumerable<Type> Tables()
		{
			return Enumerable.Empty<Type>();
		}

		[TestMethod]
		public void CloneIsCreated()
		{
			var record = new Table();
			var clone = record.Clone();
			Assert.IsNotNull(clone);
			Assert.AreNotSame(record, clone);
		}

		[TestMethod]
		public void PropertiesAreCloned()
		{
			var record = new Table
			{
				ID = 12345,
				ValueEnum = DayOfWeek.Thursday,
				ValueBool = true,
				ValueString = "MARKER",
				ValueBlob = Encoding.UTF8.GetBytes("BLOB"),
				ValueRecord = new Table(),
				ValueRecordRef = new RecordRef<Table>(54321),
				ValueInline = new MainTestTable() { ID = 112233 }
			};
			var clone = record.Clone();
			Assert.AreEqual(record.ID, clone.ID);
			Assert.AreEqual(record.ValueEnum, clone.ValueEnum);
			Assert.AreEqual(record.ValueBool, clone.ValueBool);
			Assert.AreEqual(record.ValueString, clone.ValueString);
			Assert.AreEqual(record.ValueBlob, clone.ValueBlob);
			Assert.AreEqual(record.ValueRecord, clone.ValueRecord);
			Assert.AreEqual(record.ValueRecordRef, clone.ValueRecordRef);
			Assert.AreNotSame(record.ValueInline, clone.ValueInline);
		}

		[TestMethod]
		public void ClonedRecordIsIndependent()
		{
			var blob = Encoding.UTF8.GetBytes("BLOB");
			var table = new Table();
			var record = new Table
			{
				ID = 12345,
				ValueEnum = DayOfWeek.Thursday,
				ValueBool = true,
				ValueString = "MARKER",
				ValueBlob = blob,
				ValueRecord = table,
				ValueRecordRef = new RecordRef<Table>(54321),
				ValueInline = new MainTestTable() { ID = 112233 }
			};
			var clone = record.Clone();

			record.ID = 0;
			record.ValueEnum = DayOfWeek.Monday;
			record.ValueBool = false;
			record.ValueString = "";
			record.ValueBlob = new byte[0];
			record.ValueRecord = null;
			record.ValueRecordRef = 0;
			record.ValueInline = new MainTestTable();

			Assert.AreEqual(12345, clone.ID);
			Assert.AreEqual(DayOfWeek.Thursday, clone.ValueEnum);
			Assert.AreEqual(true, clone.ValueBool);
			Assert.AreEqual("MARKER", clone.ValueString);
			Assert.AreEqual(blob, clone.ValueBlob);
			Assert.AreEqual(table, clone.ValueRecord);
			Assert.AreEqual(new RecordRef<Table>(54321), clone.ValueRecordRef);
			Assert.AreEqual(112233, clone.ValueInline.ID);
		}

		[TestMethod]
		public void ReferencedRecordsAreKept()
		{
			var table = new Table();
			var record = new Table { ValueRecord = table };
			var clone = record.Clone();

			Assert.AreEqual(table, clone.ValueRecord);
		}

		[TestMethod]
		public void InlinesAreCloned()
		{
			var inline = new MainTestTable { Value = 12345 };
			var record = new Table { ValueInline = inline };
			var clone = record.Clone();

			Assert.AreNotSame(inline, clone.ValueInline);
			Assert.AreEqual(inline.Value, clone.ValueInline.Value);
		}

		class Table : Record
		{
			[Column]
			public DayOfWeek ValueEnum { get; set; }

			[Column]
			public bool ValueBool { get; set; }

			[Column]
			public string ValueString { get; set; }

			[Column]
			public byte[] ValueBlob { get; set; }

			[Column]
			public Table ValueRecord { get; set; }

			[Column]
			public RecordRef<Table> ValueRecordRef { get; set; }

			[Inline]
			public MainTestTable ValueInline { get; set; }
		}
	}
}
