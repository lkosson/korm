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
			services.AddKORMServices<MSSQL.SQLDB>("server=(local);database=kosson;integrated security=true");
			services.AddScoped<Runner>();
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
