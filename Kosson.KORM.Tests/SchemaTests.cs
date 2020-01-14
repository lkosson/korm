using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract partial class SchemaTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(TableA);
			yield return typeof(TableA1);
			yield return typeof(TableA2);
			yield return typeof(Table2);
		}

		[TestMethod]
		public void SchemasAreCreated()
		{
			ORM.Select<TableA1>().Execute();
			ORM.Select<TableA2>().Execute();
		}

		[TestMethod]
		public void SchemasAreIndependent()
		{
			var ta1 = new TableA1 { Value = INTMARKER };
			var ta2 = new TableA2 { Value = INTMARKER + 1};
			ORM.Insert(ta1);
			ORM.Insert(ta2);
			var ra1 = ORM.Select<TableA1>().ByID(ta1.ID);
			var ra2 = ORM.Select<TableA2>().ByID(ta2.ID);
			Assert.AreEqual(ta1.Value, ra1.Value);
			Assert.AreEqual(ta2.Value, ra2.Value);
			Assert.AreNotEqual(ra1.Value, ra2.Value);
		}

		[Table]
		class TableA : Record
		{
			[Column]
			public int Value { get; set; }
		}

		[Table]
		[DBSchema("schema1")]
		class TableA1 : Record
		{
			[Column]
			public int Value { get; set; }

			[Column]
			[ForeignKey.None]
			public Table2 FK { get; set; }
		}

		[Table]
		[DBSchema("schema2")]
		class TableA2 : Record
		{
			[Column]
			public int Value { get; set; }

			[Column]
			[ForeignKey.None]
			public Table2 FK { get; set; }
		}

		[Table("schema1")]
		class Table2 : Record
		{
			[Column]
			public int Value { get; set; }
		}
	}
}
