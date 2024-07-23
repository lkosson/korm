using Kosson.KORM;
using Kosson.KORM.DB.CommandBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KORM.MSSQL
{
	class Select : DBSelect
	{
		public Select(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		private Select(Select template)
			: base(template)
		{
		}

		public override IDBSelect Clone()
		{
			return new Select(this);
		}

		protected override void AppendForUpdate(StringBuilder sb)
		{
		}

		protected override void AppendColumns(StringBuilder sb)
		{
			if (limit > 0)
			{
				sb.Append("TOP ");
				sb.Append(limit);
				sb.Append(' ');
			}
			base.AppendColumns(sb);
		}

		protected override void AppendMainTable(StringBuilder sb)
		{
			base.AppendMainTable(sb);
			if (forUpdate) sb.Append(" WITH (ROWLOCK, UPDLOCK) ");
		}
	}
}
