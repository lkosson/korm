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
			Assert.AreNotEqual(r1, r2);
			Assert.IsFalse(r1 == r2);
			Assert.IsTrue(r1 != r2);
		}

		class Record1 : Record
		{
		}

		class Record2 : Record
		{
		}
	}
}
