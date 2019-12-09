using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Kosson.Kore.Configuration
{
	/// <summary>
	/// IConfiguration implementation using .NET Core IConfiguration as backing store.
	/// </summary>
	public class NETCoreConfiguration : CascadingConfiguration
	{
		/// <summary>
		/// Configuration backing store.
		/// </summary>
		protected IConfiguration configuration;

		/// <summary>
		/// Creates a new configuration instance based on provided configuration.
		/// </summary>
		public NETCoreConfiguration(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		/// <inheritdoc/>
		protected override string Get(string key)
		{
			return configuration[key];
		}

		/// <inheritdoc/>
		protected override void Set(string key, string value)
		{
			configuration[key] = value;
		}
	}
}