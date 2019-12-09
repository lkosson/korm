using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract partial class BackupTests : ORMTestsBase
	{
		[TestInitialize]
		public override void Init()
		{
			base.Init();
			Context.Current.Add<IPropertyBinder>(new Kosson.Kore.PropertyBinder.ReflectionPropertyBinder());
			Context.Current.Add<IBackupProvider>(new Kosson.KRUD.BackupProvider());
		}

		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
			yield return typeof(TableReferenced);
			yield return typeof(TableReferencing);
			yield return typeof(TableCyclic1);
			yield return typeof(TableCyclic2);
		}

		[TestMethod]
		public void BackupCompletes()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();
			using (var set = provider.CreateBackupSet(writer))
			{
			}
		}

		[TestMethod]
		public void RecordsAreBackedUp()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();
			var r1 = new MainTestTable { ID = MainTestTable.DEFAULTVALUE };
			var r2 = new TableReferenced { ID = MainTestTable.DEFAULTVALUE };
			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddRecords(new Record[] { r1, r2 });
			}
			Assert.IsTrue(writer.Records.Contains(r1));
			Assert.IsTrue(writer.Records.Contains(r2));
		}

		[TestMethod]
		public void TablesAreBackedUp()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();
			new MainTestTable().Store();
			new MainTestTable().Store();
			new TableReferenced().Store();
			new TableReferenced().Store();
			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddTable(typeof(MainTestTable));
				set.AddTable(typeof(TableReferenced));
			}
			Assert.AreEqual(4, writer.Records.Count);
			Assert.IsTrue(writer.Records.Select(r => r.ID).Min() > 0);
		}

		[TestMethod]
		public void ReferencedRecordsAreIncluded()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();
			var r1 = new TableReferenced();
			var r2 = new TableReferenced();
			r1.Store();
			r2.Store();
			var r3 = new TableReferencing { FKNone = r1 };
			var r4 = new TableReferencing { FKRef = r1 };
			r3.Store();
			r4.Store();
			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddTable(typeof(TableReferencing));
			}
			Assert.IsTrue(writer.Records.Contains(r1));
			// no contract on whether r2 should be included or not
			Assert.IsTrue(writer.Records.Contains(r3));
			Assert.IsTrue(writer.Records.Contains(r4));
		}

		[TestMethod]
		public void CyclicReferencedRecordsAreIncluded()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();
			var r1 = new TableCyclic1();
			var r2 = new TableCyclic1();
			r1.Store();
			r2.Store();
			var r3 = new TableCyclic2();
			var r4 = new TableCyclic2(); 
			r3.Store();
			r4.Store();

			r1.Other = r3;
			r1.Self = r2;
			r2.Other = r4;
			r2.Self = r1;
			r3.Other = r2;
			r4.Other = r1;

			r1.Store();
			r2.Store();
			r3.Store();
			r4.Store();

			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddTable(typeof(TableCyclic1));
			}
			Assert.IsTrue(writer.Records.Contains(r1));
			Assert.IsTrue(writer.Records.Contains(r2));
			Assert.IsTrue(writer.Records.Contains(r3));
			Assert.IsTrue(writer.Records.Contains(r4));
		}

		[TestMethod]
		public void TableIsCleared()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			new MainTestTable().Store();
			new MainTestTable().Store();
			provider.ClearTables(new[] { typeof(MainTestTable) });
			var res = Context.Current.Get<IORM>().Select<MainTestTable>().Execute();
			Assert.AreEqual(0, res.Count);
		}

		[TestMethod]
		public void ReferencingTableIsCleared()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var r1 = new TableReferenced();
			var r2 = new TableReferenced();
			r1.Store();
			r2.Store();
			var r3 = new TableReferencing();
			r3.FKNone = r1;
			r3.Store();

			provider.ClearTables(new[] { typeof(TableReferenced), typeof(TableReferencing) });
			var res = Context.Current.Get<IORM>().Select<TableReferencing>().Execute();
			Assert.AreEqual(0, res.Count);
		}

		[TestMethod]
		public void RecordsAreRestored()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();
			var r1 = new MainTestTable { Value = MainTestTable.DEFAULTVALUE };
			var r2 = new TableReferenced { Value = MainTestTable.DEFAULTVALUE };
			r1.Store();
			r2.Store();
			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddRecords(new Record[] { r1, r2 });
			}

			provider.ClearTables(new[] { typeof(MainTestTable), typeof(TableReferenced) });

			var reader = writer.CreateReader();
			provider.Restore(reader);
			var res1 = Context.Current.Get<IORM>().Select<MainTestTable>().Execute();
			var res2 = Context.Current.Get<IORM>().Select<TableReferenced>().Execute();
			Assert.AreEqual(1, res1.Count);
			Assert.AreEqual(1, res2.Count);
			Assert.AreEqual(r1.Value, res1.First().Value);
			Assert.AreEqual(r2.Value, res2.First().Value);
		}

		[TestMethod]
		public void TablesAreRestored()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();
			new MainTestTable().Store();
			new MainTestTable().Store();
			new TableReferenced().Store();
			new TableReferenced().Store();
			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddTable(typeof(MainTestTable));
				set.AddTable(typeof(TableReferenced));
			}

			provider.ClearTables(new[] { typeof(MainTestTable), typeof(TableReferenced) });

			var reader = writer.CreateReader();
			provider.Restore(reader);
			var res1 = Context.Current.Get<IORM>().Select<MainTestTable>().Execute();
			var res2 = Context.Current.Get<IORM>().Select<TableReferenced>().Execute();
			Assert.AreEqual(2, res1.Count);
			Assert.AreEqual(2, res2.Count);
		}

		[TestMethod]
		public void ReferencedRecordsAreRestored()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();
			var r1 = new TableReferenced();
			var r2 = new TableReferenced();
			r1.Store();
			r2.Store();
			var r3 = new TableReferencing { FKNone = r1 };
			var r4 = new TableReferencing { FKRef = r1 };
			r3.Store();
			r4.Store();
			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddTable(typeof(TableReferencing));
			}

			provider.ClearTables(new[] { typeof(TableReferencing), typeof(TableReferenced) });

			var reader = writer.CreateReader();
			provider.Restore(reader);

			var res1 = Context.Current.Get<IORM>().Select<TableReferencing>().Execute();
			var res2 = Context.Current.Get<IORM>().Select<TableReferenced>().Execute();
			Assert.AreEqual(2, res1.Count);
			Assert.AreEqual(2, res2.Count);
			Assert.IsTrue(res2.Any(r => res1.Any(rx => rx.FKNone.ID == r.ID)));
			Assert.IsTrue(res2.Any(r => res1.Any(rx => rx.FKRef.ID == r.ID)));
		}

		[TestMethod]
		public void CyclicReferencedRecordsAreRestored()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();
			var r1 = new TableCyclic1();
			var r2 = new TableCyclic1();
			r1.Store();
			r2.Store();
			var r3 = new TableCyclic2();
			var r4 = new TableCyclic2();
			r3.Store();
			r4.Store();

			r1.Other = r3;
			r1.Self = r2;
			r1.Value = INTMARKER;
			r2.Other = r4;
			r2.Self = r1;
			r2.Value = INTMARKER + 1;
			r3.Other = r2;
			r3.Value = INTMARKER + 2;
			r4.Other = r1;
			r4.Value = INTMARKER + 3;

			r1.Store();
			r2.Store();
			r3.Store();
			r4.Store();

			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddTable(typeof(TableCyclic1));
			}

			provider.ClearTables(new[] { typeof(TableCyclic1), typeof(TableCyclic2) });

			var reader = writer.CreateReader();
			provider.Restore(reader);

			var res1 = Context.Current.Get<IORM>().Select<TableCyclic1>().Execute();
			var res2 = Context.Current.Get<IORM>().Select<TableCyclic2>().Execute();

			Assert.AreEqual(2, res1.Count);
			Assert.AreEqual(2, res2.Count);

			var rr1 = res1.First(r => r.Value == INTMARKER);
			var rr2 = res1.First(r => r.Value == INTMARKER + 1);
			var rr3 = res2.First(r => r.Value == INTMARKER + 2);
			var rr4 = res2.First(r => r.Value == INTMARKER + 3);

			Assert.AreEqual(rr1.Other, rr3);
			Assert.AreEqual(rr1.Self, rr2);
			Assert.AreEqual(rr2.Other, rr4);
			Assert.AreEqual(rr2.Self, rr1);
			Assert.AreEqual(rr3.Other, rr2);
			Assert.AreEqual(rr4.Other, rr1);
		}

		[TestMethod]
		public void CyclicReferencedRecordsAreRestoredXML()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var ms = new MemoryStream();
			var writer = new XMLBackupWriter(ms);
			var r1 = new TableCyclic1();
			var r2 = new TableCyclic1();
			r1.Store();
			r2.Store();
			var r3 = new TableCyclic2();
			var r4 = new TableCyclic2();
			r3.Store();
			r4.Store();

			r1.Other = r3;
			r1.Self = r2;
			r1.Value = INTMARKER;
			r2.Other = r4;
			r2.Self = r1;
			r2.Value = INTMARKER + 1;
			r3.Other = r2;
			r3.Value = INTMARKER + 2;
			r4.Other = r1;
			r4.Value = INTMARKER + 3;

			r1.Store();
			r2.Store();
			r3.Store();
			r4.Store();

			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddTable(typeof(TableCyclic1));
			}

			provider.ClearTables(new[] { typeof(TableCyclic1), typeof(TableCyclic2) });

			ms.Position = 0;
			var reader = new XMLBackupReader(ms);
			provider.Restore(reader);

			var res1 = Context.Current.Get<IORM>().Select<TableCyclic1>().Execute();
			var res2 = Context.Current.Get<IORM>().Select<TableCyclic2>().Execute();

			Assert.AreEqual(2, res1.Count);
			Assert.AreEqual(2, res2.Count);

			var rr1 = res1.First(r => r.Value == INTMARKER);
			var rr2 = res1.First(r => r.Value == INTMARKER + 1);
			var rr3 = res2.First(r => r.Value == INTMARKER + 2);
			var rr4 = res2.First(r => r.Value == INTMARKER + 3);

			Assert.AreEqual(rr1.Other, rr3);
			Assert.AreEqual(rr1.Self, rr2);
			Assert.AreEqual(rr2.Other, rr4);
			Assert.AreEqual(rr2.Self, rr1);
			Assert.AreEqual(rr3.Other, rr2);
			Assert.AreEqual(rr4.Other, rr1);
		}

		[TestMethod]
		public void DuplicatedIDsAreReplaced()
		{
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();

			var r = new MainTestTable();
			r.Store();

			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddRecords(new Record[] { r });
			}

			var reader = writer.CreateReader();
			provider.Restore(reader);
			var res = Context.Current.Get<IORM>().Select<MainTestTable>().Execute();

			Assert.AreEqual(2, res.Count);
			Assert.IsNotNull(res.ByID(r.ID));
			Assert.IsNotNull(res.FirstOrDefault(rec => rec.ID != r.ID));
		}

		[TestMethod]
		public void PrimaryKeyIsRestoredWhereSupported()
		{
			if (!Context.Current.Get<IDB>().CommandBuilder.SupportsPrimaryKeyInsert) Assert.Inconclusive("Database does not support specifying primary key value.");
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();

			var r1 = new MainTestTable() { ID = 101, Value = 1 };
			var r2 = new MainTestTable() { ID = 102, Value = 2 };
			var r3 = new MainTestTable() { ID = 103, Value = 3 };
			var r4 = new MainTestTable() { ID = 104, Value = 4 };
			var r5 = new MainTestTable() { ID = 105, Value = 5 };

			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddRecords(new Record[] { r2, r3, r4, r1, r5 });
			}

			provider.ClearTables(new[] { typeof(MainTestTable) });

			var reader = writer.CreateReader();
			provider.Restore(reader);
			var res = Context.Current.Get<IORM>().Select<MainTestTable>().Execute();
			Assert.AreEqual(5, res.Count);
			Assert.IsNotNull(res.ByID(101));
			Assert.IsNotNull(res.ByID(102));
			Assert.IsNotNull(res.ByID(103));
			Assert.IsNotNull(res.ByID(104));
			Assert.IsNotNull(res.ByID(105));
			Assert.AreEqual(1, res.ByID(101).Value);
			Assert.AreEqual(2, res.ByID(102).Value);
			Assert.AreEqual(3, res.ByID(103).Value);
			Assert.AreEqual(4, res.ByID(104).Value);
			Assert.AreEqual(5, res.ByID(105).Value);
		}

		[TestMethod]
		public void PrimaryKeyLowerThanCurrentIsRestoredWhereSupported()
		{
			if (!Context.Current.Get<IDB>().CommandBuilder.SupportsPrimaryKeyInsert) Assert.Inconclusive("Database does not support specifying primary key value.");
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();

			var r1 = new MainTestTable() { Value = 1 };
			var r2 = new MainTestTable() { Value = 2 };
			var r3 = new MainTestTable() { Value = 3 };
			var r4 = new MainTestTable() { Value = 4 };
			var r5 = new MainTestTable() { Value = 5 };

			orm.Insert<MainTestTable>().Records(new MainTestTable[] { r2, r3, r4, r1, r5 });

			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddTable(typeof(MainTestTable));
			}

			provider.ClearTables(new[] { typeof(MainTestTable) });

			var reader = writer.CreateReader();
			provider.Restore(reader);
			var res = Context.Current.Get<IORM>().Select<MainTestTable>().Execute();
			Assert.AreEqual(5, res.Count);
			Assert.IsNotNull(res.ByID(r1.ID));
			Assert.IsNotNull(res.ByID(r2.ID));
			Assert.IsNotNull(res.ByID(r3.ID));
			Assert.IsNotNull(res.ByID(r4.ID));
			Assert.IsNotNull(res.ByID(r5.ID));
			Assert.AreEqual(1, res.ByID(r1.ID).Value);
			Assert.AreEqual(2, res.ByID(r2.ID).Value);
			Assert.AreEqual(3, res.ByID(r3.ID).Value);
			Assert.AreEqual(4, res.ByID(r4.ID).Value);
			Assert.AreEqual(5, res.ByID(r5.ID).Value);
		}

		[TestMethod]
		public void PrimaryKeyAndForeignKeyIsRestoredWhereSupported()
		{
			if (!Context.Current.Get<IDB>().CommandBuilder.SupportsPrimaryKeyInsert) Assert.Inconclusive("Database does not support specifying primary key value.");
			var provider = Context.Current.Get<IBackupProvider>();
			var writer = new MockBackupWriter();

			// force IDs to start from greater than 1
			new TableReferenced().Store();
			new TableReferenced().Store();
			new TableReferenced().Store();
			new TableReferencing().Store();

			var r1 = new TableReferenced { Value = MainTestTable.DEFAULTVALUE };
			r1.Store();
			var r2 = new TableReferencing { FKCascade = r1 };
			r2.Store();
			using (var set = provider.CreateBackupSet(writer))
			{
				set.AddRecords(new Record[] { r1, r2 });
			}

			provider.ClearTables(new[] { typeof(TableReferencing), typeof(TableReferenced) });

			var reader = writer.CreateReader();
			provider.Restore(reader);
			var res1 = Context.Current.Get<IORM>().Select<TableReferenced>().Execute().FirstOrDefault();
			var res2 = Context.Current.Get<IORM>().Select<TableReferencing>().Execute().FirstOrDefault();
			Assert.IsNotNull(res1);
			Assert.IsNotNull(res2);
			Assert.AreEqual(r1.ID, res1.ID);
			Assert.AreEqual(r2.ID, res2.ID);
			Assert.IsNotNull(r2.FKCascade);
			Assert.AreEqual(r2.FKCascade.ID, res1.ID);
		}

		[Table]
		class TableReferencing : Record
		{
			[Column]
			[ForeignKey.Cascade]
			public TableReferenced FKCascade { get; set; }

			[Column]
			[ForeignKey.None]
			public TableReferenced FKNone { get; set; }

			[Column]
			[ForeignKey.None]
			public RecordRef<TableReferenced> FKRef { get; set; }
		}

		[Table]
		class TableReferenced : Record
		{
			[Column]
			public int Value { get; set; }
		}

		[Table]
		class TableCyclic1 : Record
		{
			[Column]
			public int Value { get; set; }

			[Column]
			[ForeignKey.None]
			public RecordRef<TableCyclic1> Self { get; set; }

			[Column]
			[ForeignKey.None]
			public RecordRef<TableCyclic2> Other { get; set; }
		}

		[Table]
		class TableCyclic2 : Record
		{
			[Column]
			public int Value { get; set; }

			[Column]
			[ForeignKey.None]
			public RecordRef<TableCyclic1> Other { get; set; }
		}

		class MockBackupWriter : IBackupWriter
		{
			public List<Record> Records { get; private set; }

			public MockBackupWriter()
			{
				Records = new List<Record>();
			}

			public void WriteRecord(IRecord record)
			{
				Records.Add(((Record)record).Clone());
			}

			public IBackupReader CreateReader()
			{
				return new MockBackupReader(Records);
			}

			public void Dispose()
			{
			}
		}

		class MockBackupReader : IBackupReader
		{
			private Record[] records;
			private int position;

			public MockBackupReader(IEnumerable<Record> records)
			{
				this.records = records.ToArray();
			}

			public IRecord ReadRecord()
			{
				if (position < records.Length)
					return records[position++];
				else
					return null;
			}

			public void Dispose()
			{
			}
		}
	}
}
