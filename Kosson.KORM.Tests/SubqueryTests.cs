using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;

namespace Kosson.KORM.Tests
{
	public abstract class SubqueryTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
			yield return typeof(Table);
			yield return typeof(ForeignSubqueryTable);
		}

		[TestMethod]
		public void SubqueryIsSelected()
		{
            ORM.Insert(new MainTestTable() { Value = INTMARKER - 1 });
			ORM.Insert(new MainTestTable() { Value = INTMARKER });
			ORM.Insert(new MainTestTable() { Value = INTMARKER });
			ORM.Insert(new MainTestTable() { Value = INTMARKER + 1 });

			var record = new Table();
			record.Value = INTMARKER;
            record.DefaultValue = new MainTestTable().DefaultValue;
			ORM.Insert(record);
			Assert.AreEqual(0, record.Count);
			Assert.AreEqual(0, record.CountConstructed);

			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.AreEqual(2, retrieved.Count);
			Assert.AreEqual(2, retrieved.CountConstructed);
            Assert.AreEqual(1, retrieved.CountDistinctConstructed);
			Assert.AreEqual(MainTestTable.DEFAULTVALUE * 2, retrieved.SumConstructed);
            Assert.AreEqual(INTMARKER, retrieved.AvgConstructed);
            Assert.AreEqual(INTMARKER - 1, retrieved.MinConstructed);
            Assert.AreEqual(INTMARKER + 1, retrieved.MaxConstructed);
			Assert.AreEqual(INTMARKER - 1, retrieved.FirstConstructed);
			Assert.AreEqual(INTMARKER + 1, retrieved.LastConstructed);

			Assert.AreEqual(retrieved.CountConstructed, retrieved.CountConstructedTyped);
			Assert.AreEqual(retrieved.CountDistinctConstructed, retrieved.CountDistinctConstructedTyped);
			Assert.AreEqual(retrieved.SumConstructed, retrieved.SumConstructedTyped);
			Assert.AreEqual(retrieved.AvgConstructed, retrieved.AvgConstructedTyped);
			Assert.AreEqual(retrieved.MinConstructed, retrieved.MinConstructedTyped);
			Assert.AreEqual(retrieved.MaxConstructed, retrieved.MaxConstructedTyped);
			Assert.AreEqual(retrieved.FirstConstructed, retrieved.FirstConstructedTyped);
			Assert.AreEqual(retrieved.LastConstructed, retrieved.LastConstructedTyped);
		}

		[TestMethod]
		public void ForeignSubqueryIsSelected()
		{
			ORM.Insert(new MainTestTable() { Value = INTMARKER - 1 });
			ORM.Insert(new MainTestTable() { Value = INTMARKER });
			ORM.Insert(new MainTestTable() { Value = INTMARKER });
			ORM.Insert(new MainTestTable() { Value = INTMARKER + 1 });

			var foreign = new Table();
			foreign.Value = INTMARKER;
			foreign.DefaultValue = new MainTestTable().DefaultValue;
			ORM.Insert(foreign);

			var record = new ForeignSubqueryTable();
			record.Subtable = foreign;
			ORM.Insert(record);

			var retrieved = ORM.Select<ForeignSubqueryTable>().ByID(record.ID);
			var retrievedforeign = retrieved.Subtable;

			Assert.AreEqual(2, retrievedforeign.Count);
			Assert.AreEqual(2, retrievedforeign.CountConstructed);
			Assert.AreEqual(1, retrievedforeign.CountDistinctConstructed);
			Assert.AreEqual(MainTestTable.DEFAULTVALUE * 2, retrievedforeign.SumConstructed);
			Assert.AreEqual(INTMARKER, retrievedforeign.AvgConstructed);
			Assert.AreEqual(INTMARKER - 1, retrievedforeign.MinConstructed);
			Assert.AreEqual(INTMARKER + 1, retrievedforeign.MaxConstructed);
			Assert.AreEqual(INTMARKER - 1, retrievedforeign.FirstConstructed);
			Assert.AreEqual(INTMARKER + 1, retrievedforeign.LastConstructed);
		}

		[Table]
		class Table : Record
		{
			[Subquery("SELECT COUNT(*) FROM \"MainTestTable\" WHERE \"mtt_Value\" = {0}.\"stt_Value\"")]
			public int Count { get; set; }

			[Subquery.Count("MainTestTable", "mtt_Value", "stt_Value")]
			public int CountConstructed { get; set; }

            [Subquery.CountDistinct("MainTestTable", "mtt_DefaultValue", "mtt_Value", "stt_Value")]
            public int CountDistinctConstructed { get; set; }

            [Subquery.Sum("MainTestTable", "mtt_DefaultValue", "mtt_Value", "stt_Value")]
			public int SumConstructed { get; set; }

            [Subquery.Avg("MainTestTable", "mtt_Value", "mtt_DefaultValue", "stt_DefaultValue")]
            public int AvgConstructed { get; set; }

            [Subquery.Min("MainTestTable", "mtt_Value", "mtt_DefaultValue", "stt_DefaultValue")]
            public int MinConstructed { get; set; }

            [Subquery.Max("MainTestTable", "mtt_Value", "mtt_DefaultValue", "stt_DefaultValue")]
            public int MaxConstructed { get; set; }

			[Subquery.First("MainTestTable", "mtt_Value", "mtt_DefaultValue", "stt_DefaultValue", "mtt_ID")]
			public int FirstConstructed { get; set; }

			[Subquery.First("MainTestTable", "mtt_Value", "mtt_DefaultValue", "stt_DefaultValue", "mtt_ID DESC")]
			public int LastConstructed { get; set; }

			[Subquery.Count(typeof(MainTestTable), nameof(MainTestTable.Value), nameof(Value))]
			public int CountConstructedTyped { get; set; }

			[Subquery.CountDistinct(typeof(MainTestTable), nameof(MainTestTable.DefaultValue), nameof(MainTestTable.Value), nameof(Value))]
			public int CountDistinctConstructedTyped { get; set; }

			[Subquery.Sum(typeof(MainTestTable), nameof(MainTestTable.DefaultValue), nameof(MainTestTable.Value), nameof(Value))]
			public int SumConstructedTyped { get; set; }

			[Subquery.Avg(typeof(MainTestTable), nameof(MainTestTable.Value), nameof(MainTestTable.DefaultValue), nameof(DefaultValue))]
			public int AvgConstructedTyped { get; set; }

			[Subquery.Min(typeof(MainTestTable), nameof(MainTestTable.Value), nameof(MainTestTable.DefaultValue), nameof(DefaultValue))]
			public int MinConstructedTyped { get; set; }

			[Subquery.Max(typeof(MainTestTable), nameof(MainTestTable.Value), nameof(MainTestTable.DefaultValue), nameof(DefaultValue))]
			public int MaxConstructedTyped { get; set; }

			[Subquery.First(typeof(MainTestTable), nameof(MainTestTable.Value), nameof(MainTestTable.DefaultValue), nameof(DefaultValue), "mtt_ID")]
			public int FirstConstructedTyped { get; set; }

			[Subquery.First(typeof(MainTestTable), nameof(MainTestTable.Value), nameof(MainTestTable.DefaultValue), nameof(DefaultValue), "mtt_ID DESC")]
			public int LastConstructedTyped { get; set; }

			[Column]
			public int Value { get; set; }

            [Column]
            public int DefaultValue { get; set; }
		}

		class ForeignSubqueryTable : Record
		{
			[Column]
			public int Ignored1 { get; set; }

			[Column]
			public Table Subtable { get; set; }

			[Column]
			public MainTestTable Ignored2 { get; set; }

			[Column]
			public int Ignored3 { get; set; }
		}
	}
}
