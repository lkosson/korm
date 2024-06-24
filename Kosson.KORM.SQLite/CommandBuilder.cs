using Kosson.KORM.DB.CommandBuilder;
using System;
using System.Data;

namespace Kosson.KORM.SQLite
{
	class CommandBuilder : DBCommandBuilder
	{
		/// <inheritdoc/>
		public override int MaxParameterCount => 999;

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

		public override IDBSelect Select()
		{
			return new Select(this);
		}

		public override IDBInsert Insert()
		{
			return new Insert(this);
		}

		public override IDBExpression Type(DbType type, int length, int precision)
		{
			if (type == DbType.AnsiString) return Expression("TEXT");
			if (type == DbType.AnsiStringFixedLength) return Expression("TEXT");
			if (type == DbType.Binary) return Expression("BLOB");
			if (type == DbType.Boolean) return Expression("NUMERIC");
			if (type == DbType.Byte) return Expression("INTEGER");
			if (type == DbType.Currency) return Expression("NUMERIC");
			if (type == DbType.Date) return Expression("TEXT"); // NUMERIC conflicts with ISO8601 format and retrieves only year part as a Decimal
			if (type == DbType.DateTime) return Expression("TEXT");
			if (type == DbType.DateTime2) return Expression("TEXT");
			if (type == DbType.DateTimeOffset) return Expression("TEXT");
			if (type == DbType.Time) return Expression("TEXT");
			if (type == DbType.Decimal) return Expression("NUMERIC");
			if (type == DbType.Double) return Expression("REAL");
			if (type == DbType.Int16) return Expression("INTEGER");
			if (type == DbType.Int32) return Expression("INTEGER");
			if (type == DbType.Int64) return Expression("INTEGER");
			if (type == DbType.Guid) return Expression("BLOB");
			if (type == DbType.SByte) return Expression("INTEGER");
			if (type == DbType.Single) return Expression("REAL");
			if (type == DbType.String) return Expression("TEXT");
			if (type == DbType.StringFixedLength) return Expression("INTEGER");
			if (type == DbType.UInt16) return Expression("INTEGER");
			if (type == DbType.UInt32) return Expression("INTEGER");
			if (type == DbType.UInt64) return Expression("INTEGER");
			if (type == DbType.Xml) return Expression("TEXT");
			throw new ArgumentException("Unsupported type " + type);
		}
	}
}
