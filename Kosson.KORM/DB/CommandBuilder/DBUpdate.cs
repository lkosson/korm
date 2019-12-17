using Kosson.KORM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.DB.CommandBuilder
{
	/// <inheritdoc/>
	public class DBUpdate : DBCommandWithWhere, IDBUpdate
	{
		/// <summary>
		/// List of SET expression of the command.
		/// </summary>
		protected List<SetInfo> sets;

		/// <inheritdoc/>
		public DBUpdate(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		private DBUpdate(DBUpdate template)
			: base(template)
		{
			sets = template.sets == null ? null : new List<SetInfo>(template.sets);
		}

		/// <inheritdoc/>
		public virtual IDBUpdate Clone()
		{
			return new DBUpdate(this);
		}

		void IDBUpdate.Table(IDBIdentifier table)
		{
			if (table == null) throw new ArgumentNullException("table");
			this.table = table;
		}

		void IDBUpdate.Set(IDBIdentifier field, IDBExpression expression)
		{
			if (expression == null) throw new ArgumentNullException("expression");
			if (field == null) throw new ArgumentNullException("field");
			if (sets == null) sets = new List<SetInfo>();
			sets.Add(new SetInfo(field, expression));
		}

		/// <inheritdoc/>
		protected override void AppendCommandText(StringBuilder sb)
		{
			sb.Append("UPDATE ");
			AppendTable(sb);
			AppendSets(sb);
			AppendWheres(sb);
		}

		/// <summary>
		/// Appends SET clause to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendSets(StringBuilder sb)
		{
			if (sets == null || sets.Count == 0) return;
			AppendCRLF(sb);
			sb.Append("SET ");
			bool first = true;
			foreach (var set in sets)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					AppendSetSeparator(sb);
				}
				AppendSet(sb, set);
			}
		}

		/// <summary>
		/// Appends SET expression to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="set">SET expression to add.</param>
		protected virtual void AppendSet(StringBuilder sb, SetInfo set)
		{
			set.Field.Append(sb);
			sb.Append(" = ");
			set.Expression.Append(sb);
		}

		/// <summary>
		/// Appends SET expression separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendSetSeparator(StringBuilder sb)
		{
			sb.Append(", ");
		}

		/// <summary>
		/// UPDATE SET definition.
		/// </summary>
		protected struct SetInfo
		{
			/// <summary>
			/// Column to set.
			/// </summary>
			public IDBIdentifier Field { get; private set; }

			/// <summary>
			/// Expression providing a value to set.
			/// </summary>
			public IDBExpression Expression { get; private set; }

			/// <summary>
			/// Creates a new UPDATE SET definition.
			/// </summary>
			/// <param name="field">Column to set.</param>
			/// <param name="expression">Expression providing a value to set.</param>
			public SetInfo(IDBIdentifier field, IDBExpression expression)
				: this()
			{
				Field = field;
				Expression = expression;
			}
		}
	}
}
