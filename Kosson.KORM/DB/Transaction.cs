using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.DB
{
	class Transaction(IDB db) : ITransaction
	{
		bool ITransaction.IsOpen => db.IsTransactionOpen && !db.IsImplicitTransaction;

		void ITransaction.Commit() => db.Commit();

		void ITransaction.Rollback() => db.Rollback();

		void IDisposable.Dispose()
		{
			if (db.IsTransactionOpen) db.Rollback();
		}
	}
}
