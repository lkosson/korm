﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Kosson.KORM.Tests
{
	public static class PGSQLTests
	{
		public static void PrepareKORMServices(IServiceCollection services)
			=> services.AddKORMServices<KORM.PGSQL.PGSQLDB>("host=localhost;database=korm;username=korm;password=korm");
	}

	[TestClass]
	public class PGSQLBackupTests : BackupTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLComboTests : ComboTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLCommandBuilderTests : CommandBuilderTests 
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLCommentTests : CommentTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLCustomColumnDefinitionTests : CustomColumnDefinitionTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLCustomQueryTests : CustomQueryTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLDataReaderTests : DataReaderTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLDBTests : DBTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLDeleteTests : DeleteTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLDerivedTableTests : DerivedTableTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLFKTests : FKTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLFullPathTests : FullPathTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLIndexTests : IndexTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLInlineTests : InlineTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLInsertTests : InsertTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLManualIDTests : ManualIDTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLNotificationTests : NotificationTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLRecord8Tests : Record8Tests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLRecord16Tests : Record16Tests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLRecord32Tests : Record32Tests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLRenamedTableTests : RenamedTableTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLRowVersionTests : RowVersionTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLSchemaTests : SchemaTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLSelectTests : SelectTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);

		public override void RetrieveByFormattedCondition()
		{
			ORM.Delete<MainTestTable>().Execute();
			var other = new MainTestTable();
			other.Value = INTMARKER;
			ORM.Insert(other);
			var inserted = new MainTestTable();
			inserted.Value = INTMARKER + 1;
			ORM.Insert(inserted);
			// PGSQL needs quotes
			var retrieved = ORM.Select<MainTestTable>().Where($"\"mtt_Value\" = {INTMARKER}").Execute();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(1, retrieved.Count);
		}

		public override Task AsyncFailedSelectDisposesReader()
		{
			// PostgreSQL aborts transaction on error
			return Task.CompletedTask;
		}

		public override void FailedSelectDisposesReader()
		{
			// PostgreSQL aborts transaction on error
		}

	}

	[TestClass]
	public class PGSQLStringStorageTests : StringStorageTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLSubqueryTests : SubqueryTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLUpdateTests : UpdateTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}

	[TestClass]
	public class PGSQLValueStorageTests : ValueStorageTests
	{
		protected override void PrepareKORMServices(IServiceCollection services) => PGSQLTests.PrepareKORMServices(services);
	}
}
