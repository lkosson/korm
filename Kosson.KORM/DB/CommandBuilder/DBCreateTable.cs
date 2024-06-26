using System;
using System.Data;
using System.Text;

namespace Kosson.KORM.DB.CommandBuilder
{
	/// <inheritdoc/>
	public class DBCreateTable : DBCommand, IDBCreateTable
	{
		/// <summary>
		/// Table's primary key column name.
		/// </summary>
		protected IDBIdentifier primaryKey;

		/// <summary>
		/// Primary key's column database type.
		/// </summary>
		protected IDBExpression type;

		/// <summary>
		/// Determines whether table's primary key values are assigned by database engine.
		/// </summary>
		protected bool autoincrement;

		/// <inheritdoc/>
		public DBCreateTable(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		void IDBCreateTable.Table(IDBIdentifier table)
		{
			ArgumentNullException.ThrowIfNull(table);
			this.table = table;
		}

		void IDBCreateTable.PrimaryKey(IDBIdentifier column, IDBExpression type)
		{
			ArgumentNullException.ThrowIfNull(column);
			this.primaryKey = column;
			this.type = type;
		}

		void IDBCreateTable.AutoIncrement()
		{
			autoincrement = true;
		}

		/// <inheritdoc/>
		protected override void AppendCommandText(StringBuilder sb)
		{

			sb.Append("CREATE TABLE ");
			AppendTable(sb);
			sb.Append(" (");
			AppendPrimaryKey(sb);
			sb.Append(" )");
		}

		/// <summary>
		/// Appends table's primary key definition to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendPrimaryKey(StringBuilder sb)
		{
			ArgumentNullException.ThrowIfNull(primaryKey);
			primaryKey.Append(sb);
			sb.Append(' ');
			type.Append(sb);
			sb.Append(" PRIMARY KEY");
			if (autoincrement) AppendAutoIncrement(sb);
		}

		/// <summary>
		/// Appends table's primary key's identity definition to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendAutoIncrement(StringBuilder sb)
		{
		}
	}
}
