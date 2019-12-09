using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Kore.Configuration
{
	/// <summary>
	/// Base class for IConfiguration components providing some configuration key-value pairs and using configuration from base context for other keys.
	/// </summary>
	public abstract class CascadingConfiguration : IConfiguration
	{
		private IConfiguration baseConfiguration;

		string IConfiguration.this[string key]
		{
			get
			{
				var value = Get(key);
				if (value == null && baseConfiguration != null) return baseConfiguration[key];
				return value;
			}
			set
			{
				Set(key, value);
			}
		}

		/// <summary>
		/// Retrieves a configuration value for a given key.
		/// </summary>
		/// <param name="key">Configuration key to retrieve.</param>
		/// <returns>Configuration value for a given key.</returns>
		protected abstract string Get(string key);

		/// <summary>
		/// Sets a configuration value for a given key.
		/// </summary>
		/// <param name="key">Configuration key to set.</param>
		/// <param name="value">Configuration value to set.</param>
		protected abstract void Set(string key, string value);
	}
}
