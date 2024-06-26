using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.PGSQL
{
	class CreateColumn(IDBCommandBuilder builder) : DBCreateColumn(builder)
	{
		protected override void AppendName(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS ");
			base.AppendName(sb);
		}
	}
}
