using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kosson.KORM.Tests
{
	public static class MSSQLTests
	{
		public static void PrepareKORMServices(IServiceCollection services, string connectionString)
			=> services.AddKORMServices<KORM.MSSQL.SQLDB>(connectionString);
	}

	[TestClass]
	public class MSSQLBackupTests : BackupTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLComboTests : ComboTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLCommandBuilderTests : CommandBuilderTests 
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLCommentTests : CommentTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLCustomColumnDefinitionTests : CustomColumnDefinitionTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLCustomQueryTests : CustomQueryTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLDataReaderTests : DataReaderTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLDBTests : DBTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLDeleteTests : DeleteTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLDerivedTableTests : DerivedTableTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLFKTests : FKTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLFullPathTests : FullPathTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLIndexTests : IndexTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLInlineTests : InlineTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLInsertTests : InsertTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLManualIDTests : ManualIDTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLNotificationTests : NotificationTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLRecord8Tests : Record8Tests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLRecord16Tests : Record16Tests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLRecord32Tests : Record32Tests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLRenamedTableTests : RenamedTableTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLRowVersionTests : RowVersionTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLSchemaTests : SchemaTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLSelectLinqTests : SelectLinqTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLSelectAnonymousTests : SelectAnonymousTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLSelectTests : SelectTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));

		protected override void RetrieveByMultipleIDsImpl(int insertCount, int retrieveCount)
		{
			// Non-local block tests take too long
		}
	}

	[TestClass]
	public class MSSQLStringStorageTests : StringStorageTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLSubqueryTests : SubqueryTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLUpdateTests : UpdateTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}

	[TestClass]
	public class MSSQLValueStorageTests : ValueStorageTests
	{
		protected override bool SupportsInfinity => false;
		protected override void PrepareKORMServices(IServiceCollection services) => MSSQLTests.PrepareKORMServices(services, ConnectionString("mssql"));
	}
}
