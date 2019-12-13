using Kosson.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kosson.KORM.Scratch
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddKORMServices<MSSQL.SQLDB>();
			services.AddScoped<Runner>();
			services.AddSingleton<ILogger, ConsoleLogger>();

			var sp = services.BuildServiceProvider();

			using (var scope = sp.CreateScope())
			{
				var configuration = scope.ServiceProvider.GetRequiredService<KORMConfiguration>();
				configuration.ConnectionString = "server=(local);database=kosson;integrated security=true";
				var runner = scope.ServiceProvider.GetRequiredService<Runner>();
				runner.Run();
			}
		}
	}
}
