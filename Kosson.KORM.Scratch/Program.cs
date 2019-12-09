using Kosson.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace Kosson.KORM.Scratch
{
	class Program
	{
		static void Main(string[] args)
		{
			var metaBuilder = new KRUD.Meta.ReflectionMetaBuilder();
			var db = new KRUD.MSSQL.SQLDB(null, "server=(local);database=kosson;integrated security=true");
			var orm = new KRUD.ORM.DBORM(db, metaBuilder);
			var backupProvider = new KRUD.BackupProvider(orm, metaBuilder);
			Run(db, orm, metaBuilder, backupProvider);
		}

		private static void Run(IDB db, IORM orm, IMetaBuilder metaBuilder, IBackupProvider backupProvider)
		{
			db.CreateDatabase();

			db.BeginTransaction();

			var u = new User()
			{
				ID = 123,
				Name = "admin",
				UserDetails = new UserDetails()
				{
					PasswordHash = "00-11",
					Group = 80
				}
			};

			orm.InsertAll(new[] { u, u });

			//ip.Select<User>().ForUpdate().ById(10);

			var locked = orm.Select<User>().ByID(u.ID);


			u.UserDetails.Group = u.ID;

			orm.Update(u);

			var ur = new RecordRef<User>(u.ID);

			var ux = ur.Get();

			//ip.Delete<User>().WhereFieldIsNull("Name").Execute();
			//ip.Delete<User>().ById(u.ID);

			orm.Update<User>().Set("Name", "admin2").ByID(u.ID);

			ux = ur.Get();

			orm.Delete(ux);

			var who = orm.Execute<Who>();
			var r = who("sa");

			var who2 = orm.Execute<WhoByRec>();
			r = who2(new SPWhoParameters() { Login = "sa" });


			//using (var fs = new FileStream("backup.xml", FileMode.Open))
			//using (var br = new XMLBackupReader(fs))
			//{
			//	Context.Current.Get<IBackupProvider>().Restore(br);
			//}

			using (var fs = new FileStream("backup.xml", FileMode.Create))
			using (var bw = new KRUD.XMLBackupWriter(metaBuilder, fs))
			{
				var bs = backupProvider.CreateBackupSet(bw);
				bs.AddRecords<Membership>();
			}

			KRUD.DatabaseBackupWriter.Run(metaBuilder, "sqlite", "data source=backup.sqlite3;version=3", new[] { typeof(User), typeof(Role), typeof(Membership) });

			//var profiler = Context.Current.Get<IProfiler>();
			var sw = StatStopwatch.StartNew();
			//using (profiler.Start())
			{
				for (int i = 0; i < 1000; i++)
				{
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
	}

	[Table("usr"/*, Query="SELECT 1 usr_ID, usr_Name, usr_PasswordHash, usr_Group FROM Users"*/)]
	[DBName("Users")]
	class User : Record, IRecord
	{
		[Column(50)]
		public string Name { get; set; }

		[Inline("usr")]
		public UserDetails UserDetails { get; set; }

		[Subquery("SELECT COUNT(*) FROM Membership WHERE mem_User = {0}.usr_ID")]
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
