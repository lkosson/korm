using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.Kontext;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract partial class IndexTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield break;
		}

		[TestMethod]
		public void IndexIsCreated()
		{
			orm.CreateTables(new[] { typeof(TableWithIndex) });
		}

		[TestMethod]
		public void CoveringIndexIsCreated()
		{
			orm.CreateTables(new[] { typeof(TableWithCoveringIndex) });
		}

		[Table]
		[Index("IDX_TestIndex_1", "Value1")]
		[Index("IDX_TestIndex_2", "Value1", "Value2")]
		class TableWithIndex : Record
		{
			[Column]
			public int Value1 { get; set; }

			[Column]
			public int Value2 { get; set; }
		}

		[Table]
		[Index("IDX_TestIndex_3", "Value1", IncludedFields=new[] {"Value2"})]
		class TableWithCoveringIndex : Record
		{
			[Column]
			public int Value1 { get; set; }

			[Column]
			public int Value2 { get; set; }
		}
	}
}
