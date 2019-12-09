using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Kontext;
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
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsNotNull(db);
			}
		}

		[TestMethod]
		public void DBComponentIsSingleton()
		{
			using (Context.Begin())
			{
				var db1 = Context.Current.Get<IDB>();
				var db2 = Context.Current.Get<IDB>();
				Assert.AreEqual(db1, db2);
			}
		}

		[TestMethod]
		public void DBFactoryComponentExists()
		{
			using (Context.Begin())
			{
				var dbfactory = Context.Current.Get<IDBFactory>();
				Assert.IsNotNull(dbfactory);
			}
		}

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

		[TestMethod]
		public void ConnectionStringIsNotNull()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsNotNull(db.ConnectionString);
			}
		}

		[TestMethod]
		public void CommandBuilderIsNotNull()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsNotNull(db.CommandBuilder);
			}
		}

		[TestMethod]
		public void CreateDatabaseCompletes()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.CreateDatabase();
			}
		}

		[TestMethod]
		public void BeginTransactionCompletes()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.BeginTransaction();
				Assert.IsTrue(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void BeginTransactionWithExplicitIsolationLevelCompletes()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.BeginTransaction(System.Data.IsolationLevel.Serializable);
				Assert.AreEqual(System.Data.IsolationLevel.Serializable, db.IsolationLevel);
			}
		}

		[TestMethod]
		public void MultipleBeginTransactionComplete()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.BeginTransaction();
				db.BeginTransaction();
				db.BeginTransaction();
				Assert.IsTrue(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void CommitCompletes()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.Commit();
			}
		}

		[TestMethod]
		public void RollbackCompletes()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.Rollback();
			}
		}

		[TestMethod]
		public void MultipleCommitsComplete()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.Commit();
				Assert.IsFalse(db.IsTransactionOpen);
				db.Commit();
				db.Commit();
				Assert.IsFalse(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void MultipleRollbacksComplete()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.Rollback();
				Assert.IsFalse(db.IsTransactionOpen);
				db.Rollback();
				db.Rollback();
				Assert.IsFalse(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void CommitClosesTransaction()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.BeginTransaction();
				Assert.IsTrue(db.IsTransactionOpen);
				db.Commit();
				Assert.IsFalse(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void RollbackClosesTransaction()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.BeginTransaction();
				Assert.IsTrue(db.IsTransactionOpen);
				db.Rollback();
				Assert.IsFalse(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void TransactionIsImplicitlyOpened()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.CreateCommand("SELECT 1");
				Assert.IsTrue(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void ImplicitlyOpenedTransactionIsMarked()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsImplicitTransaction);
				db.CreateCommand("SELECT 1");
				Assert.IsTrue(db.IsImplicitTransaction);
			}
		}

		[TestMethod]
		public void ExplicitTransactionIsNotMarked()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.BeginTransaction();
				Assert.IsTrue(db.IsTransactionOpen);
				Assert.IsFalse(db.IsImplicitTransaction);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void ImplicitTransactionCannotBeCommitted()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.CreateCommand("SELECT 1");
				Assert.IsTrue(db.IsTransactionOpen);
				db.Commit();
			}
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void ImplicitTransactionCannotBeRolledBack()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.CreateCommand("SELECT 1");
				Assert.IsTrue(db.IsTransactionOpen);
				db.Rollback();
			}
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void ImplicitTransactionCannotBeCommittedInNestedContext()
		{
			using (Context.Begin())
			{
				var db1 = Context.Current.Get<IDB>();
				db1.CreateCommand("SELECT 1");
				using (Context.Begin())
				{
					var db2 = Context.Current.Get<IDB>();
					Assert.AreEqual(db1, db2);
					Assert.IsTrue(db2.IsTransactionOpen);
					db2.Commit();
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void ImplicitTransactionCannotBeRolledBackInNestedContext()
		{
			using (Context.Begin())
			{
				var db1 = Context.Current.Get<IDB>();
				db1.CreateCommand("SELECT 1");
				using (Context.Begin())
				{
					var db2 = Context.Current.Get<IDB>();
					Assert.AreEqual(db1, db2);
					Assert.IsTrue(db2.IsTransactionOpen);
					db2.Rollback();
				}
			}
		}

		[TestMethod]
		public void NestedCommitDoesntChangeTransaction()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.BeginTransaction();
				Assert.IsTrue(db.IsTransactionOpen);
				using (Context.Begin())
				{
					Assert.IsTrue(db.IsTransactionOpen);
					db.BeginTransaction();
					Assert.IsTrue(db.IsTransactionOpen);
					db.Commit();
					Assert.IsTrue(db.IsTransactionOpen);
				}
				Assert.IsTrue(db.IsTransactionOpen);
				db.Commit();
				Assert.IsFalse(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void NestedRollbackDoesntChangeTransaction()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.BeginTransaction();
				Assert.IsTrue(db.IsTransactionOpen);
				using (Context.Begin())
				{
					Assert.IsTrue(db.IsTransactionOpen);
					db.BeginTransaction();
					Assert.IsTrue(db.IsTransactionOpen);
					db.Rollback();
					Assert.IsTrue(db.IsTransactionOpen);
				}
				Assert.IsTrue(db.IsTransactionOpen);
				db.Rollback();
				Assert.IsFalse(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void NestedImplicitOpenDoesntChangeTransaction()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				db.BeginTransaction();
				Assert.IsTrue(db.IsTransactionOpen);
				using (Context.Begin())
				{
					Assert.IsTrue(db.IsTransactionOpen);
					db.CreateCommand("SELECT 1");
					Assert.IsTrue(db.IsTransactionOpen);
				}
				Assert.IsTrue(db.IsTransactionOpen);
				db.Commit();
				Assert.IsFalse(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void NestedCommitInOwningContextClosesTransaction()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				using (Context.Begin())
				{
					var db2 = Context.Current.Get<IDB>();
					Assert.AreEqual(db, db2);
					Assert.IsFalse(db2.IsTransactionOpen);
					db2.BeginTransaction();
					Assert.IsTrue(db2.IsTransactionOpen);
					db2.Commit();
					Assert.IsFalse(db.IsTransactionOpen);
				}
				Assert.IsFalse(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		public void NestedRollbackInOwningContextClosesTransaction()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				Assert.IsFalse(db.IsTransactionOpen);
				using (Context.Begin())
				{
					var db2 = Context.Current.Get<IDB>();
					Assert.AreEqual(db, db2);
					Assert.IsFalse(db2.IsTransactionOpen);
					db2.BeginTransaction();
					Assert.IsTrue(db2.IsTransactionOpen);
					db2.Rollback();
					Assert.IsFalse(db.IsTransactionOpen);
				}
				Assert.IsFalse(db.IsTransactionOpen);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void NestedCommitWithoutOuterCommitRaisesException()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.BeginTransaction();
				using (Context.Begin())
				{
					var db2 = Context.Current.Get<IDB>();
					db2.BeginTransaction();
					db2.Commit();
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void NestedRollbackWithoutOuterRollbackRaisesException()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.BeginTransaction();
				using (Context.Begin())
				{
					var db2 = Context.Current.Get<IDB>();
					db2.BeginTransaction();
					db2.Rollback();
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(KRUDInvalidOperationException))]
		public void NestedRollbackWithOuterCommitRaisesException()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.BeginTransaction();
				using (Context.Begin())
				{
					var db2 = Context.Current.Get<IDB>();
					db2.BeginTransaction();
					db2.Rollback();
				}
				db.Commit();
			}
		}

		[TestMethod]
		public void NestedCommitWithOuterRollbackCompletes()
		{
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();
				db.BeginTransaction();
				using (Context.Begin())
				{
					var db2 = Context.Current.Get<IDB>();
					db2.BeginTransaction();
					db2.Commit();
				}
				db.Rollback();
			}
		}
	}
}
