#if SQLCE
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;

namespace Kosson.KRUD
{
	class SQLCEDB : ADONETDB
	{
		protected override bool SupportsCancel { get { return false; } }

		private string DBFile
		{
			get
			{
				string db = base.ConnectionString;
				if (String.IsNullOrEmpty(db)) db = "default.sdf";
				var file = Path.Combine(Context.State().DataPath, db);
				return file;
			}
		}

		public override string ConnectionString { get { return "data source=" + DBFile; } }

		public SQLCEDB()
		{
			AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);
		}

		public override void CreateDatabase()
		{
			if (!File.Exists(DBFile))
			{
				using (SqlCeEngine engine = new SqlCeEngine(ConnectionString))
				{
					engine.CreateDatabase();
				}
			}
		}

		protected override IDbConnection CreateConnection()
		{
			SqlCeConnection conn = new SqlCeConnection(ConnectionString);
			conn.Open();

			using (var cmd = new SqlCeCommand(@"SET LOCK_TIMEOUT 2000", conn))
				cmd.ExecuteNonQuery();

			return conn;
		}

		protected override void FixParameter(DbParameter parameter)
		{
			if (parameter.Value is byte[])
			{
				((SqlCeParameter)parameter).SqlDbType = SqlDbType.Image;
			}
			base.FixParameter(parameter);
		}

		protected override void HandleException(Exception exc, DbCommand cmd)
		{
			SqlCeException se = exc as SqlCeException;
			if (se != null)
			{
				if (se.NativeError == 25090) throw new DBLockError(exc);
				else if (se.NativeError == 25016) throw new DBKeyError(exc);
				else if (se.Message == "A column ID occurred more than once in the specification.") throw new DBStructureError(exc, cmd);
				else if (se.Message == "Too many identity columns are specified for the table. Only one identity column for each table is allowed.") throw new DBStructureError(exc, cmd);
				else if (se.Message.StartsWith("The specified table already exists.")) throw new DBStructureError(exc, cmd);
				else if (se.Message.StartsWith("The specified index already exists.")) throw new DBStructureError(exc, cmd);
				else if (se.Message.StartsWith("Constraint already exists.")) throw new DBStructureError(exc, cmd);
				else if (se.Message.StartsWith("The referential relationship will result in a cyclical reference that is not allowed.")) throw new DBStructureError(exc, cmd); // wielościeżkowe kaskady pod to podpadają
				/*
			else if (se.Number == 3701) throw new DBStructureError(exc, cmd);
			else if (se.Number == 1785) throw new DBStructureError(exc, cmd); // FK exists
			else if (se.Number == 2714) throw new DBStructureError(exc, cmd); // object exists
			*/
			}
			base.HandleException(exc, cmd);
		}

	}
}
#endif