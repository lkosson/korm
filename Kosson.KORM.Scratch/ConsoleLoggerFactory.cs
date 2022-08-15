using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.Scratch
{
	class ConsoleLoggerFactory : ILoggerFactory
	{
		public void AddProvider(ILoggerProvider provider)
		{
		}

		public ILogger CreateLogger(string categoryName) => new ConsoleLogger();

		public void Dispose()
		{
		}
	}
}
