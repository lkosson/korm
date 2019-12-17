using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Threading.Tasks;
using System.Linq;

namespace Kosson.KORM.Tests
{
	public abstract class NotificationTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(Table);
		}

		#region Insert notifications
		[TestMethod]
		public void InsertNotificationsCalled()
		{
			var record = new Table();
			Assert.IsFalse(record.OnInsertCalled);
			Assert.IsFalse(record.OnInsertedCalled);
			ORM.Insert(record);
			Assert.IsTrue(record.OnInsertCalled);
			Assert.IsTrue(record.OnInsertedCalled);
		}

		[TestMethod]
		public async Task InsertNotificationsCalledAsync()
		{
			var record = new Table();
			Assert.IsFalse(record.OnInsertCalled);
			Assert.IsFalse(record.OnInsertedCalled);
			await ORM.InsertAsync(record);
			Assert.IsTrue(record.OnInsertCalled);
			Assert.IsTrue(record.OnInsertedCalled);
		}

		[TestMethod]
		[ExpectedException(typeof(KORMInsertFailedException))]
		public void InsertBeforeNotificationsResultThrowsException()
		{
			var record = new Table();
			record.NotifyBeforeResult = RecordNotifyResult.Break;
			ORM.Insert(record);
		}

		[TestMethod]
		public void InsertBeforeNotificationsResultRespected()
		{
			var record = new Table();
			record.NotifyBeforeResult = RecordNotifyResult.Break;
			Assert.IsFalse(record.OnInsertCalled);
			Assert.IsFalse(record.OnInsertedCalled);
			int count = ORM.Insert<Table>().Records(new[] { record });
			Assert.IsTrue(record.OnInsertCalled);
			Assert.IsFalse(record.OnInsertedCalled);
			Assert.AreEqual(0, count);
			Assert.AreEqual(0, record.ID);
		}

		[TestMethod]
		public void InsertAfterNotificationsResultRespected()
		{
			var record = new Table();
			record.NotifyAfterResult = RecordNotifyResult.Break;
			Assert.IsFalse(record.OnInsertCalled);
			Assert.IsFalse(record.OnInsertedCalled);
			int count = ORM.Insert<Table>().Records(new[] { record, record });
			Assert.IsTrue(record.OnInsertCalled);
			Assert.IsTrue(record.OnInsertedCalled);
			Assert.AreEqual(1, count);
			Assert.AreNotEqual(0, record.ID);
		}
		#endregion
		#region Update notifications
		[TestMethod]
		public void UpdateNotificationsCalled()
		{
			var record = new Table();
			ORM.Insert(record);
			Assert.IsFalse(record.OnUpdateCalled);
			Assert.IsFalse(record.OnUpdatedCalled);
			ORM.Update(record);
			Assert.IsTrue(record.OnUpdateCalled);
			Assert.IsTrue(record.OnUpdatedCalled);
		}

		public async Task UpdateNotificationsCalledAsync()
		{
			var record = new Table();
			ORM.Insert(record);
			Assert.IsFalse(record.OnUpdateCalled);
			Assert.IsFalse(record.OnUpdatedCalled);
			await ORM.UpdateAsync(record);
			Assert.IsTrue(record.OnUpdateCalled);
			Assert.IsTrue(record.OnUpdatedCalled);
		}

		[TestMethod]
		[ExpectedException(typeof(KORMUpdateFailedException))]
		public void UpdateBeforeNotificationsResultThrowsException()
		{
			var record = new Table();
			ORM.Insert(record);
			record.NotifyBeforeResult = RecordNotifyResult.Break;
			ORM.Update(record);
		}

		[TestMethod]
		public void UpdateBeforeNotificationsResultRespected()
		{
			var record = new Table();
			ORM.Insert(record);
			record.NotifyBeforeResult = RecordNotifyResult.Break;
			Assert.IsFalse(record.OnUpdateCalled);
			Assert.IsFalse(record.OnUpdatedCalled);
			record.Value = INTMARKER;
			var count = ORM.Update<Table>().Records(new[] { record });
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsTrue(record.OnUpdateCalled);
			Assert.IsFalse(record.OnUpdatedCalled);
			Assert.AreEqual(0, count);
			Assert.AreEqual(INTMARKER, record.Value);
			Assert.AreNotEqual(INTMARKER, retrieved.Value);
		}

		[TestMethod]
		public void UpdateAfterNotificationsResultRespected()
		{
			var record = new Table();
			ORM.Insert(record);
			record.NotifyAfterResult = RecordNotifyResult.Break;
			Assert.IsFalse(record.OnUpdateCalled);
			Assert.IsFalse(record.OnUpdatedCalled);
			int count = ORM.Update<Table>().Records(new[] { record, record });
			Assert.IsTrue(record.OnUpdateCalled);
			Assert.IsTrue(record.OnUpdatedCalled);
			Assert.AreEqual(1, count);
		}
		#endregion
		#region Delete notifications
		[TestMethod]
		public void DeleteNotificationsCalled()
		{
			var record = new Table();
			ORM.Insert(record);
			Assert.IsFalse(record.OnDeleteCalled);
			Assert.IsFalse(record.OnDeletedCalled);
			ORM.Delete(record);
			Assert.IsTrue(record.OnDeleteCalled);
			Assert.IsTrue(record.OnDeletedCalled);
		}

		public async Task DeleteNotificationsCalledAsync()
		{
			var record = new Table();
			ORM.Insert(record);
			Assert.IsFalse(record.OnDeleteCalled);
			Assert.IsFalse(record.OnDeletedCalled);
			await ORM.DeleteAsync(record);
			Assert.IsTrue(record.OnDeleteCalled);
			Assert.IsTrue(record.OnDeletedCalled);
		}

		[TestMethod]
		[ExpectedException(typeof(KORMDeleteFailedException))]
		public void DeleteBeforeNotificationsResultThrowsException()
		{
			var record = new Table();
			ORM.Insert(record);
			record.NotifyBeforeResult = RecordNotifyResult.Break;
			ORM.Delete(record);
		}

		[TestMethod]
		public void DeleteBeforeNotificationsResultRespected()
		{
			var record = new Table();
			ORM.Insert(record);
			record.NotifyBeforeResult = RecordNotifyResult.Break;
			Assert.IsFalse(record.OnDeleteCalled);
			Assert.IsFalse(record.OnDeletedCalled);
			var count = ORM.Delete<Table>().Records(new[] { record });
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsTrue(record.OnDeleteCalled);
			Assert.IsFalse(record.OnDeletedCalled);
			Assert.AreEqual(0, count);
			Assert.IsNotNull(retrieved);
		}

		[TestMethod]
		public void DeleteAfterNotificationsResultRespected()
		{
			var record = new Table();
			ORM.Insert(record);
			record.NotifyAfterResult = RecordNotifyResult.Break;
			Assert.IsFalse(record.OnDeleteCalled);
			Assert.IsFalse(record.OnDeletedCalled);
			var count = ORM.Delete<Table>().Records(new[] { record });
			var retrieved = ORM.Select<Table>().ByID(record.ID);
			Assert.IsTrue(record.OnDeleteCalled);
			Assert.IsTrue(record.OnDeletedCalled);
			Assert.AreEqual(1, count);
			Assert.IsNull(retrieved);
		}
		#endregion
		#region Select notifications
		[TestMethod]
		public void SelectExecuteNotificationCalled()
		{
			var record = new Table();
			ORM.Insert(record);
			Assert.IsFalse(record.OnSelectCalled);
			Assert.IsFalse(record.OnSelectedCalled);
			record = ORM.Select<Table>().WhereID(record.ID).Execute().First();
			Assert.IsTrue(record.OnSelectCalled);
			Assert.IsTrue(record.OnSelectedCalled);
		}

		[TestMethod]
		public async Task SelectExecuteNotificationCalledAsync()
		{
			var record = new Table();
			ORM.Insert(record);
			Assert.IsFalse(record.OnSelectCalled);
			Assert.IsFalse(record.OnSelectedCalled);
			record = (await ORM.Select<Table>().WhereID(record.ID).ExecuteAsync()).First();
			Assert.IsTrue(record.OnSelectCalled);
			Assert.IsTrue(record.OnSelectedCalled);
		}

		[TestMethod]
		public void SelectExecuteReaderNotificationCalled()
		{
			var record = new Table();
			ORM.Insert(record);
			Assert.IsFalse(record.OnSelectCalled);
			Assert.IsFalse(record.OnSelectedCalled);
			var reader = ORM.Select<Table>().WhereID(record.ID).ExecuteReader();
			reader.MoveNext();
			record = reader.Read();
			Assert.IsTrue(record.OnSelectCalled);
			Assert.IsTrue(record.OnSelectedCalled);
		}

		[TestMethod]
		public async Task SelectExecuteReaderNotificationCalledAsync()
		{
			var record = new Table();
			ORM.Insert(record);
			Assert.IsFalse(record.OnSelectCalled);
			Assert.IsFalse(record.OnSelectedCalled);
			var reader = await ORM.Select<Table>().WhereID(record.ID).ExecuteReaderAsync();
			await reader.MoveNextAsync();
			record = reader.Read();
			Assert.IsTrue(record.OnSelectCalled);
			Assert.IsTrue(record.OnSelectedCalled);
		}
		#endregion

		[Table]
		class Table : Record, IRecordNotifyDelete, IRecordNotifyInsert, IRecordNotifySelect, IRecordNotifyUpdate
		{
			[Column]
			public int Value { get; set; }

			public bool OnUpdateCalled { get; set; }
			public bool OnUpdatedCalled { get; set; }
			public bool OnInsertCalled { get; set; }
			public bool OnInsertedCalled { get; set; }
			public bool OnDeleteCalled { get; set; }
			public bool OnDeletedCalled { get; set; }
			public bool OnSelectCalled { get; set; }
			public bool OnSelectedCalled { get; set; }
			public RecordNotifyResult NotifyBeforeResult { get; set; }
			public RecordNotifyResult NotifyAfterResult { get; set; }

			public Table()
			{
				NotifyBeforeResult = RecordNotifyResult.Continue;
				NotifyAfterResult = RecordNotifyResult.Continue;
			}

			public RecordNotifyResult OnUpdate()
			{
				OnUpdateCalled = true;
				return NotifyBeforeResult;
			}

			public RecordNotifyResult OnUpdated()
			{
				OnUpdatedCalled = true;
				return NotifyAfterResult;
			}

			public RecordNotifyResult OnDelete()
			{
				OnDeleteCalled = true;
				return NotifyBeforeResult;
			}

			public RecordNotifyResult OnDeleted()
			{
				OnDeletedCalled = true;
				return NotifyAfterResult;
			}

			public RecordNotifyResult OnInsert()
			{
				OnInsertCalled = true;
				return NotifyBeforeResult;
			}

			public RecordNotifyResult OnInserted()
			{
				OnInsertedCalled = true;
				return NotifyAfterResult;
			}

			public RecordNotifyResult OnSelect(IRow row)
			{
				OnSelectCalled = true;
				return NotifyBeforeResult;
			}

			public RecordNotifyResult OnSelected(IRow row)
			{
				OnSelectedCalled = true;
				return NotifyAfterResult;
			}
		}
	}
}
