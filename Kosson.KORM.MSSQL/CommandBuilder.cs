using Kosson.KORM;
using Kosson.KORM.DB.CommandBuilder;
using System;
using System.Data;

namespace Kosson.KORM.MSSQL
{
	class CommandBuilder : DBCommandBuilder
	{
		public override IDBSelect Select()
		{
			return new Select(this);
		}

		public override IDBInsert Insert()
		{
			return new Insert(this);
		}

		public override IDBCreateTable CreateTable()
		{
			return new CreateTable(this);
		}

		public override IDBCreateColumn CreateColumn()
		{
			return new CreateColumn(this);
		}

		public override IDBCreateForeignKey CreateForeignKey()
		{
			return new CreateForeignKey(this);
		}

		public override IDBCreateIndex CreateIndex()
		{
			return new CreateIndex(this);
		}

		public override IDBExpression Type(DbType type, int length, int precision)
		{
            if (type == DbType.AnsiString && length > 0) return Expression("VARCHAR(" + length + ")");
            if (type == DbType.AnsiString && length <= 0) return Expression("VARCHAR(MAX)");
            if (type == DbType.AnsiStringFixedLength) return Expression("CHAR(" + length + ")");
            if (type == DbType.Binary && length > 0) return Expression("VARBINARY(" + length + ")");
            if (type == DbType.Binary && length <= 0) return Expression("VARBINARY(MAX)");
			if (type == DbType.Boolean) return Expression("BIT");
			if (type == DbType.Byte) return Expression("TINYINT");
            if (type == DbType.Currency) return Expression("MONEY");
            if (type == DbType.Date) return Expression("DATE");
            if (type == DbType.DateTime) return Expression("DATETIME");
            if (type == DbType.DateTime2) return Expression("DATETIME2");
            if (type == DbType.DateTimeOffset && length > 0) return Expression("DATETIMEOFFSET(" + precision +")");
			if (type == DbType.DateTimeOffset && length <= 0) return Expression("DATETIMEOFFSET");
            if (type == DbType.Time) return Expression("TIME");
            if (type == DbType.Decimal && length > 0) return Expression("DECIMAL(" + length + ", " + precision + ")");
			if (type == DbType.Decimal && length <= 0) return Expression("DECIMAL(38, 6)");
            if (type == DbType.Double) return Expression("FLOAT(53)");
			if (type == DbType.Int16) return Expression("SMALLINT");
			if (type == DbType.Int32) return Expression("INT");
			if (type == DbType.Int64) return Expression("BIGINT");
            if (type == DbType.Guid) return Expression("UNIQUEIDENTIFIER");
            if (type == DbType.SByte) return Expression("SMALLINT");
			if (type == DbType.Single) return Expression("REAL");
            if (type == DbType.String && length > 0) return Expression("NVARCHAR(" + length + ")");
            if (type == DbType.String && length <= 0) return Expression("NVARCHAR(MAX)");
            if (type == DbType.StringFixedLength) return Expression("NCHAR(" + length + ")");
            if (type == DbType.UInt16) return Expression("INT");
            if (type == DbType.UInt32) return Expression("BIGINT");
            if (type == DbType.UInt64) return Expression("DECIMAL(20, 0)");
            if (type == DbType.Xml) return Expression("XML");
			throw new ArgumentException("Unsupported type " + type);
		}
	}
}
