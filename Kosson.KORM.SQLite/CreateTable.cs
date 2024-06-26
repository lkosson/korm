using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.SQLite
{
	class CreateTable(IDBCommandBuilder builder) : DBCreateTable(builder)
	{
		protected override void AppendTable(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS");
			base.AppendTable(sb);
		}
	}
}
