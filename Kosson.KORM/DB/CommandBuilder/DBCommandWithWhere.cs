using System;
using System.Collections.Generic;
using System.Linq;
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
		protected List<IDBExpression?>? wheres;

		/// <inheritdoc/>
		protected DBCommandWithWhere(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		/// <inheritdoc/>
		protected DBCommandWithWhere(DBCommandWithWhere template)
			: base(template)
		{
			wheres = template.wheres == null ? null : new List<IDBExpression?>(template.wheres);
		}

		/// <inheritdoc/>
		public void Where(IDBExpression expression)
		{
			ArgumentNullException.ThrowIfNull(expression);
			wheres ??= new List<IDBExpression?>();
			wheres.Add(expression);
		}

		/// <inheritdoc/>
		public void StartWhereGroup()
		{
			// ORing empty WHERE clause list is no-op.
			if (wheres == null) return;
			// Adding empty OR group is no-op.
			if (wheres.Last() == null) return;
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
			sb.Append("WHERE ");
			var where = BuildWhereExpression();
			where?.Append(sb);
		}

		/// <summary>
		/// Build a database expression based on WHERE clauses added to this command.
		/// </summary>
		/// <returns>Database expression from WHERE clauses. null is no clauses are added.</returns>
		protected virtual IDBExpression? BuildWhereExpression()
		{
			if (wheres == null) return null;
			var groups = new List<List<IDBExpression>>();
			var currentGroup = new List<IDBExpression>();
			groups.Add(currentGroup);
			foreach (var where in wheres)
			{
				if (where == null)
				{
					currentGroup = [];
					groups.Add(currentGroup);
				}
				else
				{
					currentGroup.Add(where);
				}
			}

			return Builder.Or(groups.Select(group => group.Count == 1 ? group.Single() : Builder.And(group.ToArray())).ToArray());
		}
	}
}
