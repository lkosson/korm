using Kosson.Interfaces;
using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class CreateIndex : DBCreateIndex
	{
		public CreateIndex(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendHeader(StringBuilder sb)
		{
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
