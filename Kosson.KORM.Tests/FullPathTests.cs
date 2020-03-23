using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using System.Data;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract class FullPathTests : TestBase
	{
		private const string TABLE = "FullPathTestTable";
		private const string TABLE_FK = "FullPathFKTestTable";
		private const string PK = "PKID";
		private const string FK = "FKID";
		private const string COLINT = "IntColumn";
		private const string COLTEXT = "TextColumn";
		private const int DEFVAL = 1234567;
		private readonly DbType[] TypesToTest = { DbType.AnsiString, DbType.AnsiStringFixedLength, DbType.Binary, DbType.Boolean, DbType.Byte, DbType.Currency, DbType.Date,
												DbType.DateTime, DbType.DateTime2, DbType.DateTimeOffset, DbType.Decimal, DbType.Double, DbType.Guid, DbType.Int16, DbType.Int32,
												DbType.Int64, DbType.Single, DbType.String, DbType.StringFixedLength, DbType.Xml };

		[TestMethod]
		public void FullPathTest()
		{
			DB.BeginTransaction();
			CreateTable();
			CreateBasicColumns();
			CreateAllColumns();
			CreateIndices();
			CreateFK();
			Insert();
			Delete();
		}

		private void CreateTable()
		{
			var cb = DB.CommandBuilder;

			var ct = cb.CreateTable();
			ct.Table(cb.Identifier(TABLE));
			ct.PrimaryKey(cb.Identifier(PK), cb.Type(DbType.Int64));
			ct.AutoIncrement();
			var sql = ct.ToString();
			DB.ExecuteNonQueryRaw(sql);
		}

		private void CreateBasicColumns()
		{
			var cb = DB.CommandBuilder;

			var cc1 = cb.CreateColumn();
			cc1.Table(cb.Identifier(TABLE));
			cc1.Name(cb.Identifier(COLINT));
			cc1.Type(cb.Type(DbType.Int32));
			cc1.DefaultValue(cb.Const(DEFVAL));
			DB.ExecuteNonQueryRaw(cc1.ToString());

			var cc2 = cb.CreateColumn();
			cc2.Table(cb.Identifier(TABLE));
			cc2.Name(cb.Identifier(COLTEXT));
			cc2.Type(cb.Type(DbType.String, 100));
			DB.ExecuteNonQueryRaw(cc2.ToString());
		}

		private void CreateAllColumns()
		{
			var cb = DB.CommandBuilder;

			for (int i = 0; i < TypesToTest.Length; i++)
			{
				var cc = cb.CreateColumn();
				cc.Table(cb.Identifier(TABLE));
				cc.Name(cb.Identifier("Col" + i));
				cc.Type(cb.Type(TypesToTest[i], 8));
				DB.ExecuteNonQueryRaw(cc.ToString());
			}
		}

		private void CreateIndices()
		{
			var cb = DB.CommandBuilder;

			var ci1 = cb.CreateIndex();
			ci1.Table(cb.Identifier(TABLE));
			ci1.Name(cb.Identifier("Idx1"));
			ci1.Column(cb.Identifier(COLINT));
			ci1.Include(cb.Identifier(COLTEXT));
			DB.ExecuteNonQueryRaw(ci1.ToString());

			var ci2 = cb.CreateIndex();
			ci2.Table(cb.Identifier(TABLE));
			ci2.Name(cb.Identifier("Idx2"));
			ci2.Column(cb.Identifier(COLTEXT));
			ci2.Unique();
			DB.ExecuteNonQueryRaw(ci2.ToString());
		}

		private void CreateFK()
		{
			var cb = DB.CommandBuilder;

			var ct = cb.CreateTable();
			ct.Table(cb.Identifier(TABLE_FK));
			ct.PrimaryKey(cb.Identifier(PK), cb.Type(DbType.Int64));
			ct.AutoIncrement();
			var sql = ct.ToString();
			DB.ExecuteNonQueryRaw(sql);

			var cc = cb.CreateColumn();
			cc.Table(cb.Identifier(TABLE_FK));
			cc.Name(cb.Identifier(FK));
			cc.Type(cb.Type(DbType.Int64));
			DB.ExecuteNonQueryRaw(cc.ToString());

			var cf = cb.CreateForeignKey();
			cf.ConstraintName(cb.Identifier("FK"));
			cf.Column(cb.Identifier(FK));
			cf.Table(cb.Identifier(TABLE_FK));
			cf.TargetTable(cb.Identifier(TABLE));
			cf.TargetColumn(cb.Identifier(PK));
			cf.Cascade();
			DB.ExecuteNonQueryRaw(cf.ToString());
		}

		private void Insert()
		{
			var cb = DB.CommandBuilder;
			var ib = cb.Insert();

			var p0 = cb.Parameter("P0");
			ib.Table(cb.Identifier(TABLE));
			ib.PrimaryKeyReturn(cb.Identifier(PK));
			ib.Column(cb.Identifier(COLTEXT), p0);

			var sql = ib.ToString();
			using (var cmd = DB.CreateCommand(sql))
			{
				DB.AddParameter(cmd, "P0", "TestInsert");
				DB.ExecuteNonQuery(cmd);
			}
		}

		private void Delete()
		{
			var cb = DB.CommandBuilder;
			var del = cb.Delete();

			del.Table(cb.Identifier(TABLE));
			del.Where(cb.Equal(cb.Identifier(PK), cb.Const(123)));

			DB.ExecuteNonQueryRaw(del.ToString());
		}
	}
}
