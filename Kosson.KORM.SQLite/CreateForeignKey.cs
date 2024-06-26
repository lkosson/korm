using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.SQLite
{
	class CreateForeignKey(IDBCommandBuilder builder) : DBCreateForeignKey(builder)
	{
		protected override void AppendCommandText(StringBuilder sb)
		{
			// SQLite does not support creating foreign keys for existing columns.
			sb.Append("SELECT 1");
			return;
		}
	}
}
