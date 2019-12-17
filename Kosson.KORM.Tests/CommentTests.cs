using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;

namespace Kosson.KORM.Tests
{
	public abstract class CommentTests : ORMTestsBase
	{
		protected override System.Collections.Generic.IEnumerable<Type> Tables()
		{
			yield return typeof(MainTestTable);
		}

		[TestMethod]
		public void CommentIsCreated()
		{
			var cb = DB.CommandBuilder;
			var comment = cb.Comment(STRINGMARKER);
			Assert.IsNotNull(comment);
			Assert.IsTrue(comment.ToString().Contains(STRINGMARKER));
		}

		[TestMethod]
		public void CommentIsIncludedInDBInsert()
		{
			var meta = MetaBuilder.Get(typeof(MainTestTable));
			var cb = DB.CommandBuilder;
			var insert = cb.Insert();
			insert.Table(cb.Identifier(meta.DBName));
			insert.Column(cb.Identifier(meta.GetField("Value").DBName), cb.Const(INTMARKER));
			insert.Tag(cb.Comment(STRINGMARKER));
			Assert.IsTrue(insert.ToString().Contains(STRINGMARKER));
		}

		[TestMethod]
		public void CommentIsIncludedInDBUpdate()
		{
			var meta = MetaBuilder.Get(typeof(MainTestTable));
			var cb = DB.CommandBuilder;
			var update = cb.Update();
			update.Table(cb.Identifier(meta.DBName));
			update.Set(cb.Identifier(meta.GetField("Value").DBName), cb.Const(INTMARKER));
			update.Tag(cb.Comment(STRINGMARKER));
			Assert.IsTrue(update.ToString().Contains(STRINGMARKER));
		}

		[TestMethod]
		public void CommentIsIncludedInDBDelete()
		{
			var meta = MetaBuilder.Get(typeof(MainTestTable));
			var cb = DB.CommandBuilder;
			var delete = cb.Delete();
			delete.Table(cb.Identifier(meta.DBName));
			delete.Tag(cb.Comment(STRINGMARKER));
			Assert.IsTrue(delete.ToString().Contains(STRINGMARKER));
		}

		[TestMethod]
		public void CommentIsIncludedInDBSelect()
		{
			var meta = MetaBuilder.Get(typeof(MainTestTable));
			var cb = DB.CommandBuilder;
			var select = cb.Select();
			select.From(cb.Identifier(meta.DBName));
			select.Column(cb.Const(1));
			select.Tag(cb.Comment(STRINGMARKER));
			Assert.IsTrue(select.ToString().Contains(STRINGMARKER));
		}

		[TestMethod]
		public void CommentWorksWithInsert()
		{
			var record = new MainTestTable { Value = INTMARKER };
			var insert = ORM.Insert<MainTestTable>().Tag(STRINGMARKER);
			Assert.IsTrue(insert.ToString().Contains(STRINGMARKER));

			insert.Records(new[] { record });
			var retrieved = ORM.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER).ExecuteFirst();
			Assert.IsNotNull(retrieved);
		}

		[TestMethod]
		public void CommentWorksWithUpdate()
		{
			var record = new MainTestTable { Value = INTMARKER };
			ORM.Insert(record);

			var update = ORM.Update<MainTestTable>().Tag(STRINGMARKER).Set("Value", INTMARKER + 1).WhereFieldEquals("Value", INTMARKER);
			Assert.IsTrue(update.ToString().Contains(STRINGMARKER));

			update.Execute();
			var retrieved = ORM.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER + 1).ExecuteFirst();
			Assert.IsNotNull(retrieved);
		}

		[TestMethod]
		public void CommentWorksWithDelete()
		{
			var record = new MainTestTable { Value = INTMARKER };
			ORM.Insert(record);

			var delete = ORM.Delete<MainTestTable>().Tag(STRINGMARKER).WhereFieldEquals("Value", INTMARKER);
			Assert.IsTrue(delete.ToString().Contains(STRINGMARKER));

			delete.Execute();
			var retrieved = ORM.Select<MainTestTable>().WhereFieldEquals("Value", INTMARKER + 1).ExecuteFirst();
			Assert.IsNull(retrieved);
		}

		[TestMethod]
		public void CommentWorksWithSelect()
		{
			var record = new MainTestTable { Value = INTMARKER };
			ORM.Insert(record);

			var select = ORM.Select<MainTestTable>().Tag(STRINGMARKER).WhereFieldEquals("Value", INTMARKER);
			Assert.IsTrue(select.ToString().Contains(STRINGMARKER));

			var retrieved = select.ExecuteFirst();
			Assert.IsNotNull(retrieved);
		}

		[TestMethod]
		public void CommentIsEscaped()
		{
			var select = ORM.Select<MainTestTable>().Tag(" */ comment\r\n'comment--'comment\"comment" + DB.CommandBuilder.CommentDelimiterRight + "comment").WhereFieldEquals("Value", INTMARKER);
			select.ExecuteFirst();
		}
	}
}
