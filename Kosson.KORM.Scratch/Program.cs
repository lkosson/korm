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
			var converter = new Kore.Converter.DefaultConverter();
			var propertyBinder = new Kore.PropertyBinder.ReflectionPropertyBinder();
			var metaBuilder = new KRUD.Meta.ReflectionMetaBuilder();
			var recordLoader = new KRUD.RecordLoader.DynamicRecordLoader(metaBuilder);
			var factory = new Kore.Factory.DynamicMethodFactory();
			var db = new KRUD.MSSQL.SQLDB(null, "server=(local);database=kosson;integrated security=true");
			var orm = new KRUD.ORM.DBORM(db, metaBuilder, converter, recordLoader, factory);
			var backupProvider = new KRUD.BackupProvider(orm, metaBuilder, propertyBinder, converter);
			Run(db, orm, metaBuilder, backupProvider, propertyBinder, converter, recordLoader, factory);
		}

		private static void Run(IDB db, IORM orm, IMetaBuilder metaBuilder, IBackupProvider backupProvider, IPropertyBinder propertyBinder, IConverter converter, IRecordLoader recordLoader, IFactory factory)
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
					Group = 3
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

			//using (var fs = new FileStream("backup.xml", FileMode.Open))
			//using (var br = new XMLBackupReader(fs))
			//{
			//	Context.Current.Get<IBackupProvider>().Restore(br);
			//}

			using (var fs = new FileStream("backup.xml", FileMode.Create))
			using (var bw = new KRUD.XMLBackupWriter(metaBuilder, propertyBinder, converter, fs))
			{
				var bs = backupProvider.CreateBackupSet(bw);
				bs.AddRecords<Membership>();
			}

			KRUD.DatabaseBackupWriter.Run(metaBuilder, propertyBinder, converter, recordLoader, factory, "sqlite", "data source=backup.sqlite3;version=3", new[] { typeof(User), typeof(Role), typeof(Membership) });

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
