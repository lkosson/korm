using Kosson.KORM.Perf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var services = new ServiceCollection();
//services.AddKORMServices<Kosson.KORM.MSSQL.SQLDB>(configuration.GetConnectionString("mssql"));
services.AddKORMServices<Kosson.KORM.SQLite.SQLiteDB>(configuration.GetConnectionString("sqlite"));
//services.AddKORMServices<Kosson.KORM.PGSQL.PGSQLDB>(configuration.GetConnectionString("pgsql"));
services.AddScoped<Runner>();
services.AddSingleton<ILoggerFactory, ConsoleLoggerFactory>();
services.AddSingleton(typeof(ILogger<>), typeof(ConsoleLogger<>));

var sp = services.BuildServiceProvider();

using (var scope = sp.CreateScope())
{
	var runner = scope.ServiceProvider.GetRequiredService<Runner>();
	runner.Run();
}