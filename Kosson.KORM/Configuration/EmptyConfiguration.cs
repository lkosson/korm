using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Kore.Configuration
{
	/// <summary>
	/// IConfiguration implementation providing empty values for all keys and ignoring value changes.
	/// </summary>
	public class EmptyConfiguration : IConfiguration
	{
		string IConfiguration.this[string key]
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
	}
}
