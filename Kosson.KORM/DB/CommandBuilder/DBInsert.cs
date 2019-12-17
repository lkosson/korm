using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.DB.CommandBuilder
{
	/// <inheritdoc/>
	public class DBInsert : DBCommand, IDBInsert
	{
		/// <summary>
		/// List of column names and values.
		/// </summary>
		protected List<InsertInfo> columns;

		/// <summary>
		/// Table's primary key column name.
		/// </summary>
		protected IDBIdentifier primaryKey;

		/// <summary>
		/// Determines whether to insert provided primary key value.
		/// </summary>
		protected bool primaryKeyInsert;

		/// <summary>
		/// Determines whether to return inserted row's primary key value.
		/// </summary>
		protected bool primaryKeyReturn;

		string IDBInsert.GetLastID { get { return null; } }

		/// <inheritdoc/>
		public DBInsert(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		/// <summary>
		/// Creates a copy of provided INSERT command.
		/// </summary>
		/// <param name="template">INSERT command to create copy for.</param>
		protected DBInsert(DBInsert template)
			: base(template)
		{
			columns = template.columns == null ? null : new List<InsertInfo>(template.columns);
			primaryKey = template.primaryKey;
			primaryKeyInsert = template.primaryKeyInsert;
			primaryKeyReturn = template.primaryKeyReturn;
		}

		/// <inheritdoc/>
		public virtual IDBInsert Clone()
		{
			return new DBInsert(this);
		}

		void IDBInsert.Table(IDBIdentifier table)
		{
			if (table == null) throw new ArgumentNullException("table");
			this.table = table;
		}

		void IDBInsert.PrimaryKeyReturn(IDBIdentifier primaryKey)
		{
			if (primaryKey == null) throw new ArgumentNullException("primaryKey");
			primaryKeyReturn = true;
			this.primaryKey = primaryKey;
		}

		void IDBInsert.PrimaryKeyInsert(IDBIdentifier primaryKey, IDBExpression value)
		{
			this.primaryKey = primaryKey;
			primaryKeyInsert = true;
			((IDBInsert)this).Column(primaryKey, value);
		}

		void IDBInsert.Column(IDBIdentifier column, IDBExpression value)
		{
			if (column == null) throw new ArgumentNullException("column");
			if (value == null) throw new ArgumentNullException("value");
			if (columns == null) columns = new List<InsertInfo>();
			columns.Add(new InsertInfo(column, value));
		}

		/// <inheritdoc/>
		protected override void AppendCommandText(StringBuilder sb)
		{
			sb.Append("INSERT INTO ");
			AppendTable(sb);
			AppendColumns(sb);
			AppendValues(sb);
		}

		/// <summary>
		/// Appends column names to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendColumns(StringBuilder sb)
		{
			bool first = true;
			if (columns != null)
			{
				foreach (var column in columns)
				{
					if (first)
					{
						sb.Append(" (");
						first = false;
					}
					else
					{
						AppendColumnSeparator(sb);
					}
					AppendColumn(sb, column.Column);
				}
			}

			if (!first) sb.Append(")");
		}

		/// <summary>
		/// Appends column name to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="column">Column name to append.</param>
		protected virtual void AppendColumn(StringBuilder sb, IDBIdentifier column)
		{
			column.Append(sb);
		}

		/// <summary>
		/// Appends column names separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendColumnSeparator(StringBuilder sb)
		{
			sb.Append(", ");
		}

		/// <summary>
		/// Appends value expressions to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendValues(StringBuilder sb)
		{
			if (columns == null) return;
			AppendCRLF(sb);
			sb.Append("VALUES ");
			bool first = true;
			if (columns != null)
			{
				foreach (var column in columns)
				{
					if (first)
					{
						sb.Append("(");
						first = false;
					}
					else
					{
						AppendValueSeparator(sb);
					}
					AppendValue(sb, column.Value);
				}
			}
			if (!first) sb.Append(")");
		}

		/// <summary>
		/// Appends value expression to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="value">Value expression to append.</param>
		protected virtual void AppendValue(StringBuilder sb, IDBExpression value)
		{
			value.Append(sb);
		}

		/// <summary>
		/// Appends value expressions separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendValueSeparator(StringBuilder sb)
		{
			sb.Append(", ");
		}

		/// <summary>
		/// INSERT column definition.
		/// </summary>
		protected struct InsertInfo
		{
			/// <summary>
			/// Column name.
			/// </summary>
			public IDBIdentifier Column { get; private set; }

			/// <summary>
			/// Value to insert.
			/// </summary>
			public IDBExpression Value { get; private set; }

			/// <summary>
			/// Creates a new INSERT column definition.
			/// </summary>
			/// <param name="column">Column name.</param>
			/// <param name="value">Value to insert.</param>
			public InsertInfo(IDBIdentifier column, IDBExpression value)
				: this()
			{
				Column = column;
				Value = value;
			}
		}
	}
}
