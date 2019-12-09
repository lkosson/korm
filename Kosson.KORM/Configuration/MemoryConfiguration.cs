using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Kore.Configuration
{
	/// <summary>
	/// Component providing IConfiguration based on memory-only dictionary.
	/// </summary>
	public class MemoryConfiguration : CascadingConfiguration
	{
		private Dictionary<string, string> configuration = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

		/// <inheritdoc/>
		protected override string Get(string key)
		{
			if (configuration.ContainsKey(key))
				return configuration[key];
			else
				return null;
		}

		/// <inheritdoc/>
		protected override void Set(string key, string value)
		{
			configuration[key] = value;
		}
	}
}
