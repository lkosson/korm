using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract class TestBase
	{
		protected abstract string Provider { get; }
		protected virtual bool NeedsDatabase { get { return true; } }
		private IServiceScope scope;
		private IServiceProvider serviceProvider;
		protected IDB DB { get; private set; }
		protected IMetaBuilder MetaBuilder { get; private set; }
		protected IServiceProvider ServiceProvider { get; private set; }

		[TestInitialize]
		public virtual void Init()
		{
			var services = new ServiceCollection();
			services.AddKORMServices<KRUD.MSSQL.SQLDB>();
			services.AddSingleton<ILogger>(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);

			serviceProvider = services.BuildServiceProvider();
			scope = serviceProvider.CreateScope();

			ServiceProvider = scope.ServiceProvider;

			var configuration = ServiceProvider.GetRequiredService<KORMConfiguration>();
			configuration.ConnectionString = "server=(local);database=kosson-tests;integrated security=true";

			DB = ServiceProvider.GetRequiredService<IDB>();
			MetaBuilder = ServiceProvider.GetRequiredService<IMetaBuilder>();

			if (NeedsDatabase) DB.CreateDatabase();
		}

		[TestCleanup]
		public void Cleanup()
		{
			scope.Dispose();
			((IDisposable)serviceProvider).Dispose();
		}
	}
}
