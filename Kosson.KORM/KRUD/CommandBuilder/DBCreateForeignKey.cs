using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KRUD.CommandBuilder
{
	/// <inheritdoc/>
	public class DBCreateForeignKey : DBCommand, IDBCreateForeignKey
	{
		/// <summary>
		/// Foreign key constraint name.
		/// </summary>
		protected IDBIdentifier name;

		/// <summary>
		/// Table column identifier.
		/// </summary>
		protected IDBIdentifier column;

		/// <summary>
		/// Identifier of a table referenced by foreign key.
		/// </summary>
		protected IDBIdentifier targetTable;

		/// <summary>
		/// Identifier of a column referenced by foreign key.
		/// </summary>
		protected IDBIdentifier targetColumn;

		/// <summary>
		/// Determines whether foreign key has ON DELETE SET NULL clause.
		/// </summary>
		protected bool setNull;

		/// <summary>
		/// Determines whether foreign key has ON DELETE CASCADE clause.
		/// </summary>
		protected bool cascade;

		/// <inheritdoc/>
		public DBCreateForeignKey(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		void IDBForeignKey.ConstraintName(IDBIdentifier name)
		{
			if (name == null) throw new ArgumentNullException("name");
			this.name = name;
		}

		void IDBCreateForeignKey.Table(IDBIdentifier table)
		{
			if (table == null) throw new ArgumentNullException("table");
			this.table = table;
		}

		void IDBCreateForeignKey.Column(IDBIdentifier column)
		{
			if (column == null) throw new ArgumentNullException("column");
			this.column = column;
		}

		void IDBForeignKey.TargetTable(IDBIdentifier targetTable)
		{
			if (targetTable == null) throw new ArgumentNullException("targetTable");
			this.targetTable = targetTable;
		}

		void IDBForeignKey.TargetColumn(IDBIdentifier targetColumn)
		{
			if (targetColumn == null) throw new ArgumentNullException("targetColumn");
			this.targetColumn = targetColumn;
		}

		void IDBForeignKey.SetNull()
		{
			setNull = true;
			cascade = false;
		}

		void IDBForeignKey.Cascade()
		{
			setNull = false;
			cascade = true;
		}

		/// <inheritdoc/>
		protected override void AppendCommandText(StringBuilder sb)
		{
			sb.Append("ALTER TABLE ");
			AppendTable(sb);
			sb.Append(" ADD CONSTRAINT ");
			AppendName(sb);
			sb.Append(" FOREIGN KEY (");
			AppendColumn(sb);
			sb.Append(") REFERENCES ");
			AppendTargetTable(sb);
			sb.Append("(");
			AppendTargetColumn(sb);
			sb.Append(")");
			AppendOnDelete(sb);
		}

		/// <inheritdoc/>
		void IDBCreateForeignKey.AppendColumnDefinition(StringBuilder sb)
		{
			sb.Append("CONSTRAINT ");
			AppendName(sb);
			sb.Append(" REFERENCES ");
			AppendTargetTable(sb);
			sb.Append("(");
			AppendTargetColumn(sb);
			sb.Append(")");
			AppendOnDelete(sb);
		}

		/// <summary>
		/// Appends foreign key constraint name to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendName(StringBuilder sb)
		{
			if (name == null) throw new ArgumentNullException("name");
			name.Append(sb);
		}

		/// <summary>
		/// Appends column name to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendColumn(StringBuilder sb)
		{
			if (column== null) throw new ArgumentNullException("column");
			column.Append(sb);
		}

		/// <summary>
		/// Appends referenced table name to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendTargetTable(StringBuilder sb)
		{
			if (targetTable == null) throw new ArgumentNullException("targetTable");
			targetTable.Append(sb);
		}

		/// <summary>
		/// Appends referenced column name to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendTargetColumn(StringBuilder sb)
		{
			if (targetColumn == null) throw new ArgumentNullException("targetColumn");
			targetColumn.Append(sb);
		}

		/// <summary>
		/// Appends ON DELETE clause to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendOnDelete(StringBuilder sb)
		{
			if (cascade) sb.Append(" ON DELETE CASCADE");
			if (setNull) sb.Append(" ON DELETE SET NULL");
		}
	}
}
