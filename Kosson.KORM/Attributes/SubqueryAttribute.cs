﻿using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Declares a property as a subquery-based property. Its value is retrieved by executing provided subquery when perfroming SELECT. Value is ignored during UPDATE and INSERT.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class SubqueryAttribute : SubqueryBuilderAttribute
	{
		/// <summary>
		/// Subquery to execute to retrieve property value. Placeholder {0} in subquery is substituted with database alias of table in which subquery property is declared.
		/// </summary>
		public string Subquery { get; private set; }

		/// <summary>
		/// Declares a property as a subquery-based property. Its value is retrieved by executing provided subquery when perfroming SELECT. Value is ignored during UPDATE and INSERT.
		/// </summary>
		/// <param name="subquery">Subquery to execute to retrieve property value. Placeholder {0} in subquery is substituted with database alias of table in which subquery property is declared.</param>
		public SubqueryAttribute(string subquery)
		{
			Subquery = subquery;
		}

		/// <inheritdoc/>
		public override IDBExpression Build(string tableAlias, IMetaRecordField field, IDBCommandBuilder builder, IMetaBuilder metaBuilder)
		{
			return builder.Expression(String.Format(Subquery, builder.Identifier(tableAlias).ToString()));
		}
	}
}
