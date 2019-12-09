using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;

namespace Kosson.KRUD.Tests
{
	public abstract class ManualIDTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Table);
		}

		[TestMethod]
		public void ManualIDIsStored()
		{
			var record = new Table();
			record.ID = INTMARKER;
			record.Insert();
			Assert.AreEqual(INTMARKER, record.ID);
			var retrieved = orm.Select<Table>().ByID(INTMARKER);
			Assert.IsNotNull(retrieved);
		}

		[Table(IsManualID = true)]
		class Table : IRecord
		{
			[Column]
			public long ID { get; set; }
		}
	}
}
