using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using Kosson.KORM.DB;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;

namespace Kosson.KORM.SQLite
{
	/// <summary>
	/// SQLite database provider for KORM.
	/// </summary>
	public class SQLiteDB : ADONETDB
	{
		/// <summary>
		/// Creates a new instance of SQLite database provider.
		/// </summary>
		public SQLiteDB(IOptionsMonitor<KORMOptions> optionsMonitor, ILogger<Kosson.KORM.SQLite.SQLiteDB> logger)
			: base(optionsMonitor, logger)
		{
		}

		/// <inheritdoc/>
		protected override IDBCommandBuilder CreateCommandBuilder()
		{
			return new CommandBuilder();
		}

		/// <inheritdoc/>
		protected override DbConnection CreateConnection()
		{
			var csb = new SqliteConnectionStringBuilder();
			csb.Mode = SqliteOpenMode.ReadWrite;

			var conn = new SqliteConnection(ConnectionString);
			conn.Open();
			return conn;
		}

		/// <inheritdoc/>
		protected override object NativeToSQL(object? val)
		{
			if (val is bool) return val;
			if (val is DateTime date) return date.ToString("yyyy-MM-dd HH:mm:ss.ffff", System.Globalization.CultureInfo.InvariantCulture);
			return base.NativeToSQL(val);
		}

		/// <inheritdoc/>
		protected override KORMException? TranslateException(Exception exc, string ?commandText, DbParameterCollection? commandParameters)
		{
			if (exc is SqliteException se)
			{
				if (se.SqliteExtendedErrorCode == 787) return new KORMForeignKeyException(se, commandText, commandParameters, null);
				if (se.SqliteExtendedErrorCode == 2067) return new KORMDuplicateValueException(se, commandText, commandParameters);
				if (se.Message.Contains("duplicate column name")) return new KORMObjectExistsException(se, commandText, commandParameters);
				if (se.Message.Contains("already exists")) return new KORMObjectExistsException(se, commandText, commandParameters);
			}
			return base.TranslateException(exc, commandText, commandParameters);
		}
	}
}
