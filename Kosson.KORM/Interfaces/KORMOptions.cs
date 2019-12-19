using Microsoft.Extensions.Logging;

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

		/// <summary>
		/// Database activity logger.
		/// </summary>
		public ILogger Logger { get; set; }

		public KORMOptions()
		{
			ConnectionString = "server=(local)";
			Logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
		}
	}
}
