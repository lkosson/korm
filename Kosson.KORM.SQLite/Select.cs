using Kosson.KORM.DB.CommandBuilder;
using System.Linq;
using System.Text;

namespace Kosson.KORM.SQLite
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

		protected override void AppendCommandText(StringBuilder sb)
		{
			base.AppendCommandText(sb);
			if (limit > 0)
			{
				AppendCRLF(sb);
				sb.Append("LIMIT ");
				sb.Append(limit);
			}
			// PRAGMA returns a resultset - it has to be after the SELECT command.
			if (forUpdate) sb.Append("; PRAGMA locking_mode = EXCLUSIVE");
		}
	}
}
