using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using System.Threading.Tasks;

namespace Kosson.KRUD.Tests
{
	public abstract class DataReaderTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
		}

		[TestMethod]
		public void EmptyReaderIsEmpty()
		{
			using (var reader = orm.Select<MainTestTable>().ExecuteReader())
			{
				Assert.IsNotNull(reader);
				Assert.IsFalse(reader.MoveNext());
			}
		}

		[TestMethod]
		public void ReaderAdvances()
		{
			var record = new MainTestTable();
			record.Value = INTMARKER;
			record.Insert();
			using (var reader = orm.Select<MainTestTable>().ExecuteReader())
			{
				Assert.IsTrue(reader.MoveNext());
				Assert.IsNotNull(reader.Read());
				Assert.IsFalse(reader.MoveNext());
			}
		}

		[TestMethod]
		public async Task AsyncReaderAdvances()
		{
			var record = new MainTestTable();
			record.Value = INTMARKER;
			record.Insert();
			using (var reader = await orm.Select<MainTestTable>().ExecuteReaderAsync())
			{
				Assert.IsTrue(await reader.MoveNextAsync());
				Assert.IsNotNull(reader.Read());
				Assert.IsFalse(await reader.MoveNextAsync());
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void DisposedReaderThrows()
		{
			var reader = orm.Select<MainTestTable>().ExecuteReader();
			reader.Dispose();
			reader.MoveNext();
		}

		[TestMethod]
		public void MultipleReaderDisposes()
		{
			var reader = orm.Select<MainTestTable>().ExecuteReader();
			reader.Dispose();
			reader.Dispose();
		}

		[TestMethod]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void FinishedEnumerationDisposes()
		{
			var record = new MainTestTable();
			record.Value = INTMARKER;
			record.Insert();
			var reader = orm.Select<MainTestTable>().ExecuteReader();
			while (reader.MoveNext()) reader.Read();
			reader.Read();
		}
	}
}
