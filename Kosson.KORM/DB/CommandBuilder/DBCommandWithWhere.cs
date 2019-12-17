using Kosson.KORM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.DB.CommandBuilder
{
	/// <summary>
	/// Base class for ORM database commands with WHERE clause.
	/// </summary>
	public abstract class DBCommandWithWhere : DBCommand
	{
		/// <summary>
		/// List of expressions in WHERE clause of the command. null value marks boundaries between groups joined by OR operator.
		/// </summary>
		protected List<IDBExpression> wheres;

		/// <inheritdoc/>
		protected DBCommandWithWhere(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		/// <inheritdoc/>
		protected DBCommandWithWhere(DBCommandWithWhere template)
			: base(template)
		{
			wheres = template.wheres == null ? null : new List<IDBExpression>(template.wheres);
		}

		/// <inheritdoc/>
		public void Where(IDBExpression expression)
		{
			if (expression == null) throw new ArgumentNullException("expression");
			if (wheres == null) wheres = new List<IDBExpression>();
			wheres.Add(expression);
		}

		/// <inheritdoc/>
		public void StartWhereGroup()
		{
			// ORing empty WHERE clause list is no-op.
			if (wheres == null) return;
			wheres.Add(null);
		}

		/// <summary>
		/// Appends WHERE clause to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendWheres(StringBuilder sb)
		{
			if (wheres == null || wheres.Count == 0) return;
			AppendCRLF(sb);
			sb.Append("WHERE (");
			bool first = true;
			bool newGroup = false;
			foreach (var where in wheres)
			{
				if (where == null)
				{
					newGroup = true;
					continue;
				}

				if (newGroup)
				{
					if (!first) AppendWhereGroupSeparator(sb);
					newGroup = false;
					first = true;
				}

				if (first)
				{
					first = false;
				}
				else
				{
					AppendWhereSeparator(sb);
				}
				AppendWhere(sb, where);
				AppendCRLF(sb);
			}
			sb.Append(")");
		}

		/// <summary>
		/// Appends given WHERE expression to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="where">Expression to append.</param>
		protected virtual void AppendWhere(StringBuilder sb, IDBExpression where)
		{
			where.Append(sb);
		}

		/// <summary>
		/// Appends WHERE clause separator used between expressions in a group to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendWhereSeparator(StringBuilder sb)
		{
			sb.Append("AND ");
		}

		/// <summary>
		/// Appends WHERE clause group separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendWhereGroupSeparator(StringBuilder sb)
		{
			sb.Append(") OR (");
		}
	}
}
