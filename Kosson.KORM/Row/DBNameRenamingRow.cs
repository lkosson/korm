using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD
{
	class DBNameRenamingRow<T> : RenamingRow
	{
		private static IMetaRecord meta;

		public DBNameRenamingRow(IMetaRecord meta, IRow row)
			: base(row, Rename)
		{
			DBNameRenamingRow<T>.meta = meta;
		}

		private static string Rename(string name)
		{
			var field = meta.GetField(name);

			if (field != null && field.DBName != null) return field.DBName;

			return name;
		}
	}
}
