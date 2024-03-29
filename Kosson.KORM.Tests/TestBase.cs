﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.KORM;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Kosson.KORM.Tests
{
	[TestClass]
	public abstract class TestBase
	{
		protected virtual bool NeedsDatabase { get { return true; } }
		private IServiceScope scope;
		private IServiceProvider serviceProvider;
		protected IDB DB { get; private set; }
		protected IMetaBuilder MetaBuilder { get; private set; }
		protected IServiceProvider ServiceProvider { get; private set; }
		protected IConfiguration Configuration { get; private set; }

		protected abstract void PrepareKORMServices(IServiceCollection services);

		protected string ConnectionString(string name) => Configuration.GetSection("ConnectionStrings")[name];

		[TestInitialize]
		public virtual void Init()
		{
			Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			var services = new ServiceCollection();
			PrepareKORMServices(services);
			services.AddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));
			services.AddSingleton<ILoggerFactory, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory>();

			serviceProvider = services.BuildServiceProvider();
			scope = serviceProvider.CreateScope();

			ServiceProvider = scope.ServiceProvider;

			DB = ServiceProvider.GetRequiredService<IDB>();
			MetaBuilder = ServiceProvider.GetRequiredService<IMetaBuilder>();

			if (NeedsDatabase) DB.CreateDatabase();
		}

		[TestCleanup]
		public void Cleanup()
		{
			scope?.Dispose();
			((IDisposable)serviceProvider)?.Dispose();
		}
	}
}
