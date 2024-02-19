using Kosson.KORM;
using Kosson.KORM.Backup;
using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;

namespace Kosson.KORM.Scratch
{
	class Runner
	{
		private IDB db;
		private IORM orm;
		private IMetaBuilder metaBuilder;
		private IBackupProvider backupProvider;
		private IPropertyBinder propertyBinder;
		private IConverter converter;
		private IDatabaseCopier databaseCopier;
		private XMLBackup xmlBackup;
		private IDatabaseScriptGenerator databaseScripting;

		public Runner(IDatabaseCopier databaseCopier, XMLBackup xmlBackup, IDatabaseScriptGenerator databaseScripting, IDB db, IORM orm, IMetaBuilder metaBuilder, IBackupProvider backupProvider, IPropertyBinder propertyBinder, IConverter converter)
		{
			this.databaseCopier = databaseCopier;
			this.xmlBackup = xmlBackup;
			this.databaseScripting = databaseScripting;
			this.db = db;
			this.orm = orm;
			this.metaBuilder = metaBuilder;
			this.backupProvider = backupProvider;
			this.propertyBinder = propertyBinder;
			this.converter = converter;
		}

		public void Run()
		{
			db.CreateDatabase();

			db.BeginTransaction();
			orm.CreateTables(new[] { typeof(User), typeof(Role), typeof(Membership) });
			db.Commit();

			db.BeginTransaction();
			var gs = Enumerable.Range(0, 10).Select(i => new User { Name = "group" + i, UserDetails = new UserDetails { Group = i / 100 } }).ToList();
			orm.InsertAll(gs);
			var us = Enumerable.Range(0, 1000).Select(i => new User { Name = "user" + i, UserDetails = new UserDetails { Group = gs.Skip(i / 100).First() } }).ToList();
			orm.InsertAll(us);

			var roles = new[] { new Role { Name = "Admin" }, new Role { Name = "User" } };
			orm.InsertAll(roles);
			orm.InsertAll(us.Select(u => new Membership { Role = roles[u.ID % 2], User = u }));

			var selected = orm.Select<Membership>().Select(m => new { m.ID, m.User.UserDetails.Group }).Execute();
			var selectedid = orm.Select<Membership>().Select(m => m.ID).ExecuteFirst();

			var y = new[] { 1, 2 };
			var linqd = orm.Select<Membership>().Where(m => m.User.UserDetails.PasswordHash == "123").Execute();
			//var linqd = orm.Select<Membership>().Where(m => 4m != y[1] + 3L || m.User.ID == DateTime.Now.AddDays(1).Ticks || m.CreationTime == null || (m.ID > 10 && m.User.UserDetails.Group.ID == x));

			var joined = orm.Select<User>().Execute().Join(orm.Select<User>(), user => user.UserDetails.Group);

			var first = orm.Select<User>().ExecuteFirst();

			var a1 = GC.GetTotalAllocatedBytes(true);
			var sw1 = StatStopwatch.StartNew();
			for (var i = 0; i < 10000; i++)
			{
				orm.Select<User>().ByID(first.ID);
				sw1.AddMeasurement();
			}
			var a2 = GC.GetTotalAllocatedBytes(true);
			Console.WriteLine(sw1.ToString());
			Console.WriteLine(a2 - a1);

			using (var fs = new FileStream("backup.sql", FileMode.Create))
			{
				databaseScripting.GenerateScript(fs, new[] { typeof(User), typeof(Membership), typeof(Role) });
			}

			var u = new User()
			{
				ID = 123,
				Name = "admin",
				UserDetails = new UserDetails()
				{
					PasswordHash = "00-11",
					Group = gs.Skip(5).First().ID
				}
			};

			orm.InsertAll(new[] { u, u });

			//ip.Select<User>().ForUpdate().ById(10);

			var locked = orm.Select<User>().ByID(u.ID);


			u.UserDetails.Group = u.ID;

			orm.Update(u);

			var ur = new RecordRef<User>(u.ID);

			var ux = orm.Get(ur);

			//ip.Delete<User>().WhereFieldIsNull("Name").Execute();
			//ip.Delete<User>().ById(u.ID);

			orm.Update<User>().Set("Name", "admin2").ByID(u.ID);

			ux = orm.Get(ur);

			orm.Delete(ux);

			//using (var fs = new FileStream("backup.xml", FileMode.Open))
			//using (var br = xmlBackup.CreateReader(fs))
			//{
			//	backupProvider.Restore(br);
			//}

			//using (var fs = new FileStream("backup.xml", FileMode.Create))
			//using (var bw = xmlBackup.CreateWriter(fs))
			//{
			//	var bs = backupProvider.CreateBackupSet(bw);
			//	bs.AddRecords<Membership>();
			//}

			//databaseCopier.CopyTo<SQLite>(new KORMConfiguration { ConnectionString = "data source=backup.sqlite3;version=3" }, new[] { typeof(User), typeof(Role), typeof(Membership) });

			//var profiler = Context.Current.Get<IProfiler>();
			var sw = StatStopwatch.StartNew();
			//using (profiler.Start())
			{
				for (int i = 0; i < 1000; i++)
				{
					LoopCounter.Instance.Inc();
					var q = orm.Select<Membership>()
						.WhereField("ID", DBExpressionComparison.GreaterOrEqual, 100)
						.WhereField("usr_Name", DBExpressionComparison.NotEqual, "d'Arc")
						.WhereFieldIsNotNull("Role");
					//var qr = (q.ExecuteAsync().Result).Where(x => x.ID % 2 == 0).Select(x => x.User);
					var qr = q.Execute().Where(x => x.ID % 2 == 0).Select(x => x.User);
					qr.ToArray();
					sw.AddMeasurement();
				}
			}

			orm.DeleteAll(orm.Select<Membership>().Execute());

			Console.WriteLine(sw);
			//Console.WriteLine(profiler.GetReport());
			db.Rollback();
		}
	}

	[EventSource(Name = "loop-counter")]
	class LoopCounter : EventSource
	{
		public readonly static LoopCounter Instance = new LoopCounter();
		private IncrementingEventCounter counter;

		private LoopCounter()
		{
			counter = new IncrementingEventCounter("loop-counter", this)
			{
				DisplayName = "Loop counter",
				DisplayRateTimeScale = TimeSpan.FromSeconds(1)
			};
		}

		protected override void Dispose(bool disposing)
		{
			counter.Dispose();
			base.Dispose(disposing);
		}

		public void Inc() => counter.Increment(1);
	}


	[Table]
	class Membership : Record, IRecord
	{
		[Column]
		[ForeignKey.Cascade]
		public User User { get; set; }

		[Column]
		[ForeignKey.Cascade]
		//public long Role { get; set; }
		public RecordRef<Role> Role { get; set; }

		[Column]
		public DateTime? CreationTime { get; set; }

		[Column]
		public bool Active { get; set; }
	}

	[Table("usr"/*, Query="SELECT 1 usr_ID, usr_Name, usr_PasswordHash, usr_Group FROM Users"*/)]
	[DBName("Users")]
	class User : Record, IRecord
	{
		[Column(50)]
		public string Name { get; set; }

		[Inline("usr")]
		public UserDetails UserDetails { get; set; }

		[Subquery("SELECT COUNT(*) FROM \"Membership\" WHERE \"mem_User\" = {0}.\"usr_ID\"")]
		public int RolesCount { get; set; }

		[Subquery.Count("Membership", "mem_User", "usr_ID")]
		public int RolesCount2 { get; set; }
	}

	class UserDetails
	{
		[Column(100)]
		public string PasswordHash { get; set; }

		[Column]
		[ForeignKey.None]
		public RecordRef<User> Group { get; set; }
	}

	[Table]
	[DBName("Roles")]
	class Role : Record, IRecord
	{
		[Column(50)]
		public string Name { get; set; }
	}

	[DBName("sp_Who")]
	delegate SPWhoResult[] Who(string loginame);

	[DBName("sp_Who")]
	delegate SPWhoResult[] WhoByRec(SPWhoParameters p);

	class SPWhoResult
	{
		public int spid { get; set; }

		public int ecid { get; set; }

		public string status { get; set; }

		[DBName("loginame")]
		public string Login { get; set; }

		[DBName("cmd")]
		public string Command { get; set; }
	}

	class SPWhoParameters
	{
		[DBName("loginame")]
		[Column]
		public string Login { get; set; }
	}

}
