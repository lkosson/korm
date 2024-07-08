using Kosson.KORM;
using Kosson.KORM.DB.CommandBuilder;
using System;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class CreateIndex(IDBCommandBuilder builder) : DBCreateIndex(builder)
	{
		protected override void AppendHeader(StringBuilder sb)
		{
			ArgumentNullException.ThrowIfNull(name);
			sb.Append("IF NOT EXISTS(SELECT name FROM sysindexes WHERE name='");
			sb.Append(name.RawValue);
			sb.AppendLine("')");
			base.AppendHeader(sb);
		}

		protected override void AppendIncludedColumnPrefix(StringBuilder sb)
		{
			sb.Append(") INCLUDE (");
		}
	}
}
