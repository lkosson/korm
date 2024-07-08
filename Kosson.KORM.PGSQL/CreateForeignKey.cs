using Kosson.KORM.DB.CommandBuilder;
using System;
using System.Text;

namespace Kosson.KORM.PGSQL
{
	class CreateForeignKey(IDBCommandBuilder builder) : DBCreateForeignKey(builder)
	{
		protected override void AppendHeader(StringBuilder sb)
		{
			ArgumentNullException.ThrowIfNull(name);
			ArgumentNullException.ThrowIfNull(table);

			sb.Append("DO $$ BEGIN IF NOT EXISTS(SELECT 1 FROM information_schema.table_constraints WHERE table_name = '");

			if (table is IDBDottedIdentifier dottedTable)
			{
				if (dottedTable.Fragments.Length == 1)
				{
					sb.Append(dottedTable.Fragments[0]);
				}
				else
				{
					sb.Append(dottedTable.Fragments[1]);
					sb.Append("' AND table_schema = '");
					sb.Append(dottedTable.Fragments[0]);
				}
			}
			else
			{
				sb.Append(table.RawValue);
			}

			sb.Append("' AND constraint_name = '");
			sb.Append(name.RawValue);
			sb.Append("') THEN ");
			base.AppendHeader(sb);
		}

		protected override void AppendFooter(StringBuilder sb)
		{
			base.AppendFooter(sb);
			sb.AppendLine();
			sb.Append("; END IF; END; $$;");
		}
	}
}
