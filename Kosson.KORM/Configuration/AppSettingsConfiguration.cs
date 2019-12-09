#if NETSTANDARD
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Kosson.Kore.Configuration
{
	/// <summary>
	/// IConfiguration implementation using appsettings.json in application directory as backing store.
	/// </summary>
	public class AppSettingsConfiguration : NETCoreConfiguration
	{
		/// <summary>
		/// Creates a new configuration instance.
		/// </summary>
		public AppSettingsConfiguration()
			: base(null)
		{
		}

		/// <inheritdoc/>
		protected override void Initialize()
		{
			var builder = new ConfigurationBuilder();
			builder.AddJsonFile("appsettings.json", true);
			configuration = builder.Build();
			base.Initialize();
		}

		/// <inheritdoc/>
		protected override string Get(string key)
		{
			return configuration["Kosson:" + key];
		}

		/// <inheritdoc/>
		protected override void Set(string key, string value)
		{
			configuration["Kosson:" + key] = value;
		}
	}
}
#endif