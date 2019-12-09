using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kosson.KRUD
{
	class OracleDB : ADONETDB
	{
		protected override bool ReplaceNewLines { get { return true; } }

		protected override DbConnection CreateConnection()
		{
			Assembly asm = Assembly.Load(new AssemblyName("Oracle.DataAccess"));
			if (asm == null) throw new TypeLoadException("Brak Oracle.DataAccess");
			Type type = asm.GetType("Oracle.DataAccess.Client.OracleConnection");
			if (type == null) throw new TypeLoadException("Brak Oracle.DataAccess.Client.OracleConnection");
			DbConnection conn = Activator.CreateInstance(type) as DbConnection;
			conn.ConnectionString = ConnectionString;
			conn.Open();

			return conn;
		}

		protected override Exception TranslateException(Exception exc, DbCommand cmd)
		{
			if (exc.GetType().Name == "OracleException")
			{
				if (exc.Message.StartsWith("ORA-30006:")) return new KRUDLockException(exc);
			}

			return base.TranslateException(exc, cmd);
		}
	}
}
