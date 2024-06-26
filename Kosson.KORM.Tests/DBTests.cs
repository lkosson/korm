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
			var tx = DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
			Assert.IsTrue(tx.IsOpen);
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
			var tx = DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
			Assert.IsTrue(tx.IsOpen);
			DB.Commit();
			Assert.IsFalse(DB.IsTransactionOpen);
			Assert.IsFalse(tx.IsOpen);
		}

		[TestMethod]
		public void RollbackClosesTransaction()
		{
			Assert.IsFalse(DB.IsTransactionOpen);
			var tx = DB.BeginTransaction();
			Assert.IsTrue(DB.IsTransactionOpen);
			Assert.IsTrue(tx.IsOpen);
			DB.Rollback();
			Assert.IsFalse(DB.IsTransactionOpen);
			Assert.IsFalse(tx.IsOpen);
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
		public void ImplicitTransactionCanBePromoted()
		{
			DB.CreateCommand("SELECT 1");
			Assert.IsTrue(DB.IsImplicitTransaction);
			var tx = DB.BeginTransaction();
			Assert.IsFalse(DB.IsImplicitTransaction);
			Assert.IsTrue(tx.IsOpen);
			DB.Commit();
			Assert.IsFalse(DB.IsTransactionOpen);
			Assert.IsFalse(tx.IsOpen);
		}

		[TestMethod]
		public void PromotedTransactionRevertsToImplicit()
		{
			DB.CreateCommand("SELECT 1");
			Assert.IsTrue(DB.IsImplicitTransaction);
			using (var tx = DB.BeginTransaction())
			{
			}
			DB.CreateCommand("SELECT 1");
			Assert.IsTrue(DB.IsImplicitTransaction);
			Assert.IsTrue(DB.IsTransactionOpen);
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

		[TestMethod]
		public void BatchExecutes()
		{
			if (!DB.IsBatchSupported) return;
			using var batch = DB.CreateBatch();
			var cmd1 = DB.CreateCommand(batch, "SELECT 1");
			var cmd2 = DB.CreateCommand(batch, "SELECT 2");
			using var reader = DB.ExecuteReader(batch);
			Assert.IsTrue(reader.Read());
			Assert.IsFalse(reader.Read());
			Assert.IsTrue(reader.NextResult());
			Assert.IsTrue(reader.Read());
			Assert.IsFalse(reader.Read());
			Assert.IsFalse(reader.NextResult());
		}

		[TestMethod]
		public void BatchParametersAreSupported()
		{
			if (!DB.IsBatchSupported) return;
			using var batch = DB.CreateBatch();
			var count = DB.CommandBuilder.MaxParameterCount * 2;
			for (int i = 0; i < count; i++)
			{
				var parName = DB.CommandBuilder.ParameterPrefix + "P" + i;
				var cmd = DB.CreateCommand(batch, "SELECT " + parName);
				DB.AddParameter(cmd, parName, i);
			}
			using var reader = DB.ExecuteReader(batch);
			for (int i = 0; i < count; i++)
			{
				Assert.IsTrue(reader.Read());
				var val = reader.GetInt32(0);
				Assert.AreEqual(i, val);
				Assert.IsFalse(reader.Read());
				if (i == count - 1) Assert.IsFalse(reader.NextResult());
				else Assert.IsTrue(reader.NextResult());
			}
		}

		[TestMethod]
		public void BatchDuplicatedParametersAreIndependent()
		{
			if (!DB.IsBatchSupported) return;
			using var batch = DB.CreateBatch();
			var count = 100;
			for (int i = 0; i < count; i++)
			{
				var parName = DB.CommandBuilder.ParameterPrefix + "P0";
				var cmd = DB.CreateCommand(batch, "SELECT " + parName);
				DB.AddParameter(cmd, parName, i);
			}
			using var reader = DB.ExecuteReader(batch);
			for (int i = 0; i < count; i++)
			{
				Assert.IsTrue(reader.Read());
				var val = reader.GetInt32(0);
				Assert.AreEqual(i, val);
				Assert.IsFalse(reader.Read());
				if (i == count - 1) Assert.IsFalse(reader.NextResult());
				else Assert.IsTrue(reader.NextResult());
			}
		}

		[TestMethod]
		[ExpectedException(typeof(KORMException))]
		public void BatchThrowsWhenConsumingResult()
		{
			if (!DB.IsBatchSupported) return;
			using var batch = DB.CreateBatch();
			DB.CreateCommand(batch, "INVALIDCOMMAND");
			using var reader = DB.ExecuteReader(batch);
		}

		[TestMethod]
		public void BatchDontThrowWhenNotConsumingResult()
		{
			if (!DB.IsBatchSupported) return;
			using var batch = DB.CreateBatch();
			DB.CreateCommand(batch, "SELECT 1");
			DB.CreateCommand(batch, "INVALIDCOMMAND");
			var reader = DB.ExecuteReader(batch);
			Assert.IsTrue(reader.Read());
			try
			{
				reader.Dispose();
			}
			catch
			{
				// ignored - might throw subsequent error
			}
		}

		[TestMethod]
		public void BatchThrownExceptionContainsCommandAndParameters()
		{
			if (!DB.IsBatchSupported) return;
			using var batch = DB.CreateBatch();
			var command = DB.CreateCommand(batch, "INVALIDCOMMAND");
			DB.AddParameter(command, "@P0", 123);
			try
			{
				using var reader = DB.ExecuteReader(batch);
				Assert.Fail();
			}
			catch (KORMException exc)
			{
				Assert.IsNotNull(exc.CommandText);
				Assert.IsNotNull(exc.CommandParameters);
				Assert.AreEqual(1, exc.CommandParameters.Count);
				Assert.AreEqual(123, exc.CommandParameters[0].Value);
			}
		}
	}
}
