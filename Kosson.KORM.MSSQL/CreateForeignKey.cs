using Kosson.Interfaces;
using Kosson.KRUD.CommandBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD.MSSQL
{
	class CreateForeignKey : DBCreateForeignKey
	{
		public CreateForeignKey(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendHeader(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS(SELECT name FROM sysobjects WHERE name='");
			sb.Append(name.RawValue);
			sb.Append("' AND type='F' AND parent_obj = OBJECT_ID('");
			sb.Append(table.RawValue);
			sb.AppendLine("', 'U'))");
			base.AppendHeader(sb);
		}
	}
}
