using Kosson.KORM;
using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class CreateForeignKey : DBCreateForeignKey
	{
		public CreateForeignKey(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendHeader(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS(SELECT name FROM sysobjects WHERE name='");
			sb.Append(name.RawValue);
			sb.Append("' AND type='F' AND parent_obj = OBJECT_ID('");
			sb.Append(table.RawValue);
			sb.AppendLine("', 'U'))");
			base.AppendHeader(sb);
		}
	}
}
