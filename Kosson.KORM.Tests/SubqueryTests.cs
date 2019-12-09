using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;

namespace Kosson.KRUD.Tests
{
	public abstract class SubqueryTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
			yield return typeof(Table);
		}

		[TestMethod]
		public void SubqueryIsSelected()
		{
            new MainTestTable() { Value = INTMARKER - 1 }.Insert();
			new MainTestTable() { Value = INTMARKER }.Insert();
			new MainTestTable() { Value = INTMARKER }.Insert();
			new MainTestTable() { Value = INTMARKER + 1 }.Insert();

			var record = new Table();
			record.Value = INTMARKER;
            record.DefaultValue = new MainTestTable().DefaultValue;
			record.Insert();
			Assert.AreEqual(0, record.Count);
			Assert.AreEqual(0, record.CountConstructed);

			var retrieved = orm.Select<Table>().ByID(record.ID);
			Assert.AreEqual(2, retrieved.Count);
			Assert.AreEqual(2, retrieved.CountConstructed);
            Assert.AreEqual(1, retrieved.CountDistinctConstructed);
			Assert.AreEqual(MainTestTable.DEFAULTVALUE * 2, retrieved.SumConstructed);
            Assert.AreEqual(INTMARKER, retrieved.AvgConstructed);
            Assert.AreEqual(INTMARKER - 1, retrieved.MinConstructed);
            Assert.AreEqual(INTMARKER + 1, retrieved.MaxConstructed);
			Assert.AreEqual(INTMARKER - 1, retrieved.FirstConstructed);
			Assert.AreEqual(INTMARKER + 1, retrieved.LastConstructed);
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

			[Column]
			public int Value { get; set; }

            [Column]
            public int DefaultValue { get; set; }
		}
	}
}
