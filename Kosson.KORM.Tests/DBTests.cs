using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;

namespace Kosson.KORM.Tests
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
		[ExpectedException(typeof(KORMInvalidOperationException))]
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
		[ExpectedException(typeof(KORMInvalidOperationException))]
		public void CommitWithoutTransactionFails()
		{
			DB.Commit();
		}

		[TestMethod]
		[ExpectedException(typeof(KORMInvalidOperationException))]
		public void RollbackWithoutTransactionFails()
		{
			DB.Rollback();
		}

		[TestMethod]
		[ExpectedException(typeof(KORMInvalidOperationException))]
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
		[ExpectedException(typeof(KORMInvalidOperationException))]
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
		[ExpectedException(typeof(KORMInvalidOperationException))]
		public void ImplicitTransactionCannotBeCommitted()
		{
			DB.CreateCommand("SELECT 1");
			Assert.IsTrue(DB.IsTransactionOpen);
			DB.Commit();
		}

		[TestMethod]
		[ExpectedException(typeof(KORMInvalidOperationException))]
		public void ImplicitTransactionCannotBeRolledBack()
		{
			DB.CreateCommand("SELECT 1");
			Assert.IsTrue(DB.IsTransactionOpen);
			DB.Rollback();
		}

		[TestMethod]
		[ExpectedException(typeof(KORMInvalidOperationException))]
		public void ImplicitTransactionCannotBeReOpened()
		{
			DB.CreateCommand("SELECT 1");
			DB.BeginTransaction();
		}

		[TestMethod]
		public void DisposeClosesTransaction()
		{
			using (var tx = DB.BeginTransaction())
			{
			}
			Assert.IsFalse(DB.IsTransactionOpen);
		}

		[TestMethod]
		public void DisposeAfterCommitCompletes()
		{
			using (var tx = DB.BeginTransaction())
			{
				tx.Commit();
			}
		}

		[TestMethod]
		public void QueryByRawSQLRetrievesValue()
		{
			var rows = DB.ExecuteQueryRaw("SELECT " + MainTestTable.DEFAULTVALUE);
			Assert.IsNotNull(rows);
			Assert.AreEqual(1, rows.Count);
			Assert.IsNotNull(rows[0][0]);
			Assert.AreEqual(MainTestTable.DEFAULTVALUE.ToString(), rows[0][0].ToString());
		}

		[TestMethod]
		public void QueryByInterpolatedSQLRetrievesValue()
		{
			var rows = DB.ExecuteQuery($"SELECT {MainTestTable.DEFAULTVALUE}");
			Assert.IsNotNull(rows);
			Assert.AreEqual(1, rows.Count);
			Assert.IsNotNull(rows[0][0]);
			Assert.AreEqual(MainTestTable.DEFAULTVALUE.ToString(), rows[0][0].ToString());
		}

		[TestMethod]
		public void QueryByInterpolatedSQLIsParametrized()
		{
			var injectionTest = "' \" [ ]] @P5 -- \\'";
			var rows = DB.ExecuteQuery($"SELECT {injectionTest}");
			Assert.IsNotNull(rows);
			Assert.AreEqual(1, rows.Count);
			Assert.IsNotNull(rows[0][0]);
			Assert.AreEqual(injectionTest, rows[0][0]);
		}

		[TestMethod]
		[ExpectedException(typeof(KORMInvalidOperationException))]
		public void ExecuteNonQueryInImplicitTransactionFails()
		{
			DB.ExecuteNonQuery($"SELECT 1");
		}
	}
}
