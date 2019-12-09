using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Kontext;
using Kosson.Interfaces;
using System.Data;

namespace Kosson.KRUD.Tests
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
			using (Context.Begin())
			{
				var db = Context.Current.Get<IDB>();

				CreateTable();
				CreateBasicColumns();
				CreateAllColumns();
				CreateIndices();
				CreateFK();
				Insert();
				Delete();

				//db.Commit();
			}
		}

		private void CreateTable()
		{
			var db = Context.Current.Get<IDB>();
			var cb = db.CommandBuilder;

			var ct = cb.CreateTable();
			ct.Table(cb.Identifier(TABLE));
			ct.PrimaryKey(cb.Identifier(PK));
			ct.AutoIncrement();
			var sql = ct.ToString();
			db.ExecuteNonQuery(sql);
		}

		private void CreateBasicColumns()
		{
			var db = Context.Current.Get<IDB>();
			var cb = db.CommandBuilder;

			var cc1 = cb.CreateColumn();
			cc1.Table(cb.Identifier(TABLE));
			cc1.Name(cb.Identifier(COLINT));
			cc1.Type(cb.Type(DbType.Int32));
			cc1.DefaultValue(cb.Const(DEFVAL));
			db.ExecuteNonQuery(cc1.ToString());

			var cc2 = cb.CreateColumn();
			cc2.Table(cb.Identifier(TABLE));
			cc2.Name(cb.Identifier(COLTEXT));
			cc2.Type(cb.Type(DbType.String, 100));
			db.ExecuteNonQuery(cc2.ToString());
		}

		private void CreateAllColumns()
		{
			var db = Context.Current.Get<IDB>();
			var cb = db.CommandBuilder;

			for (int i = 0; i < TypesToTest.Length; i++)
			{
				var cc = cb.CreateColumn();
				cc.Table(cb.Identifier(TABLE));
				cc.Name(cb.Identifier("Col" + i));
				cc.Type(cb.Type(TypesToTest[i], 8));
				db.ExecuteNonQuery(cc.ToString());
			}
		}

		private void CreateIndices()
		{
			var db = Context.Current.Get<IDB>();
			var cb = db.CommandBuilder;

			var ci1 = cb.CreateIndex();
			ci1.Table(cb.Identifier(TABLE));
			ci1.Name(cb.Identifier("Idx1"));
			ci1.Column(cb.Identifier(COLINT));
			ci1.Include(cb.Identifier(COLTEXT));
			db.ExecuteNonQuery(ci1.ToString());

			var ci2 = cb.CreateIndex();
			ci2.Table(cb.Identifier(TABLE));
			ci2.Name(cb.Identifier("Idx2"));
			ci2.Column(cb.Identifier(COLTEXT));
			ci2.Unique();
			db.ExecuteNonQuery(ci2.ToString());
		}

		private void CreateFK()
		{
			var db = Context.Current.Get<IDB>();
			var cb = db.CommandBuilder;

			var ct = cb.CreateTable();
			ct.Table(cb.Identifier(TABLE_FK));
			ct.PrimaryKey(cb.Identifier(PK));
			ct.AutoIncrement();
			var sql = ct.ToString();
			db.ExecuteNonQuery(sql);

			var cc = cb.CreateColumn();
			cc.Table(cb.Identifier(TABLE_FK));
			cc.Name(cb.Identifier(FK));
			cc.Type(cb.Type(DbType.Int64));
			db.ExecuteNonQuery(cc.ToString());

			var cf = cb.CreateForeignKey();
			cf.ConstraintName(cb.Identifier("FK"));
			cf.Column(cb.Identifier(FK));
			cf.Table(cb.Identifier(TABLE_FK));
			cf.TargetTable(cb.Identifier(TABLE));
			cf.TargetColumn(cb.Identifier(PK));
			cf.Cascade();
			db.ExecuteNonQuery(cf.ToString());
		}

		private void Insert()
		{
			var db = Context.Current.Get<IDB>();
			var cb = db.CommandBuilder;
			var ib = cb.Insert();

			var p0 = cb.Parameter("P0");
			ib.Table(cb.Identifier(TABLE));
			ib.PrimaryKeyReturn(cb.Identifier(PK));
			ib.Column(cb.Identifier(COLTEXT), p0);

			var sql = ib.ToString();
			using (var cmd = db.CreateCommand(sql))
			{
				db.AddParameter(cmd, "P0", "TestInsert");
				db.ExecuteNonQuery(cmd);
			}
		}

		private void Delete()
		{
			var db = Context.Current.Get<IDB>();
			var cb = db.CommandBuilder;
			var del = cb.Delete();

			del.Table(cb.Identifier(TABLE));
			del.Where(cb.Equal(cb.Identifier(PK), cb.Const(123)));

			db.ExecuteNonQuery(del.ToString());
		}
	}
}
