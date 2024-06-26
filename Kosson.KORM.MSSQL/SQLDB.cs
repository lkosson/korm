using Kosson.KORM;
using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Kosson.KORM.DB;
using Microsoft.Extensions.Options;

namespace Kosson.KORM.MSSQL
{
	/// <summary>
	/// Microsoft SQL Server database provider for KORM.
	/// </summary>
	public class SQLDB : ADONETDB
	{
		/// <summary>
		/// Creates a new instance of Microsoft SQL Server provider.
		/// </summary>
		public SQLDB(IOptionsMonitor<KORMOptions> optionsMonitor, ILogger<Kosson.KORM.MSSQL.SQLDB> logger)
			: base(optionsMonitor, logger)
		{
		}

		/// <inheritdoc/>
		public override bool IsBatchSupported => true;

		/// <inheritdoc/>
		protected override IDBCommandBuilder CreateCommandBuilder()
		{
			return new CommandBuilder();
		}

		/// <inheritdoc/>
		protected override DbConnection CreateConnection()
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			conn.Open();

			if (CommandTimeout.HasValue)
			{
				using var cmd = new SqlCommand(@"SET LOCK_TIMEOUT " + CommandTimeout.Value, conn);
				cmd.ExecuteNonQuery();
			}

			return conn;
		}

		/// <inheritdoc/>
		protected override void CreateDatabaseImpl()
		{
			var csb = new SqlConnectionStringBuilder(ConnectionString);
			string orgdb = csb.InitialCatalog;
			csb.InitialCatalog = "master";
			csb.Pooling = false; // there will be only one such connection
			using SqlConnection conn = new SqlConnection(csb.ToString());
			conn.Open();
			using var cmd = new SqlCommand("IF NOT EXISTS(SELECT name FROM sysdatabases WHERE name = @DB) BEGIN CREATE DATABASE [" + orgdb + "]; ALTER DATABASE [" + orgdb + "] SET READ_COMMITTED_SNAPSHOT ON; ALTER DATABASE [" + orgdb + "] SET ALLOW_SNAPSHOT_ISOLATION ON; END", conn);
			((IDB)this).AddParameter(cmd, "@DB", orgdb);
			cmd.ExecuteNonQuery();
		}

		/// <inheritdoc/>
		protected override void FixParameter(DbParameter parameter)
		{
			base.FixParameter(parameter);
			// SQL Server caches query plans by parameter types.
			// SqlClient passes decimal and string values with type definition
			// exactly matching passed value (e.g. numeric(7,1), varchar(11)).
			// This can lead to large number of similar cached plans for same query.
			if (parameter.Value is decimal)
			{
				var sp = (SqlParameter)parameter;
				System.Data.SqlTypes.SqlDecimal sd = (System.Data.SqlTypes.SqlDecimal)sp.SqlValue;
				if (sd.Precision < 38) sp.Precision = 38;
				if (sd.Scale < 8) sp.Scale = 8;
			}
			else if (parameter.Value is string stringValue)
			{
				int len = parameter.Size;
				if (len < 4000) len = 4000;
				if (stringValue.Length > 4000) len = -1;
				parameter.Size = len;
			}
		}

		/// <inheritdoc/>
		protected override KORMException TranslateException(Exception exc, string commandText, DbParameterCollection commandParameters)
		{
			if (exc is SqlException se)
			{
				if (se.Number == 1222) return new KORMLockException(exc, commandText, commandParameters);
				else if (se.Number == 2601) return new KORMDuplicateValueException(exc, commandText, commandParameters); // duplicate unique key
				else if (se.Number == 2627) return new KORMDuplicateValueException(exc, commandText, commandParameters); // duplicate primary key
				else if (se.Number == 2628) return new KORMDataLengthException(exc, commandText, commandParameters); // data truncated
				else if (se.Number == 3701) return new KORMInvalidStructureException(exc, commandText, commandParameters); // permissions
				else if (se.Number == 1785) return new KORMInvalidStructureException(exc, commandText, commandParameters); // multiple cascade paths
				else if (se.Number == 2714) return new KORMObjectExistsException(exc, commandText, commandParameters); // object exists
				else if (se.Number == 2705) return new KORMObjectExistsException(exc, commandText, commandParameters); // column exists
				else if (se.Number == 10054) return new KORMConnectionException(exc); // winsock - connection reset
				else if (se.Number == 64) return new KORMConnectionException(exc); // name no longer available
				else if (se.Number == 1231) return new KORMConnectionException(exc); // conn err
				else if (se.Number == 547)
				{
					var idx = se.Message.IndexOf("table");
					if (idx < 0) return new KORMForeignKeyException(exc, commandText, commandParameters, se.Message);
					string tab = se.Message.Substring(idx);
					tab = tab.Substring(tab.IndexOf('"') + 1);
					tab = tab.Substring(0, tab.IndexOf('"'));
					return new KORMForeignKeyException(exc, commandText, commandParameters, tab);
				}
			}
			return base.TranslateException(exc, commandText, commandParameters);
		}
	}
}
