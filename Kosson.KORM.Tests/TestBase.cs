using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract class TestBase
	{
		protected abstract string Provider { get; }
		protected virtual bool NeedsDatabase { get { return true; } }
		//private IScope
		protected IDB DB { get; private set; }
		protected IORM ORM { get; private set; }
		protected IMetaBuilder MetaBuilder { get; private set; }

		[TestInitialize]
		public virtual void Init()
		{
			var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
			services.AddKORMServices<KRUD.MSSQL.SQLDB>();
			services.AddSingleton<ILogger, Microsoft.Extensions.Logging.Abstractions.NullLogger>();

			var sp = services.BuildServiceProvider();

			var scope = sp.CreateScope();
			
			var configuration = scope.ServiceProvider.GetRequiredService<KORMConfiguration>();
			configuration.ConnectionString = "server=(local);database=kosson-tests;integrated security=true";

			//if (NeedsDatabase) ctxDefault.Get<IDB>().CreateDatabase();
		}

		[TestCleanup]
		public void Cleanup()
		{
			//ctxDefault.Dispose();
		}
	}
}
