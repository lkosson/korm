using Kosson.KORM;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for Microsoft.Extensions.DependencyInjection.IServiceCollection.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers required KORM services.
		/// </summary>
		/// <typeparam name="TDB">Database provider</typeparam>
		/// <param name="services">Services collection</param>
		/// <param name="connectionString">Hard-coded connectionstring to use.</param>
		public static void AddKORMServices<TDB>(this IServiceCollection services, string connectionString = null)
			where TDB : class, IDB
		{
			services.AddOptions<KORMOptions>();
			services.AddSingleton<IConverter, Kosson.KORM.Converter.DefaultConverter>();
			services.AddSingleton<IFactory, Kosson.KORM.Factory.DynamicMethodFactory>();
			services.AddSingleton<IPropertyBinder, Kosson.KORM.PropertyBinder.ReflectionPropertyBinder>();
			services.AddSingleton<IMetaBuilder, Kosson.KORM.Meta.ReflectionMetaBuilder>();
			services.AddTransient<IBackupProvider, Kosson.KORM.Backup.BackupProvider>();
			services.AddSingleton<IRecordCloner, Kosson.KORM.RecordCloner.FactoryRecordCloner>();
			services.AddTransient<IBackupRestorer, Kosson.KORM.Backup.BackupRestorer>();
			services.AddTransient<IDatabaseCopier, Kosson.KORM.Backup.DatabaseCopier>();
			services.AddTransient<IDatabaseEraser, Kosson.KORM.Backup.DatabaseEraser>();
			services.AddTransient<IDatabaseScriptGenerator, Kosson.KORM.Backup.DatabaseScriptGenerator>();
			services.AddScoped<IORM, Kosson.KORM.ORM.DBORM>();
			services.AddScoped<IDB, TDB>();
			services.AddTransient<Kosson.KORM.XMLBackup>();
			services.Configure<KORMOptions>(options => 
			{ 
				if (connectionString != null) options.ConnectionString = connectionString;
			});
		}
	}
}
