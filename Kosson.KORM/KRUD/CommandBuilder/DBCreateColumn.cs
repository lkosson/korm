using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KRUD.CommandBuilder
{
	/// <inheritdoc/>
	public class DBCreateColumn : DBCommand, IDBCreateColumn, IDBForeignKey
	{
		/// <summary>
		/// Column name identifier.
		/// </summary>
		protected IDBIdentifier name;

		/// <summary>
		/// Column data type.
		/// </summary>
		protected IDBExpression type;

		/// <summary>
		/// Column default value.
		/// </summary>
		protected IDBExpression defaultValue;

		/// <summary>
		/// Determines whether column has NOT NULL clause.
		/// </summary>
		protected bool notNull;

		/// <summary>
		/// Name of the foreign key constraint for the column.
		/// </summary>
		protected IDBIdentifier foreignKeyName;

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
		public DBCreateColumn(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		void IDBCreateColumn.Table(IDBIdentifier table)
		{
			if (table == null) throw new ArgumentNullException("table");
			this.table = table;
		}

		void IDBCreateColumn.Name(IDBIdentifier name)
		{
			if (name == null) throw new ArgumentNullException("name");
			this.name = name;
		}

		void IDBCreateColumn.Type(IDBExpression type)
		{
			this.type = type;
		}

		void IDBCreateColumn.DefaultValue(IDBExpression defaultValue)
		{
			this.defaultValue = defaultValue;
		}

		void IDBCreateColumn.NotNull()
		{
			notNull = true;
		}

		void IDBForeignKey.ConstraintName(IDBIdentifier name)
		{
			if (name == null) throw new ArgumentNullException("name");
			this.foreignKeyName = name;
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
			sb.Append(" ADD ");
			AppendName(sb);
			sb.Append(" ");
			AppendType(sb);
			sb.Append(" ");
			AppendNotNull(sb);
			AppendDefaultValue(sb);
			AppendForeignKey(sb);
		}

		/// <summary>
		/// Appends column name to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendName(StringBuilder sb)
		{
			if (name == null) throw new ArgumentNullException("name");
			name.Append(sb);
		}

		/// <summary>
		/// Appends column data type to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendType(StringBuilder sb)
		{
			if (type == null) throw new ArgumentNullException("type");
			type.Append(sb);
		}

		/// <summary>
		/// Appends NOT NULL clause to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendNotNull(StringBuilder sb)
		{
			if (notNull) sb.Append("NOT NULL ");
		}

		/// <summary>
		/// Appends default value clause to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendDefaultValue(StringBuilder sb)
		{
			if (defaultValue == null) return;
			sb.Append("DEFAULT ");
			defaultValue.Append(sb);
		}

		/// <summary>
		/// Appends foreign key constraint to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendForeignKey(StringBuilder sb)
		{
			if (foreignKeyName == null) return;
			var fk = Builder.CreateForeignKey();
			fk.ConstraintName(foreignKeyName);
			fk.TargetTable(targetTable);
			fk.TargetColumn(targetColumn);
			if (setNull) fk.SetNull();
			if (cascade) fk.Cascade();
			fk.AppendColumnDefinition(sb);
		}
	}
}
