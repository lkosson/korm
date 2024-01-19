using System;
using System.Data;
using System.Linq;

namespace Kosson.KORM.DB.CommandBuilder
{
	/// <inheritdoc/>
	public class DBCommandBuilder : IDBCommandBuilder
	{
		/// <inheritdoc/>
		public virtual string ParameterPrefix => "@";

		/// <inheritdoc/>
		public virtual string IdentifierQuoteLeft => "\"";

		/// <inheritdoc/>
		public virtual string IdentifierQuoteRight => "\"";

		/// <inheritdoc/>
		public virtual string IdentifierSeparator => ".";

		/// <inheritdoc/>
		public virtual string StringQuoteLeft => "'";

		/// <inheritdoc/>
		public virtual string StringQuoteRight => "'";

		/// <inheritdoc/>
		public virtual string CommentDelimiterLeft => "/*";

		/// <inheritdoc/>
		public virtual string CommentDelimiterRight => "*/";

		/// <inheritdoc/>
		public virtual string ArrayElementSeparator => ",";

		/// <inheritdoc/>
		public virtual string AndConditionOperator => " AND ";

		/// <inheritdoc/>
		public virtual string OrConditionOperator => " OR ";

		/// <inheritdoc/>
		public virtual string ConditionParenthesisLeft => "(";

		/// <inheritdoc/>
		public virtual string ConditionParenthesisRight => ")";

		/// <inheritdoc/>
		public virtual bool SupportsPrimaryKeyInsert => true;

		/// <inheritdoc/>
		public virtual IDBSelect Select() => new DBSelect(this);

		/// <inheritdoc/>
		public virtual IDBUpdate Update() => new DBUpdate(this);

		/// <inheritdoc/>
		public virtual IDBDelete Delete() => new DBDelete(this);

		/// <inheritdoc/>
		public virtual IDBInsert Insert() => new DBInsert(this);

		/// <inheritdoc/>
		public virtual IDBCreateSchema CreateSchema() => new DBCreateSchema(this);

		/// <inheritdoc/>
		public virtual IDBCreateTable CreateTable() => new DBCreateTable(this);

		/// <inheritdoc/>
		public virtual IDBCreateColumn CreateColumn() => new DBCreateColumn(this);

		/// <inheritdoc/>
		public virtual IDBCreateForeignKey CreateForeignKey() => new DBCreateForeignKey(this);

		/// <inheritdoc/>
		public virtual IDBCreateIndex CreateIndex() => new DBCreateIndex(this);

		/// <inheritdoc/>
		public virtual IDBExpression Type(DbType type, int length, int precision)
		{
			if (type == DbType.AnsiString && length > 0) return Expression("VARCHAR(" + length + ")");
			if (type == DbType.AnsiString && length <= 0) return Expression("CLOB");
			if (type == DbType.AnsiStringFixedLength) return Expression("CHAR(" + length + ")");
			if (type == DbType.Binary && length > 0) return Expression("VARBINARY(" + length + ")");
			if (type == DbType.Binary && length <= 0) return Expression("BLOB");
			if (type == DbType.Boolean) return Expression("BOOLEAN");
			if (type == DbType.Byte) return Expression("SMALLINT");
			if (type == DbType.Currency) return Expression("DECIMAL(20, 4)");
			if (type == DbType.Date) return Expression("DATE");
			if (type == DbType.DateTime) return Expression("TIMESTAMP");
			if (type == DbType.DateTime2) return Expression("TIMESTAMP");
			if (type == DbType.DateTimeOffset) return Expression("TIMESTAMP WITH TIMEZONE");
			if (type == DbType.Time) return Expression("TIME");
			if (type == DbType.Decimal && length > 0) return Expression("DECIMAL(" + length + ", " + precision + ")");
			if (type == DbType.Decimal && length <= 0) return Expression("DECIMAL(38, 6)");
			if (type == DbType.Double) return Expression("DOUBLE PRECISION");
			if (type == DbType.Int16) return Expression("SMALLINT");
			if (type == DbType.Int32) return Expression("INTEGER");
			if (type == DbType.Int64) return Expression("BIGINT");
			if (type == DbType.Guid) return Expression("VARCHAR(36)");
			if (type == DbType.SByte) return Expression("SMALLINT");
			if (type == DbType.Single) return Expression("REAL");
			if (type == DbType.String && length > 0) return Expression("NVARCHAR(" + length + ")");
			if (type == DbType.String && length <= 0) return Expression("NCLOB");
			if (type == DbType.StringFixedLength) return Expression("NCHAR(" + length + ")");
			if (type == DbType.UInt16) return Expression("INTEGER");
			if (type == DbType.UInt32) return Expression("BIGINT");
			if (type == DbType.UInt64) return Expression("DECIMAL(20, 0)");
			if (type == DbType.Xml) return Expression("XML");
			throw new ArgumentException("Unsupported type " + type);
		}

		/// <inheritdoc/>
		public virtual IDBExpression Null() => new DBRawExpression(this, "NULL");

		/// <inheritdoc/>
		public virtual IDBComment Comment(string value) => new DBComment(this, value);

		/// <inheritdoc/>
		public virtual IDBExpression Const(long value) => new DBRawExpression(this, value.ToString());

		/// <inheritdoc/>
		public virtual IDBExpression Const(string value) => new DBStringConst(this, value);

		/// <inheritdoc/>
		public virtual IDBExpression Const(double value) => new DBRawExpression(this, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <inheritdoc/>
		public virtual IDBExpression Const(decimal value) => new DBRawExpression(this, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <inheritdoc/>
		public virtual IDBExpression Const(DateTime value) => new DBStringConst(this, value.ToString("yyyy-MM-dd HH:mm:ss.ffff"));

		/// <inheritdoc/>
		public virtual IDBExpression Const(byte[] value) => new DBBlobConst(value);

		/// <inheritdoc/>
		public virtual IDBExpression Const(bool value) => value ? Expression("1=1") : Expression("0=1");

		/// <inheritdoc/>
		public virtual IDBIdentifier Identifier(string name) => new DBIdentifier(this, name);

		/// <inheritdoc/>
		public virtual IDBIdentifier Identifier(params string[] names) => new DBDottedIdentifier(this, names);

		/// <inheritdoc/>
		public virtual IDBExpression Expression(string expression) => new DBRawExpression(this, expression);

		/// <inheritdoc/>
		public virtual IDBExpression Comparison(IDBExpression lexpr, DBExpressionComparison comparison, IDBExpression rexpr) => new DBComparison(this, lexpr, comparison, rexpr);

		/// <inheritdoc/>
		public virtual IDBExpression UnaryExpression(IDBExpression expr, DBUnaryOperator unaryOperator) => new DBUnaryExpression(this, expr, unaryOperator);

		/// <inheritdoc/>
		public virtual IDBExpression BinaryExpression(IDBExpression lexpr, DBBinaryOperator binOperator, IDBExpression rexpr) => new DBBinaryExpression(this, lexpr, binOperator, rexpr);

		/// <inheritdoc/>
		public virtual IDBExpression Parameter(string name) => new DBParameter(this, name);

		/// <inheritdoc/>
		public virtual IDBExpression Array(IDBExpression[] values) => new DBArray(this, values);

		/// <inheritdoc/>
		public virtual IDBExpression And(IDBExpression[] conditions) => conditions.Length == 0 ? Const(true) : conditions.Length == 1 ? conditions.Single() : new DBCompoundCondition(this, AndConditionOperator, conditions);

		/// <inheritdoc/>
		public virtual IDBExpression Or(IDBExpression[] conditions) => conditions.Length == 0 ? Const(false) : conditions.Length == 1 ? conditions.Single() : new DBCompoundCondition(this, OrConditionOperator, conditions);
	}
}
