using Kosson.KORM;
using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class CreateColumn : DBCreateColumn
	{
		public CreateColumn(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendHeader(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS(SELECT name FROM sys.columns WHERE name='");
			sb.Append(name.RawValue);
			sb.Append("' AND object_id = (SELECT object_id FROM sys.tables WHERE name='");
			if (table is IDBDottedIdentifier dottedTable)
			{
				if (dottedTable.Fragments.Length == 1)
				{
					sb.Append(dottedTable.Fragments[0]);
					sb.Append("'");
				}
				else
				{
					sb.Append(dottedTable.Fragments[1]);
					sb.Append("' AND schema_id = (SELECT schema_id FROM sys.schemas WHERE name = '");
					sb.Append(dottedTable.Fragments[0]);
					sb.Append("')");
				}
			}
			else
			{
				sb.Append(table.RawValue);
				sb.Append("'");
			}
			sb.AppendLine("))");
			base.AppendHeader(sb);
		}
	}
}
