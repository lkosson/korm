using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.PGSQL
{
	class CreateColumn : DBCreateColumn
	{
		public CreateColumn(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendName(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS ");
			base.AppendName(sb);
		}
	}
}
