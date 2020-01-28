﻿using Microsoft.Extensions.Logging;

namespace Kosson.KORM
{
	/// <summary>
	/// Database connection parameters.
	/// </summary>
	public class KORMOptions
	{
		/// <summary>
		/// Database connection string.
		/// </summary>
		public string ConnectionString { get; set; }

		public KORMOptions()
		{
			ConnectionString = "server=(local)";
		}
	}
}
