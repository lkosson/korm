﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Kosson.KORM.DB.CommandBuilder
{
	/// <inheritdoc/>
	public class DBSelect : DBCommandWithWhere, IDBSelect
	{
		/// <summary>
		/// FOR UPDATE query type specifier.
		/// </summary>
		protected bool forUpdate;

		/// <summary>
		/// SELECT COUNT(*) query type specifier.
		/// </summary>
		protected bool forCount;

		/// <summary>
		/// LIMIT clause value.
		/// </summary>
		protected int limit;

		/// <summary>
		/// List of column expression to select.
		/// </summary>
		protected List<DBCommandColumn>? columns;

		/// <summary>
		/// List of column subqueries to select.
		/// </summary>
		protected List<DBCommandSubquery>? subqueries;

		/// <summary>
		/// List of joined tables.
		/// </summary>
		protected List<DBCommandJoin>? joins;

		/// <summary>
		/// List of grouping expression.
		/// </summary>
		protected List<IDBExpression>? groups;

		/// <summary>
		/// List of ordering expressions.
		/// </summary>
		protected List<DBCommandOrder>? orders;

		/// <summary>
		/// Subquery expression used as main table.
		/// </summary>
		protected IDBExpression? tableSubquery;

		/// <summary>
		/// Main table alias.
		/// </summary>
		protected IDBIdentifier? alias;

		/// <summary>
		/// Indicates whether the command is a subquery part of outer SELECT command.
		/// </summary>
		protected bool forSubquery;

		private bool columnsRemoved;

		/// <inheritdoc/>
		public DBSelect(IDBCommandBuilder builder) : base(builder)
		{
		}

		/// <inheritdoc/>
		protected DBSelect(DBSelect template)
			: base(template)
		{
			forUpdate = template.forUpdate;
			forCount = template.forCount;
			limit = template.limit;
			columns = template.columns == null ? null : new List<DBCommandColumn>(template.columns);
			subqueries = template.subqueries == null ? null : new List<DBCommandSubquery>(template.subqueries);
			joins = template.joins == null ? null : new List<DBCommandJoin>(template.joins);
			groups = template.groups == null ? null : new List<IDBExpression>(template.groups);
			orders = template.orders == null ? null : new List<DBCommandOrder>(template.orders);
			tableSubquery = template.tableSubquery;
			alias = template.alias;
		}

		/// <inheritdoc/>
		public virtual IDBSelect Clone()
		{
			return new DBSelect(this);
		}

		void IDBSelect.ForUpdate()
		{
			forUpdate = true;
		}

		void IDBSelect.ForCount()
		{
			forCount = true;
		}

		void IDBSelect.Limit(int limit)
		{
			this.limit = limit;
		}

		void IDBSelect.Column(IDBExpression expression, IDBIdentifier? alias)
		{
			ArgumentNullException.ThrowIfNull(expression);
			columns ??= [];
			columns.Add(new DBCommandColumn(expression, alias));
		}

		void IDBSelect.Subquery(IDBExpression expression, IDBIdentifier? alias)
		{
			ArgumentNullException.ThrowIfNull(expression);
			subqueries ??= [];
			subqueries.Add(new DBCommandSubquery(expression, alias));
		}

		void IDBSelect.From(IDBIdentifier table, IDBIdentifier? alias)
		{
			ArgumentNullException.ThrowIfNull(table);
			this.table = table;
			this.tableSubquery = null;
			this.alias = alias;
		}

		void IDBSelect.FromSubquery(IDBExpression expression, IDBIdentifier? alias)
		{
			ArgumentNullException.ThrowIfNull(expression);
			this.table = null;
			this.tableSubquery = expression;
			this.alias = alias ?? Builder.Identifier("Q");
		}

		void IDBSelect.Join(IDBIdentifier table, IDBExpression expression, IDBIdentifier? alias, bool outer)
		{
			ArgumentNullException.ThrowIfNull(table);
			ArgumentNullException.ThrowIfNull(expression);
			joins ??= [];
			joins.Add(new DBCommandJoin(table, expression, alias, outer));
		}

		void IDBSelect.GroupBy(IDBExpression expression)
		{
			ArgumentNullException.ThrowIfNull(expression);
			groups ??= [];
			groups.Add(expression);
		}

		void IDBSelect.OrderBy(IDBExpression expression, bool descending)
		{
			ArgumentNullException.ThrowIfNull(expression);
			orders ??= [];
			orders.Add(new DBCommandOrder(expression, descending));
		}

		void IDBSelect.ForSubquery()
		{
			forSubquery = true;
		}

		void IDBSelect.RemoveColumns(Func<string, bool> predicate)
		{
			columnsRemoved = true;

			for (int i = 0; i < columns?.Count; i++)
			{
				var column = columns[i];
				if (column.Alias == null || !predicate(column.Alias.RawValue)) continue;
				columns[i] = new DBCommandColumn(column.Alias);
			}

			for (int i = 0; i < subqueries?.Count; i++)
			{
				var subquery = subqueries[i];
				if (subquery.Alias == null || !predicate(subquery.Alias.RawValue)) continue;
				subqueries[i] = new DBCommandSubquery(subquery.Alias);
			}
		}

		string IDBSelect.ToStringForLog()
		{
			var sb = new StringBuilder();
			if (columnsRemoved)
			{
				if (columns != null)
				{
					foreach (var column in columns)
					{
						if (column.IsRemoved || column.Alias == null) continue;
						if (sb.Length > 0) sb.Append(',');
						sb.Append(column.Alias.RawValue);
					}
				}

				if (subqueries != null)
				{
					foreach (var subquery in subqueries)
					{
						if (subquery.IsRemoved || subquery.Alias == null) continue;
						if (sb.Length > 0) sb.Append(',');
						sb.Append(subquery.Alias.RawValue);
					}
				}

				sb.Append('\t');
			}
			var where = BuildWhereExpression();
			where?.Append(sb);
			sb.Replace("\r\n", "\n");
			sb.Replace("\n", " ");
			return sb.ToString();
		}

		/// <inheritdoc/>
		protected override void AppendCommandText(StringBuilder sb)
		{
			sb.Append("SELECT ");
			AppendForUpdate(sb);
			AppendColumns(sb);
			AppendMainTable(sb);
			AppendJoins(sb);
			AppendWheres(sb);
			AppendGroups(sb);
			AppendOrders(sb);
		}

		/// <summary>
		/// Appends FOR UPDATE clause to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendForUpdate(StringBuilder sb)
		{
			if (forUpdate) sb.Append("FOR UPDATE ");
		}

		/// <summary>
		/// Appends COUNT(*) column to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendCountColumn(StringBuilder sb)
		{
			sb.Append("COUNT(*) AS COUNT");
		}

		/// <summary>
		/// Appends column names and aliases to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendColumns(StringBuilder sb)
		{
			if (forCount)
			{
				AppendCountColumn(sb);
				return;
			}

			bool first = true;
			string? previousTable = null;
			if (columns != null)
			{
				foreach (var column in columns)
				{
					if (column.Expression is not IDBDottedIdentifier columnIdentifier)
					{
						if (!first && !column.IsRemoved) AppendCRLF(sb);
					}
					else
					{
						var columnTable = columnIdentifier.Fragments.FirstOrDefault();
						if (previousTable != columnTable)
						{
							if (previousTable != null) AppendCRLF(sb);
							previousTable = columnTable;
						}
					}

					if (first)
					{
						first = false;
					}
					else
					{
						AppendColumnSeparator(sb);
					}
					AppendColumn(sb, column);
				}
			}

			if (subqueries != null)
			{
				foreach (var subquery in subqueries)
				{
					if (first)
					{
						first = false;
					}
					else
					{
						if (!subquery.IsRemoved) AppendCRLF(sb);
						AppendColumnSeparator(sb);
					}
					AppendSubquery(sb, subquery);
				}
			}
		}

		/// <summary>
		/// Appends column separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendColumnSeparator(StringBuilder sb)
		{
			sb.Append(", ");
		}

		/// <summary>
		/// Appends column expression and alias separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendColumnAliasKeyword(StringBuilder sb)
		{
			sb.Append(" AS ");
		}

		/// <summary>
		/// Appends column expression and alias to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="column">Column to add.</param>
		protected virtual void AppendColumn(StringBuilder sb, DBCommandColumn column)
		{
			if (column.IsRemoved)
			{
				AppendRemovedColumn(sb, column.Alias);
				return;
			}

			column.Expression.Append(sb);
			if (column.Alias != null)
			{
				AppendColumnAliasKeyword(sb);
				column.Alias.Append(sb);
			}
		}

		/// <summary>
		/// Appends subquery column to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="subquery">Subquery to add.</param>
		protected virtual void AppendSubquery(StringBuilder sb, DBCommandSubquery subquery)
		{
			if (subquery.IsRemoved)
			{
				AppendRemovedColumn(sb, subquery.Alias);
				return;
			}
			sb.Append('(');
			AppendSubqueryExpression(sb, subquery.Expression);
			sb.Append(')');
			if (subquery.Alias != null)
			{
				AppendColumnAliasKeyword(sb);
				subquery.Alias.Append(sb);
			}
		}

		/// <summary>
		/// Appends an empty column expression for a removed column.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="alias">Column alias in the resultset</param>
		protected virtual void AppendRemovedColumn(StringBuilder sb, IDBIdentifier alias)
		{
			sb.Append(Builder.Null());
			AppendColumnAliasKeyword(sb);
			alias.Append(sb);
		}

		/// <summary>
		/// Appends subquery command expression to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="expression">Subquery expression to add</param>
		protected virtual void AppendSubqueryExpression(StringBuilder sb, IDBExpression expression)
		{
			expression.Append(sb);
		}

		/// <summary>
		/// Appends main table name and alias to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendMainTable(StringBuilder sb)
		{
			AppendCRLF(sb);
			sb.Append("FROM ");
			if (table == null && tableSubquery == null) throw new ArgumentNullException("table");
			if (table != null) AppendTable(sb, table, alias);
			if (tableSubquery != null) AppendTableSubquery(sb, tableSubquery, alias);
		}

		/// <summary>
		/// Appends table name and alias to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="table">Table name to add.</param>
		/// <param name="alias">Table alias to add.</param>
		protected virtual void AppendTable(StringBuilder sb, IDBIdentifier table, IDBIdentifier? alias)
		{
			table.Append(sb);
			if (alias != null)
			{
				AppendTableAliasKeyword(sb);
				alias.Append(sb);
			}
		}

		/// <summary>
		/// Appends subquery as main table to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="expression">Subquery expression to add.</param>
		/// <param name="alias">Main table alias to add.</param>
		protected virtual void AppendTableSubquery(StringBuilder sb, IDBExpression expression, IDBIdentifier? alias)
		{
			sb.Append('(');
			expression.Append(sb);
			sb.Append(") ");
			if (alias != null)
			{
				AppendTableAliasKeyword(sb);
				alias.Append(sb);
			}
		}

		/// <summary>
		/// Appends table name and alias separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendTableAliasKeyword(StringBuilder sb)
		{
			sb.Append(" AS ");
		}

		/// <summary>
		/// Appends JOIN clauses to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendJoins(StringBuilder sb)
		{
			if (joins == null) return;
			foreach (var join in joins)
			{
				AppendCRLF(sb);
				AppendJoin(sb, join);
			}
		}

		/// <summary>
		/// Appends JOIN clause to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="join">JOIN clause to add.</param>
		protected virtual void AppendJoin(StringBuilder sb, DBCommandJoin join)
		{
			AppendJoinKeyword(sb);
			AppendTable(sb, join.Table, join.Alias);
			AppendJoinKeyword2(sb);
			join.Expression.Append(sb);
		}

		/// <summary>
		/// Appends JOIN clause operator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendJoinKeyword(StringBuilder sb)
		{
			sb.Append("LEFT OUTER JOIN ");
		}

		/// <summary>
		/// Appends JOIN clause table and condition separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendJoinKeyword2(StringBuilder sb)
		{
			sb.Append(" ON ");
		}

		/// <summary>
		/// Appends GROUP BY clause to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendGroups(StringBuilder sb)
		{
			if (groups == null || groups.Count == 0) return;
			AppendCRLF(sb);
			sb.Append("GROUP BY ");
			bool first = true;
			foreach (var group in groups)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					AppendGroupSeparator(sb);
				}
				AppendGroup(sb, group);
			}
		}

		/// <summary>
		/// Appends GROUP BY expression to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="group">GROUP BY expression.</param>
		protected virtual void AppendGroup(StringBuilder sb, IDBExpression group)
		{
			group.Append(sb);
		}

		/// <summary>
		/// Appends GROUP BY expressions separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendGroupSeparator(StringBuilder sb)
		{
			sb.Append(", ");
		}

		/// <summary>
		/// Appends ORDER BY clause to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendOrders(StringBuilder sb)
		{
			if (orders == null || orders.Count == 0) return;
			AppendCRLF(sb);
			sb.Append("ORDER BY ");
			bool first = true;
			foreach (var order in orders)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					AppendOrderSeparator(sb);
				}
				AppendOrder(sb, order);
			}
		}

		/// <summary>
		/// Appends ORDER BY expression to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		/// <param name="order">ORDER BY expression to add.</param>
		protected virtual void AppendOrder(StringBuilder sb, DBCommandOrder order)
		{
			order.Expression.Append(sb);
			if (order.Descending)
				AppendOrderDescending(sb);
			else
				AppendOrderAscending(sb);
		}

		/// <summary>
		/// Appends ascending sort keyword to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendOrderAscending(StringBuilder sb)
		{
		}

		/// <summary>
		/// Appends descending sort keywort to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendOrderDescending(StringBuilder sb)
		{
			sb.Append(" DESC");
		}

		/// <summary>
		/// Appends ORDER BY expression separator to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendOrderSeparator(StringBuilder sb)
		{
			sb.Append(", ");
		}

		/// <inheritdoc/>
		protected override void AppendCRLF(StringBuilder sb)
		{
			if (forSubquery)
			{
				sb.Append(' ');
				return;
			}
			base.AppendCRLF(sb);
		}
	}

	/// <summary>
	/// SELECT column definition.
	/// </summary>
	public readonly struct DBCommandColumn
	{
		/// <summary>
		/// Column value expression.
		/// </summary>
		public readonly IDBExpression? Expression { get; }

		/// <summary>
		/// Column alias.
		/// </summary>
		public readonly IDBIdentifier? Alias { get; }

		/// <summary>
		/// Determines whether the column has been removed using RemoveColumns.
		/// </summary>
		[MemberNotNullWhen(false, nameof(Expression))]
		[MemberNotNullWhen(true, nameof(Alias))]
		public readonly bool IsRemoved => Expression == null;

		/// <summary>
		/// Creates new SELECT column definition.
		/// </summary>
		/// <param name="expression">Column value expression.</param>
		/// <param name="alias">Column alias.</param>
		public DBCommandColumn(IDBExpression expression, IDBIdentifier? alias)
			: this()
		{
			Expression = expression;
			Alias = alias;
		}

		/// <summary>
		/// Creates new SELECT column definition for a removed column.
		/// </summary>
		/// <param name="alias">Column alias.</param>
		public DBCommandColumn(IDBIdentifier alias)
			: this()
		{
			Expression = default;
			Alias = alias;
		}
	}

	/// <summary>
	/// SELECT subquery column definition.
	/// </summary>
	public readonly struct DBCommandSubquery
	{
		/// <summary>
		/// Column subquery expression.
		/// </summary>
		public readonly IDBExpression? Expression { get; }

		/// <summary>
		/// Column alias.
		/// </summary>
		public readonly IDBIdentifier? Alias { get; }

		/// <summary>
		/// Determines whether the subquery has been removed using RemoveColumns.
		/// </summary>
		[MemberNotNullWhen(false, nameof(Expression))]
		[MemberNotNullWhen(true, nameof(Alias))]
		public readonly bool IsRemoved => Expression == null;

		/// <summary>
		/// Creates new SELECT subquery column definition.
		/// </summary>
		/// <param name="expression">Column subquery expression.</param>
		/// <param name="alias">Column alias.</param>
		public DBCommandSubquery(IDBExpression expression, IDBIdentifier? alias)
			: this()
		{
			Expression = expression;
			Alias = alias;
		}

		/// <summary>
		/// Creates new SELECT subquery column definition for a removed subquery.
		/// </summary>
		/// <param name="alias">Column alias.</param>
		public DBCommandSubquery(IDBIdentifier alias)
			: this()
		{
			Expression = default;
			Alias = alias;
		}
	}

	/// <summary>
	/// SELECT JOIN definition.
	/// </summary>
	public struct DBCommandJoin
	{
		/// <summary>
		/// Joined table name.
		/// </summary>
		public IDBIdentifier Table { get; private set; }

		/// <summary>
		/// Join condition expression.
		/// </summary>
		public IDBExpression Expression { get; private set; }

		/// <summary>
		/// Joined table alias.
		/// </summary>
		public IDBIdentifier? Alias { get; private set; }

		/// <summary>
		/// OUTER/INNER join specifier.
		/// </summary>
		public bool Outer { get; private set; }

		/// <summary>
		/// Creates new SELECT JOIN definition.
		/// </summary>
		/// <param name="table">Joined table name.</param>
		/// <param name="expression">Join condition expression.</param>
		/// <param name="alias">Joined table alias.</param>
		/// <param name="outer">OUTER/INNER join specifier.</param>
		public DBCommandJoin(IDBIdentifier table, IDBExpression expression, IDBIdentifier? alias, bool outer)
			: this()
		{
			Table = table;
			Expression = expression;
			Alias = alias;
			Outer = outer;
		}
	}

	/// <summary>
	/// SELECT ORDER BY definition.
	/// </summary>
	public struct DBCommandOrder
	{
		/// <summary>
		/// Sorting expression.
		/// </summary>
		public IDBExpression Expression { get; private set; }

		/// <summary>
		/// Descending/ascending specifier.
		/// </summary>
		public bool Descending { get; private set; }

		/// <summary>
		/// Creates new SELECT ORDER BY definition.
		/// </summary>
		/// <param name="expression">Sorting expression.</param>
		/// <param name="descending">Descending/ascending specifier.</param>
		public DBCommandOrder(IDBExpression expression, bool descending)
			: this()
		{
			Expression = expression;
			Descending = descending;
		}
	}
}
