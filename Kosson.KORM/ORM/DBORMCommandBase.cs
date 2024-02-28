using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Kosson.KORM.ORM
{
	class DBORMCommandBase
	{
		internal static string[] parametersNameCache = new[] { "P0", "P1", "P2", "P3", "P4", "P5", "P6", "P7" };
		internal static int nextTraceId;
		internal static Stopwatch opStopwatch = Stopwatch.StartNew();
	}

	class DBORMCommandBase<TRecord> : DBORMCommandBase
		where TRecord : IRecord
	{
		protected static IMetaRecord meta;
		protected readonly IMetaBuilder metaBuilder;
		private List<object> parameters;
		protected virtual bool UseFullFieldNames { get { return true; } }
		protected IEnumerable<object> Parameters { get { return parameters ?? Enumerable.Empty<object>(); } }
		private readonly ILogger operationLogger;
		private readonly ILogger recordLogger;

		public IDB DB { get; }

		public IMetaRecord Meta => meta;

		public DBORMCommandBase(IDB db, IMetaBuilder metaBuilder, ILogger operationLogger, ILogger recordLogger)
		{
			this.DB = db;
			this.metaBuilder = metaBuilder;
			this.operationLogger = operationLogger;
			this.recordLogger = recordLogger;
			if (meta == null) meta = metaBuilder.Get(typeof(TRecord));
			if (opStopwatch == null) opStopwatch = Stopwatch.StartNew();
		}

		public IDBExpression Parameter(object value)
		{
			if (parameters == null) parameters = new List<object>(8);
			var pnum = parameters.Count;
			var pname = pnum < parametersNameCache.Length ? parametersNameCache[pnum] : "P" + pnum;
			parameters.Add(value);
			return DB.CommandBuilder.Parameter(pname);
		}

		public IDBExpression Array<T>(T[] values)
		{
			var pvalues = new IDBExpression[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				pvalues[i] = Parameter(values[i]);
			}
			return DB.CommandBuilder.Array(pvalues);
		}

		public IDBIdentifier Field(string name)
		{
			IMetaRecordField metafield = meta.GetField(name);
			if (metafield == null) return DB.CommandBuilder.Identifier(name);

			if (UseFullFieldNames)
			{
				var tableAlias = meta.GetFieldTableAlias(name);
				return DB.CommandBuilder.Identifier(tableAlias, metafield.DBName);
			}
			else
			{
				return DB.CommandBuilder.Identifier(metafield.DBName);
			}
		}

		private string LogFormatDesc(TraceToken token)
		{
			var ormcommand = GetType().Name;
			ormcommand = ormcommand.Substring(5, ormcommand.IndexOf('`') - 5);
			var desc = ormcommand + "\t" + typeof(TRecord).Name + (token.command == null ? "" : "\t" + token.command.ToStringForLog());
			return desc;
		}

		protected TraceToken LogStart([CallerMemberName] string method = "", IDBSelect dbcommand = null)
		{
			if (operationLogger == null) return default;
			if (!operationLogger.IsEnabled(LogLevel.Warning)) return default;
			int id = Interlocked.Increment(ref nextTraceId);
			TraceToken token = new TraceToken();
			token.id = new EventId(id);
			token.start = opStopwatch.ElapsedMilliseconds;
			token.command = dbcommand;
			if (operationLogger.IsEnabled(LogLevel.Debug)) operationLogger.LogDebug(token.id, $"{LogFormatDesc(token)}");
			return token;
		}

		protected void LogEnd(TraceToken token, int? result = null)
		{
			if (operationLogger == null) return;
			if (token.id == 0) return;
			var time = opStopwatch.ElapsedMilliseconds - token.start;
			if (time >= 1000)
			{
				if (operationLogger.IsEnabled(LogLevel.Warning)) operationLogger.LogWarning(token.id, $"{LogFormatDesc(token)}\t{time} ms\t{result}");
			}
			else
			{
				if (operationLogger.IsEnabled(LogLevel.Information)) operationLogger.LogInformation(token.id, $"{LogFormatDesc(token)}\t{time} ms\t{result}");
			}
		}

		protected void LogRaw(TraceToken token, string sql, IEnumerable<object> parameters)
		{
			if (operationLogger == null) return;
			if (token.id == 0) return;
			if (!operationLogger.IsEnabled(LogLevel.Debug)) return;
			var desc = LogFormatDesc(token);
			operationLogger.LogDebug(token.id, $"{desc}\t{sql}");
			var i = 0;
			foreach (var parameter in parameters)
			{
				operationLogger.LogDebug(token.id, $"{desc}\t@P{i}\t{parameter}");
				i++;
			}
		}

		protected void LogRecord(LogLevel level, TraceToken token, TRecord record)
		{
			if (recordLogger == null) return;
			if (token.id == 0) return;
			if (!recordLogger.IsEnabled(level)) return;
			recordLogger.Log(level, token.id, $"{LogFormatDesc(token)}\t{(record.Ref().IsNull ? "" : record.Ref() + "\t")}{record.ToStringByFields(meta)}");
		}

		protected void LogID(TraceToken token, TRecord record)
		{
			if (recordLogger == null) return;
			if (token.id == 0) return;
			if (!recordLogger.IsEnabled(LogLevel.Information)) return;
			recordLogger.Log(LogLevel.Information, token.id, $"{LogFormatDesc(token)}\t{record.Ref()}");
		}

		protected struct TraceToken
		{
			public EventId id;
			public long start;
			public IDBSelect command;
		}
	}

	abstract class DBORMCommandBase<TRecord, TCommand> : DBORMCommandBase<TRecord>
		where TRecord : IRecord
		where TCommand : IDBCommand<TCommand>
	{
		private static TCommand template;

		protected TCommand command;

		protected abstract TCommand BuildCommand(IDBCommandBuilder cb);

		public DBORMCommandBase(IDB db, IMetaBuilder metaBuilder, ILogger operationLogger, ILogger recordLogger)
			: base(db, metaBuilder, operationLogger, recordLogger)
		{
			var cb = db.CommandBuilder;

			var localTemplate = template;
			if (localTemplate == null || localTemplate.Builder != cb)
			{
				localTemplate = BuildCommand(cb);
				template = localTemplate;
			}
			command = localTemplate.Clone();
		}
	}
}
