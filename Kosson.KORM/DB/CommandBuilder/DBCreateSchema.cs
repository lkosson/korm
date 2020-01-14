using System;
using System.Data;
using System.Text;

namespace Kosson.KORM.DB.CommandBuilder
{
	/// <inheritdoc/>
	public class DBCreateSchema : DBCommand, IDBCreateSchema
	{
		/// <inheritdoc/>
		public DBCreateSchema(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		void IDBCreateSchema.Schema(IDBIdentifier schema)
		{
			if (schema == null) throw new ArgumentNullException("schema");
			this.table = schema;
		}

		/// <inheritdoc/>
		protected override void AppendCommandText(StringBuilder sb)
		{
			sb.Append("CREATE SCHEMA ");
			AppendTable(sb);
		}
	}
}
