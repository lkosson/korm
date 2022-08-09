using Kosson.KORM.DB.CommandBuilder;
using System;
using System.Data;

namespace Kosson.KORM.PGSQL
{
	class CommandBuilder : DBCommandBuilder
	{
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
			if (type == DbType.AnsiString && length > 0) return Expression("VARCHAR(" + length + ")");
			if (type == DbType.AnsiString && length <= 0) return Expression("TEXT");
			if (type == DbType.AnsiStringFixedLength) return Expression("CHAR(" + length + ")");
			if (type == DbType.Binary) return Expression("BYTEA");
			if (type == DbType.Boolean) return Expression("BOOL");
			if (type == DbType.Byte) return Expression("SMALLINT");
			if (type == DbType.Currency) return Expression("MONEY");
			if (type == DbType.Date) return Expression("DATE");
			if (type == DbType.DateTime) return Expression("TIMESTAMP");
			if (type == DbType.DateTime2) return Expression("TIMESTAMP");
			if (type == DbType.DateTimeOffset) return Expression("TIMESTAMP WITH TIME ZONE");
			if (type == DbType.Time) return Expression("TIME");
			if (type == DbType.Decimal && length > 0) return Expression("DECIMAL(" + length + ", " + precision + ")");
			if (type == DbType.Decimal && length <= 0) return Expression("DECIMAL(38, 6)");
			if (type == DbType.Double) return Expression("DOUBLE PRECISION");
			if (type == DbType.Int16) return Expression("SMALLINT");
			if (type == DbType.Int32) return Expression("INT");
			if (type == DbType.Int64) return Expression("BIGINT");
			if (type == DbType.Guid) return Expression("UUID");
			if (type == DbType.SByte) return Expression("SMALLINT");
			if (type == DbType.Single) return Expression("REAL");
			if (type == DbType.String && length > 0) return Expression("VARCHAR(" + length + ")");
			if (type == DbType.String && length <= 0) return Expression("TEXT");
			if (type == DbType.StringFixedLength) return Expression("CHAR(" + length + ")");
			if (type == DbType.UInt16) return Expression("INT");
			if (type == DbType.UInt32) return Expression("BIGINT");
			if (type == DbType.UInt64) return Expression("DECIMAL(20, 0)");
			// Insert with string parameter doesn't work for columns declared as XML
			//if (type == DbType.Xml) return Expression("XML");
			if (type == DbType.Xml) return Expression("TEXT");
			throw new ArgumentException("Unsupported type " + type);
		}

		private string TrimIdentifier(string name)
		{
			if (String.IsNullOrEmpty(name)) return name;
			if (name.Length <= 63) return name;
			return name.Substring(0, 63);
		}

		public override IDBIdentifier Identifier(string name)
		{
			name = TrimIdentifier(name);
			return base.Identifier(name);
		}

		public override IDBIdentifier Identifier(params string[] names)
		{
			for (int i = 0; i < names.Length; i++) names[i] = TrimIdentifier(names[i]);
			return base.Identifier(names);
		}
	}
}
