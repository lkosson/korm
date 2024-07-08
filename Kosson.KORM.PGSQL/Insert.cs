using Kosson.KORM.DB.CommandBuilder;
using System;
using System.Text;
using System.Xml.Linq;

namespace Kosson.KORM.PGSQL
{
	class Insert : DBInsert, IDBInsert
	{
		public Insert(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		private Insert(Insert template)
			: base(template)
		{
		}

		public override IDBInsert Clone()
		{
			return new Insert(this);
		}

		protected override void AppendCommandText(StringBuilder sb)
		{
			base.AppendCommandText(sb);
			if (primaryKeyReturn)
			{
				ArgumentNullException.ThrowIfNull(primaryKey);
				sb.Append(" RETURNING ");
				AppendColumn(sb, primaryKey);
			}
			if (primaryKeyInsert)
			{
				ArgumentNullException.ThrowIfNull(primaryKey);
				ArgumentNullException.ThrowIfNull(table);
				var sequence = CreateTable.SequenceForTable(Builder, table);
				sb.Append("; SELECT setval('");
				sequence.Append(sb);
				sb.Append("', (SELECT max(");
				primaryKey.Append(sb);
				sb.Append(") FROM ");
				AppendTable(sb);
				sb.Append("))");
			}
		}
	}
}
