using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public class RecordRefTests
	{
		private const int MARKER = 12345;

		[TestMethod]
		public void RecordRefHasID()
		{
			var rr = new RecordRef<Record1>(MARKER);
			var rrid = (IHasID)rr;
			Assert.AreEqual(MARKER, rrid.ID);
		}

		[TestMethod]
		public void RecordRefFromRecordHasID()
		{
			var rec = new Record1 { ID = MARKER };
			var rr = new RecordRef<Record>(rec);
			Assert.AreEqual(MARKER, rr.ID);
		}

		[TestMethod]
		public void RecordRefEqualsRecordRef()
		{
			var rr1 = new RecordRef<Record1>(MARKER);
			var rr2 = new RecordRef<Record1>(MARKER);
			Assert.AreEqual(rr1, rr2);
			Assert.IsTrue(rr1 == rr2);
			Assert.IsFalse(rr1 != rr2);
		}

		[TestMethod]
		public void RecordRefEqualsRecord()
		{
			var rec = new Record1 { ID = MARKER };
			var rr = new RecordRef<Record1>(rec);
			Assert.AreEqual(rec, rr);
			Assert.AreEqual(rr, rec);
			Assert.IsTrue(rr == rec);
			Assert.IsTrue(rec == rr);
			Assert.IsFalse(rec != rr);
			Assert.IsFalse(rr != rec);
		}

		[TestMethod]
		public void RecordRefEqualsID()
		{
			var rr = new RecordRef<Record1>(MARKER);
			Assert.AreEqual(rr, MARKER);
			Assert.AreEqual(MARKER, rr);
			Assert.IsTrue(rr == MARKER);
			Assert.IsTrue(MARKER == rr);
			Assert.IsFalse(MARKER != rr);
			Assert.IsFalse(rr != MARKER);
		}

		[TestMethod]
		public void RecordRefEqualsNull()
		{
			var rr = new RecordRef<Record1>();
			Assert.AreEqual(rr, null);
			Assert.IsTrue(rr == null);
			Assert.IsTrue(null == rr);
			Assert.IsFalse(null != rr);
			Assert.IsFalse(rr != null);
		}

		[TestMethod]
		public void RecordRefsOfDifferentTypesDiffer()
		{
			var rr1 = new RecordRef<Record1>(MARKER);
			var rr2 = new RecordRef<Record2>(MARKER);
			Assert.AreNotEqual(rr1, rr2);
			Assert.AreEqual(rr1.ID, rr2.ID);
			//shouldn't compile:
			//Assert.IsFalse(rr1 == rr2);
			//Assert.IsTrue(rr1 != rr2);
		}

		[TestMethod]
		public void RecordRefsOfDerivedTypeEquals()
		{
			var rrBase = new RecordRef<Record>(MARKER);
			var rrDerived = new RecordRef<Record1>(MARKER);
			Assert.AreEqual(rrBase, rrDerived);
			Assert.AreEqual(rrDerived, rrBase);
			//shouldn't compile:
			//Assert.IsTrue(rrBase == rrDerived);
			//Assert.IsFalse(rrBase != rrDerived);
		}

		[TestMethod]
		public void RecordExtensionCreatesRecordRef()
		{
			var rec = new Record1 { ID = MARKER };
			var rr = rec.Ref();
			Assert.IsNotNull(rr);
			Assert.AreEqual(rec.ID, rr.ID);
		}

		[TestMethod]
		[ExpectedException(typeof(OverflowException))]
		public void RecordRefConvertible16Overflows()
		{
			var rr = new RecordRef<Record1>(Int64.MaxValue);
			var irr = (IConvertible)rr;
			irr.ToInt16(null);
		}

		[TestMethod]
		[ExpectedException(typeof(OverflowException))]
		public void RecordRefConvertible32Overflows()
		{
			var rr = new RecordRef<Record1>(Int64.MaxValue);
			var irr = (IConvertible)rr;
			irr.ToInt32(null);
		}

		class Record1 : Record
		{
		}

		class Record2 : Record
		{
		}
	}
}
