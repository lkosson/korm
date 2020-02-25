using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.SQLite
{
	class CreateTable : DBCreateTable
	{
		public CreateTable(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendTable(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS");
			base.AppendTable(sb);
		}
	}
}
