using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using System.Threading.Tasks;

namespace Kosson.KRUD.Tests
{
	public abstract class ComboTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Table1);
			yield return typeof(Table2);
		}

		[TestMethod]
		public void ComboTest1()
		{
			var record1 = new Table1();
			var record2 = new Table2();
			record2.Inline = new Inline();
			record2.Insert();
			record1.FK = record2;
			record1.Insert();

			var retrieved = orm.Select<Table1>().ByID(record1.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.FK);
			Assert.AreEqual(record2.ID, retrieved.FK.ID);
			Assert.IsNotNull(retrieved.FK.Inline);
			Assert.AreEqual(INTMARKER, retrieved.FK.Inline.Value);

			retrieved.FK.Inline.Value++;
			retrieved.FK.Inline.FK = record1;
			retrieved.FK.Update();

			var retrievedfk = orm.Select<Table2>().ByID(retrieved.FK.ID);
			Assert.IsNotNull(retrievedfk);
			Assert.IsNotNull(retrievedfk.Inline);
			Assert.AreEqual(INTMARKER + 1, retrievedfk.Inline.Value);
			Assert.AreEqual(record1, retrievedfk.Inline.FK);
		}

		[TestMethod]
		public async Task AsyncComboTest()
		{
			var record1 = new Table1();
			var record2 = new Table2();
			record2.Inline = new Inline();
			await record2.InsertAsync();
			record1.FK = record2;
			await record1.InsertAsync();

			var retrieved = await orm.Select<Table1>().ByIDAsync(record1.ID);
			Assert.IsNotNull(retrieved);
			Assert.IsNotNull(retrieved.FK);
			Assert.AreEqual(record2.ID, retrieved.FK.ID);
			Assert.IsNotNull(retrieved.FK.Inline);
			Assert.AreEqual(INTMARKER, retrieved.FK.Inline.Value);

			retrieved.FK.Inline.Value++;
			retrieved.FK.Inline.FK = record1;
			await retrieved.FK.UpdateAsync();

			var retrievedfk = await orm.Select<Table2>().ByIDAsync(retrieved.FK.ID);
			Assert.IsNotNull(retrievedfk);
			Assert.IsNotNull(retrievedfk.Inline);
			Assert.AreEqual(INTMARKER + 1, retrievedfk.Inline.Value);
			Assert.AreEqual(record1, retrievedfk.Inline.FK);
		}

		[Table]
		class Table1 : Record
		{
			[Column]
			[ForeignKey.Cascade]
			public Table2 FK { get; set; }

			[Subquery.Count("ComboTestsTable2", "t2_Inline_FK", "ctt_FK")]
			public int Count { get; set; }
		}

		[Table("t2")]		
		class Table2 : Record
		{
			[Inline]
			public Inline Inline { get; set; }
		}

		class Inline
		{
			[Column(System.Data.DbType.String, Length=10, IsNotNull=true)]
			[DBName("IV")]
			public int Value { get; set; }

			[Column]
			[ForeignKey.None]
			public RecordRef<Table1> FK { get; set; }

			public Inline()
			{
				Value = INTMARKER;
			}
		}
	}
}
