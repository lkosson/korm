using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Kosson.KRUD
{
	/// <summary>
	/// Base class for exceptions indicating error during database communication or processing.
	/// </summary>
	[Serializable]
	public class KRUDException : Exception
	{
		private DbCommand cmd;

		/// <summary>
		/// Command that caused the exception.
		/// </summary>
		public DbCommand Command { get { return cmd; } }

		/// <summary>
		/// Creates a new KRUDException with error message.
		/// </summary>
		/// <param name="msg">Exception message.</param>
		public KRUDException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// Creates a new KRUDException with error message and underlying database engine-specific exception.
		/// </summary>
		/// <param name="msg">Exception message.</param>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		public KRUDException(string msg, Exception inner)
			: base(msg, inner)
		{
		}

		/// <summary>
		/// Creates a new KRUDException with error message, underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="msg">Exception message.</param>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="cmd">Command causing the exception.</param>
		public KRUDException(string msg, Exception inner, DbCommand cmd)
			: base(msg + (cmd == null ? "" : ("\n\n" + cmd.CommandText)), inner)
		{
			this.cmd = cmd;
		}
	}

	/// <summary>
	/// Exception indicating error condition during database connection attempt.
	/// </summary>
	[Serializable]
	public class KRUDConnectionException : KRUDException
	{
		/// <summary>
		/// Creates a new exception from underlying, database engine-specific exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		public KRUDConnectionException(Exception inner)
			: base("Database connection error: " + inner.Message, inner)
		{
		}
	}

	/// <summary>
	/// Exception indicating attempt to perform invalid or unavailable operation.
	/// </summary>
	[Serializable]
	public class KRUDInvalidOperationException : KRUDException
	{
		/// <summary>
		/// Creates a new exception with description of the cause.
		/// </summary>
		/// <param name="message">Underlying, database engine-specific exception.</param>
		public KRUDInvalidOperationException(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Exception indicating attempt to insert duplicate value to a column declared as unique.
	/// </summary>
	[Serializable]
	public class KRUDDuplicateValueException : KRUDException
	{
		/// <summary>
		/// Creates a new exception from underlying, database engine-specific exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		public KRUDDuplicateValueException(Exception inner)
			: base("Duplicate value in unique field.", inner)
		{
		}
	}

	/// <summary>
	/// Exception indicating database lock acquisition timeout.
	/// </summary>
	[Serializable]
	public class KRUDLockException : KRUDException
	{
		/// <summary>
		/// Creates a new exception from underlying, database engine-specific exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		public KRUDLockException(Exception inner)
			: base("Row is locked in another transaction.", inner)
		{
		}
	}

	/// <summary>
	/// Exception indicating concurrent modification of a database row.
	/// </summary>
	[Serializable]
	public class KRUDConcurrentModificationException : KRUDException
	{
		/// <summary>
		/// Creates a new exception with information about causing command.
		/// </summary>
		/// <param name="cmd">Command that caused the exception.</param>
		public KRUDConcurrentModificationException(DbCommand cmd)
			: base("Row has been modified in another transaction.", null, cmd)
		{
		}
	}

	/// <summary>
	/// Base exception for signalling errors during database structure creation.
	/// </summary>
	[Serializable]
	public class KRUDInvalidStructureException : KRUDException
	{
		/// <summary>
		/// Creates a new exception from underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="cmd">Command causing the exception.</param>
		public KRUDInvalidStructureException(Exception inner, DbCommand cmd)
			: base("Database structure creation error.", inner, cmd)
		{
		}

		/// <summary>
		/// Creates a new exception with error message, underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="msg">Exception message.</param>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="cmd">Command causing the exception.</param>
		public KRUDInvalidStructureException(string msg, Exception inner, DbCommand cmd)
			: base(msg, inner, cmd)
		{
		}
	}

	/// <summary>
	/// Exception indicating that database object already exists.
	/// </summary>
	[Serializable]
	public class KRUDObjectExistsException : KRUDInvalidStructureException
	{
		/// <summary>
		/// Creates a new exception from underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="cmd">Command causing the exception.</param>
		public KRUDObjectExistsException(Exception inner, DbCommand cmd)
			: base("Database object already exists.", inner, cmd)
		{
		}
	}

	/// <summary>
	/// Exception indicating foreign key violation.
	/// </summary>
	[Serializable]
	public class KRUDForeignKeyException : KRUDException
	{
		/// <summary>
		/// Table referenced by a violated foreign key constraint.
		/// </summary>
		public string Remote { get; set; }

		/// <summary>
		/// Creates a new exception from underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="cmd">Command causing the exception.</param>
		/// <param name="remote">Table referenced by violated foreign key.</param>
		public KRUDForeignKeyException(Exception inner, DbCommand cmd, string remote)
			: base("Operation failed due to foreign key violation in table \"" + remote + "\".", inner, cmd)
		{
			Remote = remote;
		}
	}

	/// <summary>
	/// Exception indicating an attempt to store value larger than declared maximum for a database column.
	/// </summary>
	[Serializable]
	public class KRUDDataLengthException : KRUDException
	{
		/// <summary>
		/// Creates a new exception from underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="cmd">Command causing the exception.</param>
		public KRUDDataLengthException(Exception inner, DbCommand cmd)
			: base("Data too long.", inner, cmd)
		{
		}

		/// <summary>
		/// Creates a new exception for length mismatch detected before sending command to database engine.
		/// </summary>
		/// <param name="field">Property causing error.</param>
		/// <param name="maxlen">Maximum length declared for a column.</param>
		/// <param name="val">Value to store in column</param>
		public KRUDDataLengthException(string field, int maxlen, string val)
			: base("Maximum data lenght for column \"" + field + "\" (" + maxlen + ") is too small for value \"" + (val == null ? "" : val.Length > 100 ? val.Substring(0, 100) + "... (" + val.Length + ")" : val) + "\"")
		{
		}
	}

	/// <summary>
	/// Base exception indicating ORM operation failure.
	/// </summary>
	[Serializable]
	public class ORMException : KRUDException
	{
		/// <summary>
		/// Creates a new exception with a given message.
		/// </summary>
		/// <param name="message">Exception message.</param>
		public ORMException(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Exception indicating that number of rows updated by database is different than expected.
	/// </summary>
	[Serializable]
	public class ORMUpdateFailedException : ORMException
	{
		/// <summary>
		/// Creates a new exception.
		/// </summary>
		public ORMUpdateFailedException()
			: base("Update failed.")
		{
		}
	}

	/// <summary>
	/// Exception indicating that number of rows inserted to database is different than expected.
	/// </summary>
	[Serializable]
	public class ORMInsertFailedException : ORMException
	{
		/// <summary>
		/// Creates a new exception.
		/// </summary>
		public ORMInsertFailedException()
			: base("Insert failed.")
		{
		}
	}

	/// <summary>
	/// Exception indicating that number of rows deleted by database is different than expected.
	/// </summary>
	[Serializable]
	public class ORMDeleteFailedException : ORMException
	{
		/// <summary>
		/// Creates a new exception.
		/// </summary>
		public ORMDeleteFailedException()
			: base("Delete failed.")
		{
		}
	}

	/// <summary>
	/// Exception during backup operation.
	/// </summary>
	[Serializable]
	public class KRUDBackupException : KRUDException
	{
		/// <summary>
		/// Creates a new exception.
		/// </summary>
		/// <param name="message">Description of the exception.</param>
		public KRUDBackupException(string message)
			: base(message)
		{
		}
	}
}
