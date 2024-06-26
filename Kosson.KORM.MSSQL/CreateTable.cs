using Kosson.KORM;
using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class CreateTable(IDBCommandBuilder builder) : DBCreateTable(builder)
	{
		protected override void AppendHeader(StringBuilder sb)
		{
			if (table is IDBDottedIdentifier dottedTable)
			{
				sb.Append("IF NOT EXISTS(SELECT name FROM sys.tables WHERE name='");
				if (dottedTable.Fragments.Length == 1)
				{
					sb.Append(dottedTable.Fragments[0]);
					sb.Append('\'');
				}
				else
				{
					sb.Append(dottedTable.Fragments[1]);
					sb.Append("' AND schema_id = (SELECT schema_id FROM sys.schemas WHERE name = '");
					sb.Append(dottedTable.Fragments[0]);
					sb.Append("')");
				}
				sb.Append(')');
			}
			else
			{
				sb.Append("IF NOT EXISTS(SELECT name FROM sys.tables WHERE name='");
				sb.Append(table.RawValue);
				sb.Append("')");
			}
			base.AppendHeader(sb);
		}

		protected override void AppendAutoIncrement(StringBuilder sb)
		{
			sb.Append(" IDENTITY(1,1)");
		}
	}
}
