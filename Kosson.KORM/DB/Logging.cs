﻿using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Kosson.KORM.DB
{
	class Logging
	{
		private readonly Stopwatch queryTimer;
		private readonly ILogger? logger;
		private static int nextTraceId;
		public bool TraceEnabled => logger != null && logger.IsEnabled(LogLevel.Critical);
		public bool TraceWarningEnabled => logger != null && logger.IsEnabled(LogLevel.Warning);
		public bool TraceInformationEnabled => logger != null && logger.IsEnabled(LogLevel.Information);
		public bool TraceDebugEnabled => logger != null && logger.IsEnabled(LogLevel.Debug);

		static Logging()
		{
			nextTraceId = -1;
		}

		public Logging(ILogger? logger)
		{
			this.logger = logger;
			queryTimer = Stopwatch.StartNew();
		}

		private void Trace(LogLevel level, int id, string msg)
		{
			logger?.Log(level, new EventId(id), msg);
		}

		public void Trace(LogLevel level, string msg)
		{
			logger?.Log(level, msg);
		}

		public TraceToken Start(string msg)
		{
			if (!TraceWarningEnabled) return default;
			int id = Interlocked.Increment(ref nextTraceId);
			// Keep LogLevel.Information in sync with Log(Exception) method
			if (TraceInformationEnabled) Trace(LogLevel.Information, id, msg);
			TraceToken token = new TraceToken();
			token.id = id;
			token.start = queryTimer.ElapsedMilliseconds;
			token.query = msg;
			return token;
		}

		public TraceToken Start(DbCommand command)
		{
			if (!TraceWarningEnabled) return default;
			var sql = command.CommandText;
			var token = Start(sql);
			TraceQueryParameters(LogLevel.Debug, token, command.Parameters);
			return token;
		}

		public TraceToken Start(DbBatch batch)
		{
			if (!TraceWarningEnabled) return default;

			if (TraceDebugEnabled)
			{
				var token = Start("<BATCH>");
				foreach (var command in batch.BatchCommands)
				{
					Trace(LogLevel.Information, token.id, command.CommandText);
					TraceQueryParameters(LogLevel.Debug, token, command.Parameters);
				}
				return token;
			}
			else if (TraceInformationEnabled)
			{
				var token = Start("<BATCH>");
				foreach (var command in batch.BatchCommands)
				{
					Trace(LogLevel.Information, token.id, command.CommandText);
				}
				return token;
			}
			else
			{
				var batchsql = new StringBuilder();
				foreach (var command in batch.BatchCommands)
				{
					if (batchsql.Length > 0) batchsql.AppendLine();
					var sql = command.CommandText;
					batchsql.Append(sql);
				}
				var token = Start(batchsql.ToString());
				return token;
			}
		}

		private void TraceQueryParameters(LogLevel level, TraceToken token, DbParameterCollection parameters)
		{
			if (logger == null || !logger.IsEnabled(level)) return;
			foreach (DbParameter parameter in parameters)
			{
				string val;
				if (parameter.Value is string stringParameter) val = "\"" + stringParameter.Replace('\r', ' ').Replace('\n', ' ') + "\"";
				else if (parameter.Value is DBNull || parameter.Value == null) val = "<NULL>";
				else val = parameter.Value.ToString() ?? "<NULL>";
				Trace(level, token.id, parameter.ParameterName + "=" + val);
			}
		}

		public void Stop(TraceToken token, int rows = -1)
		{
			if (token.id == 0) return;
			if (!TraceWarningEnabled) return;
			var time = queryTimer.ElapsedMilliseconds - token.start;
			var level = time >= 1000 ? LogLevel.Warning : LogLevel.Information;
			if (!TraceInformationEnabled)
			{
				if (level == LogLevel.Information) return;
				// on Warning LogLevel queries are not logged at start
				Trace(LogLevel.Warning, token.id, token.query);
			}
			var msg = rows == -1 ? time + " ms" : time + " ms\t" + rows.ToString() + " rows";
			Trace(level, token.id, msg);
		}

		public void StopStart(ref TraceToken token, string msg)
		{
			if (token.id == 0) return;
			if (!TraceWarningEnabled) return;
			Stop(token);
			if (!TraceInformationEnabled) return;
			token.start = queryTimer.ElapsedMilliseconds;
			token.query = msg;
			Trace(LogLevel.Information, token.id, msg);
		}

		public void Log(KORMException exc, TraceToken token)
		{
			if (!TraceEnabled) return;
			var exceptionLevel = exc is KORMInvalidStructureException ? LogLevel.Warning : LogLevel.Error;
			if (logger == null || !logger.IsEnabled(exceptionLevel)) return;

			Trace(exceptionLevel, token.id, exc.GetType().Name + ": " + exc.OriginalMessage);

			if (exc.CommandText != null)
			{
				// On Information LogLevel queries are logged at start
				if (!TraceInformationEnabled) Trace(exceptionLevel, token.id, exc.CommandText);

				// On Debug LogLevel parameters are logged at start
				if (exc.CommandParameters != null && !TraceDebugEnabled) TraceQueryParameters(exceptionLevel, token, exc.CommandParameters);
			}
		}
	}

	struct TraceToken
	{
		public int id;
		public long start;
		public string query;
	}
}
