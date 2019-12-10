using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract class CommandBuilderTests : TestBase
	{
		[TestMethod]
		public void PropertiesAreNotNull()
		{
			var cb = DB.CommandBuilder;
			Assert.IsNotNull(cb.ParameterPrefix);
			Assert.IsNotNull(cb.IdentifierQuoteLeft);
			Assert.IsNotNull(cb.IdentifierQuoteRight);
			Assert.IsNotNull(cb.IdentifierSeparator);
			Assert.IsNotNull(cb.StringQuoteLeft);
			Assert.IsNotNull(cb.StringQuoteRight);
			Assert.IsNotNull(cb.CommentDelimiterLeft);
			Assert.IsNotNull(cb.CommentDelimiterRight);
		}

		[TestMethod]
		public void BuildersAreNotNull()
		{
			var cb = DB.CommandBuilder;
			Assert.IsNotNull(cb.Select());
			Assert.IsNotNull(cb.Update());
			Assert.IsNotNull(cb.Delete());
			Assert.IsNotNull(cb.Insert());
			Assert.IsNotNull(cb.CreateTable());
			Assert.IsNotNull(cb.CreateColumn());
			Assert.IsNotNull(cb.CreateForeignKey());
			Assert.IsNotNull(cb.CreateIndex());
		}

		[TestMethod]
		public void CloneCreatesSameType()
		{
			var cb = DB.CommandBuilder;
			Assert.IsInstanceOfType(cb.Select().Clone(), cb.Select().GetType());
			Assert.IsInstanceOfType(cb.Update().Clone(), cb.Update().GetType());
			Assert.IsInstanceOfType(cb.Delete().Clone(), cb.Delete().GetType());
			Assert.IsInstanceOfType(cb.Insert().Clone(), cb.Insert().GetType());
		}
	}
}
