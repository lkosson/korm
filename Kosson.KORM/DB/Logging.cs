﻿using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;

namespace Kosson.KORM.DB
{
	class Logging
	{
		private readonly Stopwatch queryTimer;
		private readonly ILogger logger;
		private static int nextTraceId;
		public bool TraceEnabled => logger != null && logger.IsEnabled(LogLevel.Critical);
		public bool TraceInformationEnabled => logger != null && logger.IsEnabled(LogLevel.Information);
		public bool TraceDebugEnabled => logger != null && logger.IsEnabled(LogLevel.Debug);

		static Logging()
		{
			nextTraceId = -1;
		}

		public Logging(ILogger logger)
		{
			this.logger = logger;
			queryTimer = Stopwatch.StartNew();
		}

		private void Trace(LogLevel level, int id, string msg)
		{
			logger.Log(level, new EventId(id), msg);
		}

		public void Trace(LogLevel level, string msg)
		{
			logger.Log(level, msg);
		}

		public TraceToken Start(string msg)
		{
			if (!TraceInformationEnabled) return default(TraceToken);
			int id = Interlocked.Increment(ref nextTraceId);
			// Keep LogLevel.Information in sync with Log(Exception) method
			Trace(LogLevel.Information, id, msg);
			TraceToken token = new TraceToken();
			token.id = id;
			token.start = queryTimer.ElapsedMilliseconds;
			return token;
		}

		public TraceToken Start(DbCommand command)
		{
			if (!TraceInformationEnabled) return default(TraceToken);
			var sql = command.CommandText;
			var token = Start(sql);
			TraceQueryParameters(LogLevel.Debug, token, command);
			return token;
		}

		private void TraceQueryParameters(LogLevel level, TraceToken token, DbCommand command)
		{
			if (!logger.IsEnabled(level)) return;
			foreach (DbParameter parameter in command.Parameters)
			{
				string val;
				if (parameter.Value is string) val = "\"" + parameter.Value.ToString().Replace('\r', ' ').Replace('\n', ' ') + "\"";
				else if (parameter.Value is DBNull || parameter.Value == null) val = "<NULL>";
				else val = parameter.Value.ToString();
				Trace(level, token.id, parameter.ParameterName + "=" + val);
			}
		}

		public void Stop(TraceToken token, int rows = -1)
		{
			if (token.id == 0) return;
			if (!TraceInformationEnabled) return;
			if (rows == -1)
				Trace(LogLevel.Information, token.id, (queryTimer.ElapsedMilliseconds - token.start).ToString() + " ms");
			else
				Trace(LogLevel.Information, token.id, (queryTimer.ElapsedMilliseconds - token.start).ToString() + " ms\t" + rows.ToString() + " rows");
		}

		public void StopStart(ref TraceToken token, string msg)
		{
			if (token.id == 0) return;
			if (!TraceInformationEnabled) return;
			Stop(token);
			token.start = queryTimer.ElapsedMilliseconds;
			Trace(LogLevel.Information, token.id, msg);
		}

		public void Log(Exception exc, DbCommand cmd, TraceToken token)
		{
			if (!TraceEnabled) return;
			var exceptionLevel = exc is KORMInvalidStructureException ? LogLevel.Warning : LogLevel.Error;
			if (!logger.IsEnabled(exceptionLevel)) return;
			Trace(exceptionLevel, token.id, exc.Message);
			// On Information and Debug LogLevels queries are already logged at start
			if (cmd != null && !logger.IsEnabled(LogLevel.Information))
			{
				Trace(exceptionLevel, token.id, cmd.CommandText);
				TraceQueryParameters(exceptionLevel, token, cmd);
			}
		}
	}

	struct TraceToken
	{
		public int id;
		public long start;
	}
}
