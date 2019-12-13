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
			services.AddSingleton<IConverter, Kosson.KORM.Converter.DefaultConverter>();
			services.AddSingleton<IFactory, Kosson.KORM.Factory.DynamicMethodFactory>();
			services.AddSingleton<IPropertyBinder, Kosson.KORM.PropertyBinder.ReflectionPropertyBinder>();
			services.AddSingleton<IMetaBuilder, Kosson.KORM.Meta.ReflectionMetaBuilder>();
			services.AddSingleton<IRecordLoader, Kosson.KORM.RecordLoader.DynamicRecordLoader>();
			services.AddTransient<IBackupProvider, Kosson.KORM.Backup.BackupProvider>();
			services.AddSingleton<IRecordCloner, Kosson.KORM.RecordCloner.FactoryRecordCloner>();
			services.AddTransient<IBackupRestorer, Kosson.KORM.Backup.BackupRestorer>();
			services.AddTransient<IDatabaseCopier, Kosson.KORM.Backup.DatabaseCopier>();
			services.AddTransient<IDatabaseEraser, Kosson.KORM.Backup.DatabaseEraser>();
			services.AddTransient<IDatabaseScriptGenerator, Kosson.KORM.Backup.DatabaseScriptGenerator>();
			services.AddScoped<IORM, Kosson.KORM.ORM.DBORM>();
			services.AddScoped<IDB, TDB>();
			services.AddTransient<Kosson.KORM.Backup.XMLBackup>();
		}
	}
}
