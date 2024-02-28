using System.Runtime.CompilerServices;

namespace Kosson.KORM.Perf;

class Runner(IDB db, IORM orm)
{
	private const int REP = 100;
	private const int BLOCK = 1000;

	public void Run()
	{
		PrepareDatabase();
		WarmUp();
		EmptyInsertTest();
		FilledInsertTest();
		OneByOneInsertTest();
		BulkInsertTest();
		BeginCommitTest();
		GetAllTest();
		GetFirstTest();
		GetSingleTest();
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

	private void Report(StatStopwatch sw, [CallerMemberName] string? caller = default)
	{
		Console.WriteLine("=== " + caller + " ===");
		Console.WriteLine(sw);
		Console.WriteLine();
	}

	private void EmptyInsertTest()
	{
		db.BeginTransaction();
		var records = Enumerable.Range(0, BLOCK).Select(n => new TestTable()).ToList();
		var sw = new StatStopwatch();
		foreach (var record in records)
		{
			sw.Start();
			orm.Store(record);
			sw.Stop();
		}
		db.Rollback();
		Report(sw);
	}

	private void FilledInsertTest()
	{
		db.BeginTransaction();
		var records = Enumerable.Range(0, BLOCK).Select(n => new TestTable { DateValue = DateTime.Now, StringValue = Environment.TickCount64.ToString(), IntValue = n }).ToList();
		var sw = new StatStopwatch();
		foreach (var record in records)
		{
			sw.Start();
			orm.Store(record);
			sw.Stop();
		}
		db.Rollback();
		Report(sw);
	}

	private void OneByOneInsertTest()
	{
		db.BeginTransaction();
		var sw = new StatStopwatch();
		for (int i = 0; i < REP; i++)
		{
			var records = Enumerable.Range(0, BLOCK).Select(n => new TestTable { DateValue = DateTime.Now, StringValue = Environment.TickCount64.ToString(), IntValue = n }).ToList();
			sw.Start();
			foreach (var record in records)
			orm.Store(record);
			sw.Stop();
			orm.Delete<TestTable>().Execute();
		}
		db.Rollback();
		Report(sw);
	}

	private void BulkInsertTest()
	{
		db.BeginTransaction();
		var sw = new StatStopwatch();
		for (int i = 0; i < REP; i++)
		{
			var records = Enumerable.Range(0, BLOCK).Select(n => new TestTable { DateValue = DateTime.Now, StringValue = Environment.TickCount64.ToString(), IntValue = n }).ToList();
			sw.Start();
			orm.StoreAll(records);
			sw.Stop();
			orm.Delete<TestTable>().Execute();
		}
		db.Rollback();
		Report(sw);
	}

	private void BeginCommitTest()
	{
		var sw = new StatStopwatch();
		for (int i = 0; i < REP; i++)
		{
			sw.Start();
			db.BeginTransaction();
			db.Commit();
			sw.Stop();
		}
		Report(sw);
	}

	private void GetAllTest()
	{
		db.BeginTransaction();
		for (int i = 0; i < BLOCK; i++)
		{
			orm.Store(new TestTable { DateValue = DateTime.Now, StringValue = Environment.TickCount64.ToString(), IntValue = i });
		}
		var sw = new StatStopwatch();
		for (int i = 0; i < REP; i++)
		{
			sw.Start();
			orm.Select<TestTable>().Execute();
			sw.Stop();
		}
		db.Rollback();
		Report(sw);
	}

	private void GetFirstTest()
	{
		db.BeginTransaction();
		for (int i = 0; i < BLOCK; i++)
		{
			orm.Store(new TestTable { DateValue = DateTime.Now, StringValue = Environment.TickCount64.ToString(), IntValue = i });
		}

		var sw = new StatStopwatch();
		for (int i = 0; i < REP; i++)
		{
			sw.Start();
			orm.Select<TestTable>().ExecuteFirst();
			sw.Stop();
		}
		db.Rollback();
		Report(sw);
	}

	private void GetSingleTest()
	{
		db.BeginTransaction();
		for (int i = 0; i < BLOCK; i++)
		{
			orm.Store(new TestTable { DateValue = DateTime.Now, StringValue = Environment.TickCount64.ToString(), IntValue = i });
		}

		var firstRef = orm.Select<TestTable>().ExecuteFirst().Ref();

		var sw = new StatStopwatch();
		for (int i = 0; i < REP; i++)
		{
			sw.Start();
			orm.Select<TestTable>().ByRef(firstRef);
			sw.Stop();
		}
		db.Rollback();
		Report(sw);
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
	public int IntValue { get; set; }

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