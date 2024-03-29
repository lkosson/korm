﻿using System;
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

		// Cancelling already completed query (e.g. getLastID during batch insert) causes next execution of the command to return no rows.
		/// <inheritdoc/>
		protected override bool SupportsCancel { get { return false; } }

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
		protected override object NativeToSQL(object val)
		{
			if (val is bool) return val;
			if (val is DateTime) return ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss.ffff", System.Globalization.CultureInfo.InvariantCulture);
			return base.NativeToSQL(val);
		}

		/// <inheritdoc/>
		protected override KORMException TranslateException(Exception exc, DbCommand cmd)
		{
			var se = exc as SqliteException;
			if (se != null)
			{
				if (se.SqliteExtendedErrorCode == 787) return new KORMForeignKeyException(se, cmd, null);
				if (se.Message.Contains("duplicate column name")) return new KORMObjectExistsException(se, cmd);
				if (se.Message.Contains("already exists")) return new KORMObjectExistsException(se, cmd);
			}
			return base.TranslateException(exc, cmd);
		}
	}
}
