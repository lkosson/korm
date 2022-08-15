using Kosson.KORM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kosson.KORM.Scratch
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();
			//services.AddKORMServices<MSSQL.SQLDB>("server=(local);database=kosson;integrated security=true");
			//services.AddKORMServices<SQLite.SQLiteDB>("data source=:memory:");
			//services.AddKORMServices<SQLite.SQLiteDB>("data source=korm.sqlite3");
			services.AddKORMServices<PGSQL.PGSQLDB>("host=10.0.0.149;database=korm;username=korm;password=korm");
			services.AddScoped<Runner>();
			services.AddSingleton<ILoggerFactory, ConsoleLoggerFactory>();
			services.AddSingleton(typeof(ILogger<>), typeof(ConsoleLogger<>));

			var sp = services.BuildServiceProvider();

			using (var scope = sp.CreateScope())
			{
				var runner = scope.ServiceProvider.GetRequiredService<Runner>();
				runner.Run();
			}
		}
	}
}
