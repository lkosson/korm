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
		/// <returns>Subquery expression.</returns>
		public abstract IDBExpression Build(string tableAlias, IMetaRecordField field, IDBCommandBuilder builder);
	}

    /// <summary>
    /// Declares a property value as a scalar result of "SELECT SqlFunction() FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
    /// </summary>
    public abstract class SingleValueAttribute : SubqueryBuilderAttribute
    {
        private string remoteTable;
        private string remoteJoinField;
        private string localJoinField;

		/// <summary>
		/// Constructs value expression for the subquery.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction</param>
		/// <returns>Database expression selecting subquery value.</returns>
        protected abstract string GetSqlExpression(IDBCommandBuilder builder);

        /// <summary>
        /// Declares a property value as a scalar result of "SELECT SqlFunction() FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
        /// </summary>
        /// <param name="remoteTable">Table to perform SqlFunction on.</param>
        /// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
        /// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
        public SingleValueAttribute(string remoteTable, string remoteJoinField, string localJoinField)
        {
            this.remoteTable = remoteTable;
            this.remoteJoinField = remoteJoinField;
            this.localJoinField = localJoinField;
        }

        /// <inheritdoc/>
        public override IDBExpression Build(string tableAlias, IMetaRecordField field, IDBCommandBuilder builder)
        {
            var select = builder.Select();
			BuildSelect(select, tableAlias, field, builder);
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
		protected virtual void BuildSelect(IDBSelect select, string tableAlias, IMetaRecordField field, IDBCommandBuilder builder)
		{
			select.Column(builder.Expression(GetSqlExpression(builder)));
			var remoteAlias = "SQ";
			select.From(builder.Identifier(remoteTable), builder.Identifier(remoteAlias));
			var localJoinExpr = builder.Identifier(tableAlias, localJoinField);
			var remoteJoinExpr = builder.Identifier(remoteAlias, remoteJoinField);
			select.Where(builder.Equal(localJoinExpr, remoteJoinExpr));
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
        protected string remoteArgField;

        /// <summary>
        /// Declares a property value as a scalar result of "SELECT SqlFunction(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
        /// </summary>
        /// <param name="remoteTable">Table to perform SqlFunction on.</param>
        /// <param name="remoteArgField">Column name of remoteTable column used as a parameter for SqlFunction.</param>
        /// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
        /// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
        public SingleValueWithArgAttribute(string remoteTable, string remoteArgField, string remoteJoinField, string localJoinField)
            : base (remoteTable, remoteJoinField, localJoinField)
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
            protected override string GetSqlExpression(IDBCommandBuilder builder)
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
		}
        
        /// <summary>
        /// Declares a property value as a scalar result of "SELECT SUM(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
        /// </summary>
        public sealed class SumAttribute : SingleValueWithArgAttribute
        {
			/// <inheritdoc/>
            protected override string GetSqlExpression(IDBCommandBuilder builder)
            {
                return "SUM(" + builder.Identifier(remoteArgField) + ")";
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
        }

        /// <summary>
        /// Declares a property value as a scalar result of "SELECT MIN(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
        /// </summary>
        public sealed class MinAttribute : SingleValueWithArgAttribute
        {
			/// <inheritdoc/>
            protected override string GetSqlExpression(IDBCommandBuilder builder)
            {
                return "MIN(" + builder.Identifier(remoteArgField) + ")";
            }

            /// <summary>
            /// Declares a property value as a scalar result of "SELECT MIN(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
            /// </summary>
            /// <param name="remoteTable">Table to perform MIN on.</param>
            /// <param name="remoteArgField">Column name of remoteTable column used as a parameter for min function.</param>
            /// <param name="remoteJoinField">Column name of remoteTable column used for equality comparison.</param>
			/// <param name="localJoinField">Column name in the table for which the property is declared used for equality comparison.</param>
            public MinAttribute(string remoteTable, string remoteArgField, string remoteJoinField, string localJoinField)
                : base(remoteTable, remoteArgField, remoteJoinField, localJoinField)
            {
            }
        }

        /// <summary>
        /// Declares a property value as a scalar result of "SELECT MAX(remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
        /// </summary>
        public sealed class MaxAttribute : SingleValueWithArgAttribute
        {
			/// <inheritdoc/>
            protected override string GetSqlExpression(IDBCommandBuilder builder)
            {
                return "MAX(" + builder.Identifier(remoteArgField) + ")";
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
        }

        /// <summary>
        /// Declares a property value as a scalar result of "SELECT AVG(remoteSumField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
        /// </summary>
        public sealed class AvgAttribute : SingleValueWithArgAttribute
        {
			/// <inheritdoc/>
            protected override string GetSqlExpression(IDBCommandBuilder builder)
            {
                return "AVG(" + builder.Identifier(remoteArgField) + ")";
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
        }

        /// <summary>
        /// Declares a property value as a scalar result of "SELECT COUNT(DISTINCT remoteArgField) FROM remoteTable WHERE remoteJoinField = localJoinField" subquery using a given table and a join condition.
        /// </summary>
        public sealed class CountDistinctAttribute : SingleValueWithArgAttribute
        {
			/// <inheritdoc/>
            protected override string GetSqlExpression(IDBCommandBuilder builder)
            {
                return "COUNT(DISTINCT " + builder.Identifier(remoteArgField) + ")";
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

			/// <inheritdoc/>
			protected override string GetSqlExpression(IDBCommandBuilder builder)
			{
				return builder.Identifier(remoteArgField).ToString();
			}

			/// <inheritdoc/>
			protected override void BuildSelect(IDBSelect select, string tableAlias, IMetaRecordField field, IDBCommandBuilder builder)
			{
				base.BuildSelect(select, tableAlias, field, builder);
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
					select.OrderBy(builder.Identifier(remoteOrderFieldName), descending);
				}
			}
		}
	}
}
