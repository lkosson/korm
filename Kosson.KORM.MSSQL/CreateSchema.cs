using Kosson.KORM;
using Kosson.KORM.DB.CommandBuilder;
using System;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class CreateSchema(IDBCommandBuilder builder) : DBCreateSchema(builder)
	{
		protected override void AppendHeader(StringBuilder sb)
		{
			ArgumentNullException.ThrowIfNull(table);
			sb.Append("IF NOT EXISTS(SELECT name FROM sys.schemas WHERE name='");
			sb.Append(table.RawValue);
			sb.AppendLine("')");
			base.AppendHeader(sb);
		}
	}
}
