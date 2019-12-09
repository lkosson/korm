using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KRUD.CommandBuilder
{
	/// <inheritdoc/>
	public class DBDelete : DBCommandWithWhere, IDBDelete
	{
		/// <inheritdoc/>
		public DBDelete(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		private DBDelete(DBDelete template)
			: base(template)
		{
		}

		/// <inheritdoc/>
		public virtual IDBDelete Clone()
		{
			return new DBDelete(this);
		}

		void IDBDelete.Table(IDBIdentifier table)
		{
			if (table == null) throw new ArgumentNullException("table");
			this.table = table;
		}

		/// <inheritdoc/>
		protected override void AppendCommandText(StringBuilder sb)
		{
			sb.Append("DELETE FROM ");
			AppendTable(sb);
			AppendCRLF(sb);
			AppendWheres(sb);
		}
	}
}
