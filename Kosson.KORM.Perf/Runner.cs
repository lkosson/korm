namespace Kosson.KORM.Perf;

class Runner(IDB db, IORM orm)
{
	public void Run()
	{
		PrepareDatabase();
		WarmUp();
		TransactedTest(EmptyInsertTest);
		TransactedTest(FilledInsertTest);
	}

	private void PrepareDatabase()
	{
		db.CreateDatabase();

		db.BeginTransaction();
		orm.CreateTables([typeof(TestTable), typeof(ReferencedTable)]);
		db.Commit();
	}

	private void WarmUp()
	{
		WarmUp<TestTable>();
		WarmUp<ReferencedTable>();
	}

	private void WarmUp<TRecord>()
		where TRecord : Record, new()
	{
		db.BeginTransaction();
		orm.Store(new TRecord());
		orm.Select<TRecord>().Execute();
		orm.Delete<TRecord>().Execute();
		db.Rollback();
	}

	private void TransactedTest(Action step)
	{
		db.BeginTransaction();
		var sw = new StatStopwatch();
		for (int i = 0; i < 1000; i++)
		{
			sw.Start();
			step();
			sw.Stop();
		}
		db.Rollback();
		Console.WriteLine("=== " + step.Method.Name + " ===");
		Console.WriteLine(sw);
		Console.WriteLine();
	}

	private void EmptyInsertTest()
	{
		orm.Store(new TestTable());
	}

	private void FilledInsertTest()
	{
		orm.Store(new TestTable { DateValue = DateTime.Now, StringValue = Environment.TickCount64.ToString() });
	}
}

[Table]
class TestTable : Record
{
	[Column(100)]
	public string? StringValue { get; set; }

	[Column]
	public DateTime DateValue { get; set; }

	[Column]
	[ForeignKey.None]
	public RecordRef<ReferencedTable> ForeignRef { get; set; }

	[Column]
	[ForeignKey.None]
	public ReferencedTable? ForeignValue { get; set; }
}

[Table]
class ReferencedTable : Record
{
	[Column]
	public int IntValue { get; set; }
}