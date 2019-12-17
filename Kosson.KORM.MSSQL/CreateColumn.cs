using Kosson.KORM;
using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class CreateColumn : DBCreateColumn
	{
		public CreateColumn(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendHeader(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS(SELECT name FROM syscolumns WHERE name='");
			sb.Append(name.RawValue);
			sb.Append("' AND id=OBJECT_ID('");
			sb.Append(table.RawValue);
			sb.AppendLine("', 'U'))");
			base.AppendHeader(sb);
		}
	}
}
