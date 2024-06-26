using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KORM.DB.CommandBuilder
{
	/// <inheritdoc/>
	public class DBCreateIndex : DBCommand, IDBCreateIndex
	{
		/// <summary>
		/// Index identifier.
		/// </summary>
		protected IDBIdentifier name;

		/// <summary>
		/// List of index key column identifiers.
		/// </summary>
		protected List<IDBIdentifier> columns;

		/// <summary>
		/// List of columns included in leaf nodes of covering index.
		/// </summary>
		protected List<IDBIdentifier> included;

		/// <summary>
		/// Determines whether index is unique.
		/// </summary>
		protected bool unique;

		/// <inheritdoc/>
		public DBCreateIndex(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		void IDBCreateIndex.Name(IDBIdentifier name)
		{
			ArgumentNullException.ThrowIfNull(name);
			this.name = name;
		}

		void IDBCreateIndex.Table(IDBIdentifier table)
		{
			ArgumentNullException.ThrowIfNull(table);
			this.table = table;
		}

		void IDBCreateIndex.Column(IDBIdentifier column)
		{
			ArgumentNullException.ThrowIfNull(column);
			columns ??= [];
			columns.Add(column);
		}

		void IDBCreateIndex.Include(IDBIdentifier column)
		{
			ArgumentNullException.ThrowIfNull(column);
			included ??= [];
			included.Add(column);
		}

		void IDBCreateIndex.Unique()
		{
			unique = true;
		}

		/// <inheritdoc/>
		protected override void AppendCommandText(StringBuilder sb)
		{

			sb.Append("CREATE ");
			if (unique) sb.Append("UNIQUE ");
			sb.Append("INDEX ");
			AppendName(sb);
			sb.Append(" ON ");
			AppendTable(sb);
			sb.Append('(');
			AppendColumns(sb);
			sb.Append(')');
		}

		/// <summary>
		/// Appends index name to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendName(StringBuilder sb)
		{
			ArgumentNullException.ThrowIfNull(name);
			name.Append(sb);
		}

		/// <summary>
		/// Appends index key and leaf column identifiers to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendColumns(StringBuilder sb)
		{
			if (!columns.Any()) throw new ArgumentOutOfRangeException("columns", "column list is empty");
			bool first = true;
			foreach (var column in columns)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					sb.Append(", ");
				}
				AppendColumn(sb, column);
			}

			if (included != null)
			{
				AppendIncludedColumnPrefix(sb);

				first = true;
				foreach (var column in included)
				{
					if (first)
					{
						first = false;
					}
					else
					{
						sb.Append(", ");
					}
					AppendColumn(sb, column);
				}
			}
		}

		/// <summary>
		/// Appends keyword separating index key columns from covering index columns to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendIncludedColumnPrefix(StringBuilder sb)
		{
			sb.Append(", ");
		}

		/// <summary>
		/// Appends index column key identifier to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="column">Column identifier to append.</param>
		protected virtual void AppendColumn(StringBuilder sb, IDBIdentifier column)
		{
			ArgumentNullException.ThrowIfNull(column);
			column.Append(sb);
		}
	}
}
