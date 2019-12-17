using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public class MSSQLBackupTests : BackupTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLComboTests : ComboTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLCommandBuilderTests : CommandBuilderTests 
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLCommentTests : CommentTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLCustomColumnDefinitionTests : CustomColumnDefinitionTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLCustomQueryTests : CustomQueryTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLDataReaderTests : DataReaderTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLDBTests : DBTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLDeleteTests : DeleteTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLDerivedTableTests : DerivedTableTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLFKTests : FKTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLFullPathTests : FullPathTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLIndexTests : IndexTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLInlineTests : InlineTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLInsertTests : InsertTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLManualIDTests : ManualIDTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLNotificationTests : NotificationTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLRenamedTableTests : RenamedTableTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLRowVersionTests : RowVersionTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLSelectTests : SelectTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLStringStorageTests : StringStorageTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLSubqueryTests : SubqueryTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLUpdateTests : UpdateTests
	{
		protected override string Provider { get { return "mssql"; } }
	}

	[TestClass]
	public class MSSQLValueStorageTests : ValueStorageTests
	{
		protected override string Provider { get { return "mssql"; } }
	}
}
