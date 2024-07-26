using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.Scratch
{
	class ConsoleLogger : ILogger
	{
		LogLevel MinLevel => LogLevel.Trace;
		IDisposable ILogger.BeginScope<TState>(TState state) => null;
		bool ILogger.IsEnabled(LogLevel logLevel) => logLevel >= MinLevel;

		void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (logLevel < MinLevel) return;
			var c = Console.ForegroundColor;
			if (eventId.Id > 0)
			{
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write($"[{eventId.Id:000000}]\t");
			}
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

	class ConsoleLogger<T> : ConsoleLogger, ILogger<T>
	{
	}
}
