using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Kosson.KRUD
{
	class FirebirdDB : ADONETDB
	{
		protected override bool ReplaceNewLines { get { return true; } }

		protected override DbConnection CreateConnection()
		{
			Assembly asm = Assembly.Load(new AssemblyName("FirebirdSql.Data.FirebirdClient"));
			if (asm == null) throw new TypeLoadException("Brak FirebirdSql.Data.FirebirdClient");
			Type type = asm.GetType("FirebirdSql.Data.FirebirdClient.FbConnection");
			if (type == null) throw new TypeLoadException("FirebirdSql.Data.FirebirdClient.FbConnection");
			DbConnection conn = Activator.CreateInstance(type) as DbConnection;
			conn.ConnectionString = ConnectionString;
			conn.Open();

			return conn;
		}

		protected override Exception TranslateException(Exception exc, DbCommand cmd)
		{
			if (exc.GetType().Name == "FbException")
			{
				if (exc.Message.StartsWith("unsuccessful metadata update"))
				{
					if (exc.Message.Contains("RDB$RELATION_FIELDS")) return new KRUDInvalidStructureException(exc, cmd);
					if (Regex.IsMatch(exc.Message, "Table \\w* already exists")) return new KRUDInvalidStructureException(exc, cmd);
					if (Regex.IsMatch(exc.Message, "Generator \\w* already exists")) return new KRUDInvalidStructureException(exc, cmd);
					if (Regex.IsMatch(exc.Message, "Index \\w* already exists")) return new KRUDInvalidStructureException(exc, cmd);
					if (exc.Message.Contains("DEFINE TRIGGER failed")) return new KRUDInvalidStructureException(exc, cmd);
				}
			}
			return base.TranslateException(exc, cmd);
		}
	}
}
