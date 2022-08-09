using Kosson.KORM.DB.CommandBuilder;
using System.Linq;
using System.Text;

namespace Kosson.KORM.PGSQL
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
			if (limit > 0) sb.Append(" LIMIT " + limit);
			if (forUpdate) sb.Append(" FOR UPDATE NOWAIT");
		}
	}
}
