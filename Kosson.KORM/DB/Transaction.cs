using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.DB
{
	class Transaction : ITransaction
	{
		private readonly IDB db;

		bool ITransaction.IsOpen => db.IsTransactionOpen && !db.IsImplicitTransaction;

		public Transaction(IDB db)
		{
			this.db = db;
		}

		void ITransaction.Commit()
		{
			db.Commit();
		}

		void ITransaction.Rollback()
		{
			db.Rollback();
		}

		void IDisposable.Dispose()
		{
			if (db.IsTransactionOpen) db.Rollback();
		}
	}
}
