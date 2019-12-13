using Kosson.Interfaces;
using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class CreateTable : DBCreateTable
	{
		public CreateTable(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendHeader(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS(SELECT name FROM sysobjects WHERE name='");
			sb.Append(table.RawValue);
			sb.AppendLine("' AND type='U')");
			base.AppendHeader(sb);
		}

		protected override void AppendAutoIncrement(StringBuilder sb)
		{
			sb.Append(" IDENTITY(1,1)");
		}
	}
}
