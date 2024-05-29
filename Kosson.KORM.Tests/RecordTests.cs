using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public class RecordTests
	{
		[TestMethod]
		public void RecordsWithSameIDAreEqual()
		{
			var r1 = new Record1() { ID = 1 };
			var r2 = new Record1() { ID = 1 };
			Assert.AreEqual(r1, r2);
			Assert.IsTrue(r1 == r2);
			Assert.IsFalse(r1 != r2);
		}

		[TestMethod]
		public void RecordsWithDifferentIDAreDifferent()
		{
			var r1 = new Record1() { ID = 1 };
			var r2 = new Record1() { ID = 2 };
			Assert.AreNotEqual(r1, r2);
			Assert.IsFalse(r1 == r2);
			Assert.IsTrue(r1 != r2);
		}

		[TestMethod]
		public void RecordIsNotNull()
		{
			var r1 = new Record1();
			Assert.AreNotEqual(r1, null);
			Assert.IsFalse(r1 == null);
			Assert.IsTrue(r1 != null);
		}

		[TestMethod]
		public void NullRecordIsNull()
		{
			Record1 r1 = null;
			Assert.IsTrue(r1 == null);
			Assert.IsFalse(r1 != null);
		}

		[TestMethod]
		public void RecordsOfDifferentTypesAreNotEqual()
		{
			var r1 = new Record1() { ID = 1 };
			var r2 = new Record2() { ID = 1 };
			Assert.AreNotEqual<Record>(r1, r2);
			Assert.IsFalse(r1 == r2);
			Assert.IsTrue(r1 != r2);
		}

		[TestMethod]
		public void DerivedRecordsAreEqual()
		{
			var r1 = new Record { ID = 1 };
			var r2 = new Record1 { ID = 1 };
			Assert.AreEqual(r1, r2);
			Assert.IsTrue(r1 == r2);
			Assert.IsTrue(r2 == r1);
		}

		[TestMethod]
		public void DerivedRecordsAreNotEqual()
		{
			var r1 = new Record { ID = 1 };
			var r2 = new Record1 { ID = 2 };
			Assert.AreNotEqual(r1, r2);
			Assert.IsFalse(r1 == r2);
			Assert.IsFalse(r2 == r1);
		}

		[TestMethod]
		public void RecordAndRecordRefsAreEqual()
		{
			var r1 = new Record { ID = 1 };
			var r2 = new Record { ID = 2 };
			var rref = new RecordRef<Record> { ID = 1 };
			Assert.AreEqual(r1, rref);
			Assert.IsTrue(r1 == rref);
			Assert.IsTrue(rref == r1);

			Assert.AreNotEqual(r2, rref);
			Assert.IsTrue(r2 != rref);
			Assert.IsTrue(rref != r2);
		}

		[TestMethod]
		public void DerivedRecordAndRecordRefsAreEqual()
		{
			var r = new Record1 { ID = 1 };
			var rref = new RecordRef<Record> { ID = 1 };
			Assert.AreEqual(r, rref);
			Assert.IsTrue(r == rref);
			Assert.IsTrue(rref == r);
		}

		class Record1 : Record
		{
		}

		class Record2 : Record
		{
		}
	}
}
