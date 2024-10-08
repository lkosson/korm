﻿using System;
using System.Data.Common;

namespace Kosson.KORM
{
	/// <summary>
	/// Base class for exceptions indicating error during database communication or processing.
	/// </summary>
	[Serializable]
	public class KORMException : Exception
	{
		/// <summary>
		/// Command that caused the exception.
		/// </summary>
		public string? CommandText { get; }

		/// <summary>
		/// Parameters of the command that caused the exception.
		/// </summary>
		public DbParameterCollection? CommandParameters { get; }

		/// <summary>
		/// Exception message without appended command text.
		/// </summary>
		public string OriginalMessage { get; }

		/// <summary>
		/// Creates a new KORMException with error message.
		/// </summary>
		/// <param name="msg">Exception message.</param>
		public KORMException(string msg)
			: base(msg)
		{
			OriginalMessage = msg;
		}

		/// <summary>
		/// Creates a new KORMException with error message and underlying database engine-specific exception.
		/// </summary>
		/// <param name="msg">Exception message.</param>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		public KORMException(string msg, Exception? inner)
			: base(msg, inner)
		{
			OriginalMessage = msg;
		}

		/// <summary>
		/// Creates a new KORMException with error message, underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="msg">Exception message.</param>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		public KORMException(string msg, Exception? inner, string? commandText, DbParameterCollection? commandParameters)
			: base(msg + (commandText == null ? "" : "\n\n" + commandText), inner)
		{
			CommandText = commandText;
			CommandParameters = commandParameters;
			OriginalMessage = msg;
		}
	}

	/// <summary>
	/// Exception indicating error condition during database connection attempt.
	/// </summary>
	[Serializable]
	public class KORMConnectionException : KORMException
	{
		/// <summary>
		/// Creates a new exception from underlying, database engine-specific exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		public KORMConnectionException(Exception inner)
			: base("Database connection error: " + inner.Message, inner)
		{
		}
	}

	/// <summary>
	/// Exception indicating attempt to perform invalid or unavailable operation.
	/// </summary>
	[Serializable]
	public class KORMInvalidOperationException : KORMException
	{
		/// <summary>
		/// Creates a new exception with description of the cause.
		/// </summary>
		/// <param name="message">Underlying, database engine-specific exception.</param>
		public KORMInvalidOperationException(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Exception indicating attempt to insert duplicate value to a column declared as unique.
	/// </summary>
	[Serializable]
	public class KORMDuplicateValueException : KORMException
	{
		/// <summary>
		/// Creates a new exception from underlying, database engine-specific exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		public KORMDuplicateValueException(Exception inner, string? commandText, DbParameterCollection? commandParameters)
			: base("Duplicate value in unique field: " + inner.Message, inner, commandText, commandParameters)
		{
		}
	}

	/// <summary>
	/// Exception indicating database lock acquisition timeout.
	/// </summary>
	[Serializable]
	public class KORMLockException : KORMException
	{
		/// <summary>
		/// Creates a new exception from underlying, database engine-specific exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		public KORMLockException(Exception inner, string? commandText, DbParameterCollection? commandParameters)
			: base("Row is locked in another transaction: " + inner.Message, inner, commandText, commandParameters)
		{
		}
	}

	/// <summary>
	/// Exception indicating concurrent modification of a database row.
	/// </summary>
	[Serializable]
	public class KORMConcurrentModificationException : KORMException
	{
		/// <summary>
		/// Creates a new exception with information about causing command.
		/// </summary>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		public KORMConcurrentModificationException(string? commandText, DbParameterCollection? commandParameters)
			: base("Row has been modified in another transaction.", null, commandText, commandParameters)
		{
		}
	}

	/// <summary>
	/// Base exception for signalling errors during database structure creation.
	/// </summary>
	[Serializable]
	public class KORMInvalidStructureException : KORMException
	{
		/// <summary>
		/// Creates a new exception from underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		public KORMInvalidStructureException(Exception inner, string? commandText, DbParameterCollection? commandParameters)
			: base("Database structure creation error: " + inner.Message, inner, commandText, commandParameters)
		{
		}

		/// <summary>
		/// Creates a new exception with error message, underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="msg">Exception message.</param>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		public KORMInvalidStructureException(string msg, Exception inner, string? commandText, DbParameterCollection? commandParameters)
			: base(msg, inner, commandText, commandParameters)
		{
		}
	}

	/// <summary>
	/// Exception indicating that database object already exists.
	/// </summary>
	[Serializable]
	public class KORMObjectExistsException : KORMInvalidStructureException
	{
		/// <summary>
		/// Creates a new exception from underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		public KORMObjectExistsException(Exception inner, string? commandText, DbParameterCollection? commandParameters)
			: base("Database object already exists: " + inner.Message, inner, commandText, commandParameters)
		{
		}
	}

	/// <summary>
	/// Exception indicating foreign key violation.
	/// </summary>
	[Serializable]
	public class KORMForeignKeyException : KORMException
	{
		/// <summary>
		/// Table referenced by a violated foreign key constraint.
		/// </summary>
		public string? Remote { get; set; }

		/// <summary>
		/// Creates a new exception from underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		/// <param name="remote">Table referenced by violated foreign key.</param>
		public KORMForeignKeyException(Exception inner, string? commandText, DbParameterCollection? commandParameters, string? remote)
			: base("Operation failed due to foreign key violation in table \"" + remote + "\".", inner, commandText, commandParameters)
		{
			Remote = remote;
		}
	}

	/// <summary>
	/// Exception indicating an attempt to store value larger than declared maximum for a database column.
	/// </summary>
	[Serializable]
	public class KORMDataLengthException : KORMException
	{
		/// <summary>
		/// Creates a new exception from underlying database engine-specific exception and command causing the exception.
		/// </summary>
		/// <param name="inner">Underlying, database engine-specific exception.</param>
		/// <param name="commandText">Text of the command that caused the exception.</param>
		/// <param name="commandParameters">Parameters of the command causing the exception.</param>
		public KORMDataLengthException(Exception inner, string? commandText, DbParameterCollection? commandParameters)
			: base("Data too long: " + inner.Message, inner, commandText, commandParameters)
		{
		}

		/// <summary>
		/// Creates a new exception for length mismatch detected before sending command to database engine.
		/// </summary>
		/// <param name="field">Property causing error.</param>
		/// <param name="maxlen">Maximum length declared for a column.</param>
		/// <param name="val">Value to store in column</param>
		public KORMDataLengthException(string field, int maxlen, string val)
			: base("Maximum data lenght for column \"" + field + "\" (" + maxlen + ") is too small for value \"" + (val == null ? "" : val.Length > 100 ? val.Substring(0, 100) + "... (" + val.Length + ")" : val) + "\"")
		{
		}
	}

	/// <summary>
	/// Exception indicating that number of rows updated by database is different than expected.
	/// </summary>
	[Serializable]
	public class KORMUpdateFailedException : KORMException
	{
		/// <summary>
		/// Creates a new exception.
		/// </summary>
		public KORMUpdateFailedException()
			: base("Update failed.")
		{
		}
	}

	/// <summary>
	/// Exception indicating that number of rows inserted to database is different than expected.
	/// </summary>
	[Serializable]
	public class KORMInsertFailedException : KORMException
	{
		/// <summary>
		/// Creates a new exception.
		/// </summary>
		public KORMInsertFailedException()
			: base("Insert failed.")
		{
		}
	}

	/// <summary>
	/// Exception indicating that number of rows deleted by database is different than expected.
	/// </summary>
	[Serializable]
	public class KORMDeleteFailedException : KORMException
	{
		/// <summary>
		/// Creates a new exception.
		/// </summary>
		public KORMDeleteFailedException()
			: base("Delete failed.")
		{
		}
	}

	/// <summary>
	/// Exception during backup operation.
	/// </summary>
	[Serializable]
	public class KORMBackupException : KORMException
	{
		/// <summary>
		/// Creates a new exception.
		/// </summary>
		/// <param name="message">Description of the exception.</param>
		public KORMBackupException(string message)
			: base(message)
		{
		}
	}
}
