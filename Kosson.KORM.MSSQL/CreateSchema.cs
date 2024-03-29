﻿using Kosson.KORM;
using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.MSSQL
{
	class CreateSchema : DBCreateSchema
	{
		public CreateSchema(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		protected override void AppendHeader(StringBuilder sb)
		{
			sb.Append("IF NOT EXISTS(SELECT name FROM sys.schemas WHERE name='");
			sb.Append(table.RawValue);
			sb.AppendLine("')");
			base.AppendHeader(sb);
		}
	}
}
