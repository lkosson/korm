using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.Kontext;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract class TestBase
	{
		protected abstract string Provider { get; }
		protected virtual bool NeedsDatabase { get { return true; } }

		private IContext ctxDefault;

		[TestInitialize]
		public virtual void Init()
		{
			string connectionString;
			using (Context.Begin())
			{
				Context.Current.Add<IConfiguration>(new Kosson.Kore.Configuration.NETConfiguration());
				connectionString = Context.Current.Configuration(Provider + "-connectionstring"); 
			}
			if (String.IsNullOrEmpty(connectionString)) Assert.Inconclusive("Connection string for " + Provider + " missing.");
			ctxDefault = Context.Begin();
			ctxDefault.Add<IConfiguration>(new Kosson.Kore.Configuration.MemoryConfiguration());
			ctxDefault.Add<IConverter>(new Kosson.Kore.Converter.DefaultConverter());
			ctxDefault.Add<IApplicationState>(new Kosson.Kore.ApplicationState.DefaultApplicationState());
			ctxDefault.Add<IFactory>(new Kosson.Kore.Factory.DynamicMethodFactory());
			ctxDefault.Add<IDBFactory>(new Kosson.KRUD.DBFactory());

			ctxDefault.Configuration()["db-provider"] = Provider;
			ctxDefault.Configuration()["db-connectionstring"] = connectionString;

			ctxDefault.Get<IDBFactory>().RegisterProvider(Kosson.KRUD.MSSQL.Provider.Instance);
			ctxDefault.Get<IDBFactory>().RegisterProvider(Kosson.KRUD.PGSQL.Provider.Instance);
			ctxDefault.Get<IDBFactory>().RegisterProvider(Kosson.KRUD.SQLite.Provider.Instance);

			if (NeedsDatabase) ctxDefault.Get<IDB>().CreateDatabase();
		}

		[TestCleanup]
		public void Cleanup()
		{
			ctxDefault.Dispose();
		}
	}
}
