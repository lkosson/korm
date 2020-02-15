using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.Scratch
{
	class ConsoleLogger<T> : ILogger<T>, ILogger
	{
		IDisposable ILogger.BeginScope<TState>(TState state) => null;
		bool ILogger.IsEnabled(LogLevel logLevel) => true;

		void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (logLevel < LogLevel.Warning) return;
			var c = Console.ForegroundColor;
			if (logLevel == LogLevel.Critical) Console.ForegroundColor = ConsoleColor.Red;
			else if (logLevel == LogLevel.Error) Console.ForegroundColor = ConsoleColor.Red;
			else if (logLevel == LogLevel.Warning) Console.ForegroundColor = ConsoleColor.Yellow;
			else if (logLevel == LogLevel.Information) Console.ForegroundColor = ConsoleColor.Gray;
			else if (logLevel == LogLevel.Debug) Console.ForegroundColor = ConsoleColor.DarkGray;
			else if (logLevel == LogLevel.Trace) Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine(formatter(state, exception));
			Console.ForegroundColor = c;
		}
	}
}
