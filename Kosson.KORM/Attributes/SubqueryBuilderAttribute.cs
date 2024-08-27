using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Declares a property value as a subquery.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public abstract class SubqueryBuilderAttribute : Attribute
	{
		/// <summary>
		/// Builds expression for a property subquery.
		/// </summary>
		/// <param name="tableAlias">Alias of a table on which the subquery-based property is defined.</param>
		/// <param name="field">Property for which the subquery is defined.</param>
		/// <param name="builder">Command builder to use for expression construction.</param>
		/// <param name="metaBuilder">Metadata builder for resolving database object names.</param>
		/// <returns>Subquery expression.</returns>
		public abstract IDBExpression Build(string tableAlias, IMetaRecordField field, IDBCommandBuilder builder, IMetaBuilder metaBuilder);
	}

	/// <summary>
	/// Declares a property value as a scalar result of "SELECT SqlFunction() FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
	/// </summary>
	public abstract class SingleValueAttribute : SubqueryBuilderAttribute
	{
		private readonly Type? remoteTableType;
		private readonly string? remoteTable;
		private readonly string remoteJoinField;
		private readonly string localJoinField;

		/// <summary>
		/// Constructs value expression for the subquery.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction</param>
		/// <param name="localMeta">Metadata for the table defining the subquery.</param>
		/// <param name="remoteMeta">Metadata for the table referenced by the subquery.</param>
		/// <returns>Database expression selecting subquery value.</returns>
		protected abstract string GetSqlExpression(IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta);

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT SqlFunction() FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		/// <param name="remoteTable">Table to perform SqlFunction on.</param>
		/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
		/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
		public SingleValueAttribute(string remoteTable, string remoteJoinField, string localJoinField)
		{
			this.remoteTableType = null;
			this.remoteTable = remoteTable;
			this.remoteJoinField = remoteJoinField;
			this.localJoinField = localJoinField;
		}

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT SqlFunction() FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		/// <param name="remoteTableType">Table to perform SqlFunction on.</param>
		/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
		/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
		public SingleValueAttribute(Type remoteTableType, string remoteJoinField, string localJoinField)
		{
			this.remoteTableType = remoteTableType;
			this.remoteTable = null;
			this.remoteJoinField = remoteJoinField;
			this.localJoinField = localJoinField;
		}

		/// <inheritdoc/>
		public override IDBExpression Build(string tableAlias, IMetaRecordField field, IDBCommandBuilder builder, IMetaBuilder metaBuilder)
		{
			var select = builder.Select();
			select.ForSubquery();
			var localMeta = field.Record;
			var remoteMeta = remoteTableType == null ? null : metaBuilder.Get(remoteTableType);
			BuildSelect(select, tableAlias, field, builder, localMeta, remoteMeta);
			var selectExpr = builder.Expression(select.ToString());
			return selectExpr;
		}

		/// <summary>
		/// Builds provided SELECT expression based on referenced table.
		/// </summary>
		/// <param name="select">Expression to build.</param>
		/// <param name="tableAlias">Alias of the outer database table.</param>
		/// <param name="field">Field for which the subquery is built.</param>
		/// <param name="builder">Command builder to use for expression construction.</param>
		/// <param name="localMeta">Metadata for the table defining the subquery.</param>
		/// <param name="remoteMeta">Metadata for the table referenced by the subquery.</param>
		protected virtual void BuildSelect(IDBSelect select, string tableAlias, IMetaRecordField field, IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta)
		{
			select.Column(builder.Expression(GetSqlExpression(builder, localMeta, remoteMeta)));
			var remoteAlias = "SQ";
			select.From(builder.Identifier((remoteMeta == null ? remoteTable : remoteMeta.DBName)!), builder.Identifier(remoteAlias));
			var localJoinExpr = builder.Identifier(tableAlias, ResolveField(localMeta, localJoinField));
			var remoteJoinExpr = builder.Identifier(remoteAlias, ResolveField(remoteMeta, remoteJoinField));
			select.Where(builder.Equal(localJoinExpr, remoteJoinExpr));
		}

		internal string ResolveField(IMetaRecord? metaRecord, string field)
		{
			if (metaRecord == null) return field;
			var metaField = metaRecord.GetField(field);
			if (metaField == null) return field;
			return metaField.DBName;
		}
	}

	/// <summary>
	/// Declares a property value as a scalar result of "SELECT SqlFunction(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
	/// </summary>
	public abstract class SingleValueWithArgAttribute : SingleValueAttribute
	{
		/// <summary>
		/// Field on which the aggregation for subquery is performed.
		/// </summary>
		protected readonly string remoteArgField;

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT SqlFunction(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		/// <param name="remoteTable">Table to perform SqlFunction on.</param>
		/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for SqlFunction.</param>
		/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
		/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
		public SingleValueWithArgAttribute(string remoteTable, string remoteArgField, string remoteJoinField, string localJoinField)
			: base(remoteTable, remoteJoinField, localJoinField)
		{
			this.remoteArgField = remoteArgField;
		}

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT SqlFunction(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		/// <param name="remoteTableType">Table to perform SqlFunction on.</param>
		/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for SqlFunction.</param>
		/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
		/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
		public SingleValueWithArgAttribute(Type remoteTableType, string remoteArgField, string remoteJoinField, string localJoinField)
			: base(remoteTableType, remoteJoinField, localJoinField)
		{
			this.remoteArgField = remoteArgField;
		}
	}

	/// <summary>
	/// Declares a property value as a subquery.
	/// </summary>
	public static class Subquery
	{
		/// <summary>
		/// Declares a property value as a scalar result of "SELECT COUNT(*) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		public sealed class CountAttribute : SingleValueAttribute
		{
			/// <inheritdoc/>
			protected override string GetSqlExpression(IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta)
			{
				return "COUNT(*)";
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT COUNT(*) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTable">Table to perform COUNT on.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public CountAttribute(string remoteTable, string remoteJoinField, string localJoinField)
				: base(remoteTable, remoteJoinField, localJoinField)
			{
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT COUNT(*) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTableType">Table to perform COUNT on.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public CountAttribute(Type remoteTableType, string remoteJoinField, string localJoinField = nameof(Record.ID))
				: base(remoteTableType, remoteJoinField, localJoinField)
			{
			}
		}

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT SUM(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		public sealed class SumAttribute : SingleValueWithArgAttribute
		{
			/// <inheritdoc/>
			protected override string GetSqlExpression(IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta)
			{
				return "SUM(" + builder.Identifier(ResolveField(remoteMeta, remoteArgField)) + ")";
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT SUM(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTable">Table to perform SUM on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for sum function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public SumAttribute(string remoteTable, string remoteArgField, string remoteJoinField, string localJoinField)
				: base(remoteTable, remoteArgField, remoteJoinField, localJoinField)
			{
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT SUM(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTableType">Table to perform SUM on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for sum function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public SumAttribute(Type remoteTableType, string remoteArgField, string remoteJoinField, string localJoinField = nameof(Record.ID))
				: base(remoteTableType, remoteArgField, remoteJoinField, localJoinField)
			{
			}
		}

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT MIN(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		public sealed class MinAttribute : SingleValueWithArgAttribute
		{
			/// <inheritdoc/>
			protected override string GetSqlExpression(IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta)
			{
				return "MIN(" + builder.Identifier(ResolveField(remoteMeta, remoteArgField)) + ")";
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT MIN(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTable">Table to perform MIN on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for min function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public MinAttribute(string remoteTable, string remoteArgField, string remoteJoinField, string localJoinField = nameof(Record.ID))
				: base(remoteTable, remoteArgField, remoteJoinField, localJoinField)
			{
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT MIN(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTableType">Table to perform MIN on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for min function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public MinAttribute(Type remoteTableType, string remoteArgField, string remoteJoinField, string localJoinField = nameof(Record.ID))
				: base(remoteTableType, remoteArgField, remoteJoinField, localJoinField)
			{
			}
		}

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT MAX(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		public sealed class MaxAttribute : SingleValueWithArgAttribute
		{
			/// <inheritdoc/>
			protected override string GetSqlExpression(IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta)
			{
				return "MAX(" + builder.Identifier(ResolveField(remoteMeta, remoteArgField)) + ")";
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT MAX(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTable">Table to perform MAX on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for max function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public MaxAttribute(string remoteTable, string remoteArgField, string remoteJoinField, string localJoinField)
				: base(remoteTable, remoteArgField, remoteJoinField, localJoinField)
			{
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT MAX(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTableType">Table to perform MAX on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for max function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public MaxAttribute(Type remoteTableType, string remoteArgField, string remoteJoinField, string localJoinField = nameof(Record.ID))
				: base(remoteTableType, remoteArgField, remoteJoinField, localJoinField)
			{
			}
		}

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT AVG(remoteSumField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		public sealed class AvgAttribute : SingleValueWithArgAttribute
		{
			/// <inheritdoc/>
			protected override string GetSqlExpression(IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta)
			{
				return "AVG(" + builder.Identifier(ResolveField(remoteMeta, remoteArgField)) + ")";
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT AVG(remoteSumField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTable">Table to perform AVG on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for avg function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public AvgAttribute(string remoteTable, string remoteArgField, string remoteJoinField, string localJoinField)
				: base(remoteTable, remoteArgField, remoteJoinField, localJoinField)
			{
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT AVG(remoteSumField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTableType">Table to perform AVG on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for avg function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public AvgAttribute(Type remoteTableType, string remoteArgField, string remoteJoinField, string localJoinField = nameof(Record.ID))
				: base(remoteTableType, remoteArgField, remoteJoinField, localJoinField)
			{
			}
		}

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT COUNT(DISTINCT remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
		/// </summary>
		public sealed class CountDistinctAttribute : SingleValueWithArgAttribute
		{
			/// <inheritdoc/>
			protected override string GetSqlExpression(IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta)
			{
				return "COUNT(DISTINCT " + builder.Identifier(ResolveField(remoteMeta, remoteArgField)) + ")";
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT COUNT(DISTINCT remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTable">Table to perform COUNT DISTINCT on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for count distinct function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public CountDistinctAttribute(string remoteTable, string remoteArgField, string remoteJoinField, string localJoinField)
				: base(remoteTable, remoteArgField, remoteJoinField, localJoinField)
			{
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT COUNT(DISTINCT remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTableType">Table to perform COUNT DISTINCT on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for count distinct function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			public CountDistinctAttribute(Type remoteTableType, string remoteArgField, string remoteJoinField, string localJoinField = nameof(Record.ID))
				: base(remoteTableType, remoteArgField, remoteJoinField, localJoinField)
			{
			}
		}

		/// <summary>
		/// Declares a property value as a scalar result of "SELECT FIRST 1 remoteArgField FROM remoteTable WHERE remoteJoinField = localJoinField" subquery.
		/// </summary>
		public sealed class FirstAttribute : SingleValueWithArgAttribute
		{
			private string[] remoteOrderFields;

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT remoteArgField FROM remoteTable WHERE remoteJoinField = localJoinField ORDER BY remoteOrderFields LIMIT 1" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTable">Table to perform SELECT on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for count distinct function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			/// <param name="remoteOrderFields">Column names in remoteTable to use for sorting of the subquery result. Can contain DESC suffix to reverse sort order.</param>
			public FirstAttribute(string remoteTable, string remoteArgField, string remoteJoinField, string localJoinField, params string[] remoteOrderFields)
				: base(remoteTable, remoteArgField, remoteJoinField, localJoinField)
			{
				this.remoteOrderFields = remoteOrderFields;
			}

			/// <summary>
			/// Declares a property value as a scalar result of "SELECT remoteArgField FROM remoteTable WHERE remoteJoinField = localJoinField ORDER BY remoteOrderFields LIMIT 1" subquery using a given table and a join condition.
			/// </summary>
			/// <param name="remoteTableType">Table to perform SELECT on.</param>
			/// <param name="remoteArgField">Column name of remoteTable column used as a parameter for count distinct function.</param>
			/// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
			/// <param name="remoteOrderFields">Column names in remoteTable to use for sorting of the subquery result. Can contain DESC suffix to reverse sort order.</param>
			public FirstAttribute(Type remoteTableType, string remoteArgField, string remoteJoinField, string localJoinField, params string[] remoteOrderFields)
				: base(remoteTableType, remoteArgField, remoteJoinField, localJoinField)
			{
				this.remoteOrderFields = remoteOrderFields;
			}

			/// <inheritdoc/>
			protected override string GetSqlExpression(IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta)
			{
				return builder.Identifier(ResolveField(remoteMeta, remoteArgField)).ToString();
			}

			/// <inheritdoc/>
			protected override void BuildSelect(IDBSelect select, string tableAlias, IMetaRecordField field, IDBCommandBuilder builder, IMetaRecord localMeta, IMetaRecord? remoteMeta)
			{
				base.BuildSelect(select, tableAlias, field, builder, localMeta, remoteMeta);
				select.Limit(1);
				foreach (var remoteOrderField in remoteOrderFields)
				{
					string remoteOrderFieldName;
					bool descending = false;
					if (remoteOrderField.EndsWith(" DESC", StringComparison.CurrentCultureIgnoreCase))
					{
						remoteOrderFieldName = remoteOrderField.Substring(0, remoteOrderField.Length - 5);
						descending = true;
					}
					else
					{
						remoteOrderFieldName = remoteOrderField;
					}
					select.OrderBy(builder.Identifier(ResolveField(remoteMeta, remoteOrderFieldName)), descending);
				}
			}
		}
	}
}
