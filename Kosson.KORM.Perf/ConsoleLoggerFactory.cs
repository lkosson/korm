using Microsoft.Extensions.Logging;

namespace Kosson.KORM.Perf;

class ConsoleLoggerFactory : ILoggerFactory
{
	public void AddProvider(ILoggerProvider provider) { }
	public ILogger CreateLogger(string categoryName) => new ConsoleLogger();
	public void Dispose() { }
}
