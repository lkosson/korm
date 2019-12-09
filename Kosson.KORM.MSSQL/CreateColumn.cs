using Kosson.Interfaces;
using Kosson.KRUD.CommandBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD.MSSQL
{
	class CreateColumn : DBCreateColumn
	{
		public CreateColumn(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendHeader(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS(SELECT name FROM syscolumns WHERE name='");
			sb.Append(name.RawValue);
			sb.Append("' AND id=OBJECT_ID('");
			sb.Append(table.RawValue);
			sb.AppendLine("', 'U'))");
			base.AppendHeader(sb);
		}
	}
}
