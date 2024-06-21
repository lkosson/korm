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
using Npgsql;

namespace Kosson.KORM.PGSQL
{
	/// <summary>
	/// PostgreSQL database provider for KORM.
	/// </summary>
	public class PGSQLDB : ADONETDB
	{
		/// <summary>
		/// Creates a new instance of PostgreSQL database provider.
		/// </summary>
		public PGSQLDB(IOptionsMonitor<KORMOptions> optionsMonitor, ILogger<Kosson.KORM.PGSQL.PGSQLDB> logger)
			: base(optionsMonitor, logger)
		{
		}

		// Prepare is called before parameter value substitution and causes PGSQL to throw syntax error
		/// <inheritdoc/>
		protected override bool PrepareCommands { get { return false; } }

		/// <inheritdoc/>
		protected override bool ReplaceNewLines { get { return true; } }

		/// <inheritdoc/>
		protected override IDBCommandBuilder CreateCommandBuilder()
		{
			return new CommandBuilder();
		}

		/// <inheritdoc/>
		protected override DbConnection CreateConnection()
		{
			var conn = new NpgsqlConnection(ConnectionString);
			conn.Open();
			return conn;
		}

		/// <inheritdoc/>
		protected override object NativeToSQL(object val)
		{
			if (val is bool) return val;
			return base.NativeToSQL(val);
		}

		/// <inheritdoc/>
		protected override KORMException TranslateException(Exception exc, DbCommand cmd)
		{
			var pe = exc as PostgresException;
			if (pe != null)
			{
				if (pe.SqlState == "23503") return new KORMForeignKeyException(exc, cmd, pe.TableName);
				// TODO: Use ErrorCode instead of parsing messages
				if (pe.Message.StartsWith("ERROR: 55P03:")) return new KORMLockException(exc, cmd);
				if (pe.Message.StartsWith("A timeout has occured.")) return new KORMLockException(exc, cmd);
				// All exceptions cause transaction to rollback anyway
			}
			return base.TranslateException(exc, cmd);
		}
	}
}
