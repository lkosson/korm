using Kosson.KORM;
using System;
using System.Data;

namespace Kosson.KORM.DB.CommandBuilder
{
	/// <inheritdoc/>
	public class DBCommandBuilder : IDBCommandBuilder
	{
		/// <inheritdoc/>
		public virtual string ParameterPrefix { get { return "@"; } }

		/// <inheritdoc/>
		public virtual string IdentifierQuoteLeft { get { return "\""; } }

		/// <inheritdoc/>
		public virtual string IdentifierQuoteRight { get { return "\""; } }

		/// <inheritdoc/>
		public virtual string IdentifierSeparator { get { return "."; } }

		/// <inheritdoc/>
		public virtual string StringQuoteLeft { get { return "'"; } }

		/// <inheritdoc/>
		public virtual string StringQuoteRight { get { return "'"; } }

		/// <inheritdoc/>
		public virtual string CommentDelimiterLeft { get { return "/*"; } }

		/// <inheritdoc/>
		public virtual string CommentDelimiterRight { get { return "*/"; } }

		/// <inheritdoc/>
		public virtual string ArrayElementSeparator { get { return ","; } }

		/// <inheritdoc/>
		public virtual bool SupportsPrimaryKeyInsert { get { return true; } }

		/// <inheritdoc/>
		public virtual IDBSelect Select()
		{
			return new DBSelect(this);
		}

		/// <inheritdoc/>
		public virtual IDBUpdate Update()
		{
			return new DBUpdate(this);
		}

		/// <inheritdoc/>
		public virtual IDBDelete Delete()
		{
			return new DBDelete(this);
		}

		/// <inheritdoc/>
		public virtual IDBInsert Insert()
		{
			return new DBInsert(this);
		}

		/// <inheritdoc/>
		public virtual IDBCreateTable CreateTable()
		{
			return new DBCreateTable(this);
		}

		/// <inheritdoc/>
		public virtual IDBCreateColumn CreateColumn()
		{
			return new DBCreateColumn(this);
		}

		/// <inheritdoc/>
		public virtual IDBCreateForeignKey CreateForeignKey()
		{
			return new DBCreateForeignKey(this);
		}

		/// <inheritdoc/>
		public virtual IDBCreateIndex CreateIndex()
		{
			return new DBCreateIndex(this);
		}

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
		public virtual IDBExpression Null()
		{
			return new DBExpression("NULL");
		}

		/// <inheritdoc/>
		public virtual IDBComment Comment(string value)
		{
			return new DBComment(this, value);
		}

		/// <inheritdoc/>
		public virtual IDBExpression Const(long value)
		{
			return new DBExpression(value.ToString());
		}

		/// <inheritdoc/>
		public virtual IDBExpression Const(string value)
		{
			return new DBStringConst(this, value);
		}

		/// <inheritdoc/>
		public virtual IDBExpression Const(double value)
		{
			return new DBExpression(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}

		/// <inheritdoc/>
		public virtual IDBExpression Const(decimal value)
		{
			return new DBExpression(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}

		/// <inheritdoc/>
		public virtual IDBExpression Const(DateTime value)
		{
			return new DBStringConst(this, value.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
		}

		/// <inheritdoc/>
		public virtual IDBExpression Const(byte[] value)
		{
			return new DBBlobConst(value);
		}

		/// <inheritdoc/>
		public virtual IDBIdentifier Identifier(string name)
		{
			return new DBIdentifier(this, name);
		}

		/// <inheritdoc/>
		public virtual IDBIdentifier Identifier(params string[] names)
		{
			return new DBDottedIdentifier(this, names);
		}

		/// <inheritdoc/>
		public virtual IDBExpression Expression(string expression)
		{
			return new DBExpression(expression);
		}

		/// <inheritdoc/>
		public virtual IDBExpression Comparison(IDBExpression lexpr, DBExpressionComparison comparison, IDBExpression rexpr)
		{
			return new DBComparison(this, lexpr, comparison, rexpr);
		}

		/// <inheritdoc/>
		public virtual IDBExpression Parameter(string name)
		{
			return new DBParameter(this, name);
		}

		/// <inheritdoc/>
		public virtual IDBExpression Array(IDBExpression[] values)
		{
			return new DBArray(this, values);
		}
	}
}
