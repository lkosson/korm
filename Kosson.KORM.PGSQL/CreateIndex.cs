using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.PGSQL
{
	class CreateIndex(IDBCommandBuilder builder) : DBCreateIndex(builder)
	{
		protected override void AppendName(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS");
			base.AppendName(sb);
		}
	}
}
