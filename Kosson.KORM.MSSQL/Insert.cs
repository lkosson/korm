using Kosson.Interfaces;
using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class Insert : DBInsert
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

		protected override void AppendHeader(StringBuilder sb)
		{
			if (primaryKeyInsert)
			{
				sb.Append("SET IDENTITY_INSERT ");
				table.Append(sb);
				sb.Append(" ON; ");
			}
			base.AppendHeader(sb);
		}

		protected override void AppendFooter(StringBuilder sb)
		{
			if (primaryKeyInsert)
			{
				sb.Append("; SET IDENTITY_INSERT ");
				table.Append(sb);
				sb.Append(" OFF");
			}
			if (primaryKeyReturn) sb.Append("; SELECT SCOPE_IDENTITY()");
		}
	}
}
