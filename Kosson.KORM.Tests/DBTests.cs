using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract class DBTests : TestBase
	{
		// Checking for BeginTransaction/Commit fails if transaction is opened in TestBase.Init
		protected override bool NeedsDatabase { get { return false; } }

		[TestMethod]
		public void DBComponentExists()
		{
			Assert.IsNotNull(DB);
		}
/* TODO
		[TestMethod]
		public void DBCanBeCreatedExplicitly()
		{
			using (Context.Begin())
			{
				var dbfactory = Context.Current.Get<IDBFactory>();
				var db = dbfactory.Create(Provider);
				Assert.IsNotNull(db);
				Assert.AreEqual(Context.Current, db.OwningContext);
			}
		}

		[TestMethod]
		public void ExplicitlyCreatedDBIsIndependent()
		{
			using (Context.Begin())
			{
				var dbfactory = Context.Current.Get<IDBFactory>();
				var db1 = Context.Current.Get<IDB>();
				var db2 = dbfactory.Create(Provider);
				var db3 = dbfactory.Create(Provider);
				Assert.AreNotEqual(db1, db2);
				Assert.AreNotEqual(db2, db3);
			}
		}
		*/
		[TestMethod]
		public void ConnectionStringIsNotNull()
		{
			Assert.IsNotNull(DB.ConnectionString);
		}

		[TestMethod]
		public void CommandBuilderIsNotNull()
		{
			Assert.IsNotNull(DB.CommandBuilder);
		}

		[TestMethod]
		public void CreateDatabaseCompletes()
		{
			DB.CreateDatabase();
		}

		[TestMethod]
		public void BeginTransactionCompletes()
		{
			Assert.IsFalse(DB.IsTransactionOpen);
			DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
		}

		[TestMethod]
		public void BeginTransactionWithExplicitIsolationLevelCompletes()
		{
			DB.BeginTransaction(System.Data.IsolationLevel.Serializable);
			Assert.AreEqual(System.Data.IsolationLevel.Serializable, DB.IsolationLevel);
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void MultipleBeginTransactionFails()
		{
			Assert.IsFalse(DB.IsTransactionOpen);
			DB.BeginTransaction();
			DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
		}

		[TestMethod]
		public void CommitCompletes()
		{
			DB.BeginTransaction();
			DB.Commit();
		}

		[TestMethod]
		public void RollbackCompletes()
		{
			DB.BeginTransaction();
			DB.Rollback();
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void CommitWithoutTransactionFails()
		{
			DB.Commit();
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void RollbackWithoutTransactionFails()
		{
			DB.Rollback();
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void MultipleCommitsFails()
		{
			Assert.IsFalse(DB.IsTransactionOpen);
			DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
			DB.Commit();
			Assert.IsFalse(DB.IsTransactionOpen);
			DB.Commit();
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void MultipleRollbacksFails()
		{
			Assert.IsFalse(DB.IsTransactionOpen);
			DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
			DB.Rollback();
			Assert.IsFalse(DB.IsTransactionOpen);
			DB.Rollback();
		}

		[TestMethod]
		public void CommitClosesTransaction()
		{
			Assert.IsFalse(DB.IsTransactionOpen);
			DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
			DB.Commit();
			Assert.IsFalse(DB.IsTransactionOpen);
		}

		[TestMethod]
		public void RollbackClosesTransaction()
		{
			Assert.IsFalse(DB.IsTransactionOpen);
			DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
			DB.Rollback();
			Assert.IsFalse(DB.IsTransactionOpen);
		}

		[TestMethod]
		public void TransactionIsImplicitlyOpened()
		{
			Assert.IsFalse(DB.IsTransactionOpen);
			DB.CreateCommand("SELECT 1");
			Assert.IsTrue(DB.IsTransactionOpen);
		}

		[TestMethod]
		public void ImplicitlyOpenedTransactionIsMarked()
		{
			Assert.IsFalse(DB.IsImplicitTransaction);
			DB.CreateCommand("SELECT 1");
			Assert.IsTrue(DB.IsImplicitTransaction);
		}

		[TestMethod]
		public void ExplicitTransactionIsNotMarked()
		{
			DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
			Assert.IsFalse(DB.IsImplicitTransaction);
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void ImplicitTransactionCannotBeCommitted()
		{
			DB.CreateCommand("SELECT 1");
			Assert.IsTrue(DB.IsTransactionOpen);
			DB.Commit();
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void ImplicitTransactionCannotBeRolledBack()
		{
			DB.CreateCommand("SELECT 1");
			Assert.IsTrue(DB.IsTransactionOpen);
			DB.Rollback();
		}
	}
}
