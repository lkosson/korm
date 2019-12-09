using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD.ORM
{
	class ExecuteProxyBase
	{
		protected static IReadOnlyList<IRow> Execute(string dbname, Dictionary<string, object> parameters)
		{
			var db = KORMContext.Current.DB;

			using (var cmd = db.CreateCommand(dbname))
			{
				cmd.CommandType = System.Data.CommandType.StoredProcedure;
				foreach (var parameter in parameters)
				{
					var value = parameter.Value;
					var type = value == null ? null : value.GetType();
					if (type != null && !type.GetTypeInfo().IsValueType && !type.FullName.StartsWith("System.") && !type.FullName.StartsWith("Microsoft."))
					{
						DbParameter[] ignored = null;
						DBParameterLoader.Run(db, cmd, value, ref ignored);
					}
					else
					{
						db.AddParameter(cmd, parameter.Key, value);
					}
				}
				return db.ExecuteQuery(cmd);
			}
		}

		protected static TElement WrapResultScalar<TElement>(IReadOnlyList<IRow> rows)
		{
			if (rows.Count == 0) return default(TElement);
			var value = rows[0][0];
			var converter = KORMContext.Current.Converter;
			return (TElement)converter.Convert(value, typeof(TElement));
		}

		protected static TElement WrapResultSingle<TElement>(IReadOnlyList<IRow> rows) where TElement : class, new()
		{
			if (rows.Count == 0) return default(TElement);
			var row = rows[0];
			var loader = KORMContext.Current.RecordLoader;
			var factory = KORMContext.Current.Factory;
			var converter = KORMContext.Current.Converter;
			var result = (TElement)factory.Create(typeof(TElement));
			var renamedrow = new DBNameRenamingRow<TElement>(row);
			loader.GetLoader<TElement>()(result, renamedrow, converter, factory);
			return result;
		}

		protected static TElement[] WrapResultArray<TElement>(IReadOnlyList<IRow> rows) where TElement : class, new()
		{
			return rows.Select(row => new DBNameRenamingRow<TElement>(row)).Load<TElement>().ToArray();
		}
	}
}
