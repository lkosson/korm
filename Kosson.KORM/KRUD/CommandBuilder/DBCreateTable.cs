using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Kosson.KRUD.CommandBuilder
{
	/// <inheritdoc/>
	public class DBCreateTable : DBCommand, IDBCreateTable
	{
		/// <summary>
		/// Table's primary key column name.
		/// </summary>
		protected IDBIdentifier primaryKey;

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
			if (table == null) throw new ArgumentNullException("table");
			this.table = table;
		}

		void IDBCreateTable.PrimaryKey(IDBIdentifier column)
		{
			if (column == null) throw new ArgumentNullException("column");
			this.primaryKey = column;
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
			if (primaryKey == null) throw new ArgumentNullException("primaryKey");
			primaryKey.Append(sb);
			sb.Append(" ");
			var type = Builder.Type(DbType.Int64);
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
