using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Kosson.KORM.Tests
{
	public static class SQLiteTests
	{
		public static void PrepareKORMServices(IServiceCollection services)
			=> services.AddKORMServices<SQLite.SQLiteDB>("data source=:memory:");
	}

	[TestClass]
	public class SQLiteBackupTests : BackupTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteComboTests : ComboTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteCommandBuilderTests : CommandBuilderTests 
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteCommentTests : CommentTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	// SQLiteCustomColumnDefinitionTests/CustomColumnDefinitionTests tests missing - SQLite doesn't support proper numeric data types to detect if custom definition works.

	[TestClass]
	public class SQLiteCustomQueryTests : CustomQueryTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteDataReaderTests : DataReaderTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteDBTests : DBTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteDeleteTests : DeleteTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteDerivedTableTests : DerivedTableTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteFKTests : FKTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteFullPathTests : FullPathTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteIndexTests : IndexTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteInlineTests : InlineTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteInsertTests : InsertTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteManualIDTests : ManualIDTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteNotificationTests : NotificationTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteRecord8Tests : Record8Tests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteRecord16Tests : Record16Tests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteRecord32Tests : Record32Tests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteRenamedTableTests : RenamedTableTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);

		public override void AliasColumnDoesNotExist()
		{
			// SQLite interpretes quoted identifier as a string literal if identifier is invalid.
		}
	}

	[TestClass]
	public class SQLiteRowVersionTests : RowVersionTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteSelectLinqTests : SelectLinqTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	// SQLiteSchemaTests/SchemaTests tests missing - SQLite doesn't support schemas

	[TestClass]
	public class SQLiteSelectTests : SelectTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);

		public override Task AsyncFailedSelectDisposesReader()
		{
			// SQLite doesn't throw error on divide by zero
			return Task.CompletedTask;
		}

		public override Task FailedSelectAsyncThrowsException()
		{
			// SQLite doesn't throw error on divide by zero
			return Task.CompletedTask;
		}

		public override void FailedSelectDisposesReader()
		{
			// SQLite doesn't throw error on divide by zero
		}

		public override void FailedSelectThrowsException()
		{
			// SQLite doesn't throw error on divide by zero
		}
	}

	[TestClass]
	public class SQLiteStringStorageTests : StringStorageTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteSubqueryTests : SubqueryTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteUpdateTests : UpdateTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class SQLiteValueStorageTests : ValueStorageTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => SQLiteTests.PrepareKORMServices(services);
	}
}
