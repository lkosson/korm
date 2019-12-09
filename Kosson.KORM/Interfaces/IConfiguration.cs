using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Component providing access to key-value pairs containing application configuration.
	/// </summary>
	public interface IConfiguration
	{
		/// <summary>
		/// Gets or sets conviguration value for provided key.
		/// </summary>
		/// <param name="key">Configuration key to retrieve or change.</param>
		/// <returns>Configuration value for provided key.</returns>
		string this[string key] { get; set; }
	}
}
