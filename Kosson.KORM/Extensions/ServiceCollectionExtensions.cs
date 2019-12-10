using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static void AddKORMServices<TDB>(this IServiceCollection services)
			where TDB : class, IDB
		{
			services.AddSingleton<KORMConfiguration>();
			services.AddSingleton<IConverter, Kosson.Kore.Converter.DefaultConverter>();
			services.AddSingleton<IFactory, Kosson.Kore.Factory.DynamicMethodFactory>();
			services.AddSingleton<IPropertyBinder, Kosson.Kore.PropertyBinder.ReflectionPropertyBinder>();
			services.AddSingleton<IMetaBuilder, Kosson.KRUD.Meta.ReflectionMetaBuilder>();
			services.AddSingleton<IRecordLoader, Kosson.KRUD.RecordLoader.DynamicRecordLoader>();
			services.AddSingleton<IBackupProvider, Kosson.KRUD.BackupProvider>();
			services.AddScoped<IORM, Kosson.KRUD.ORM.DBORM>();
			services.AddScoped<IDB, TDB>();
				/*
			var db = new KRUD.MSSQL.SQLDB(null, "server=(local);database=kosson;integrated security=true");
			*/
		}
	}
}
