using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Kosson.KORM.ORM
{
	class DBORMCommandBase
	{
		internal static string[] parametersNameCache = ["P0", "P1", "P2", "P3", "P4", "P5", "P6", "P7"];
		internal static int nextTraceId;
		internal static Stopwatch opStopwatch = Stopwatch.StartNew();
	}

	class DBORMCommandBase<TRecord> : DBORMCommandBase
		where TRecord : IRecord
	{
		protected static IMetaRecord meta = default!;
		private List<object?>? parameters;
		protected virtual bool UseFullFieldNames { get { return true; } }
		protected IEnumerable<object?> Parameters { get { return parameters ?? Enumerable.Empty<object?>(); } }
		private readonly ILogger operationLogger;
		private readonly ILogger recordLogger;
		protected readonly IMetaBuilder metaBuilder;

		public IDB DB { get; }

		public IMetaRecord Meta => meta;

		public DBORMCommandBase(IDB db, IMetaBuilder metaBuilder, ILogger operationLogger, ILogger recordLogger)
		{
			this.DB = db;
			this.operationLogger = operationLogger;
			this.recordLogger = recordLogger;
			this.metaBuilder = metaBuilder;
			meta ??= metaBuilder.Get(typeof(TRecord));
			opStopwatch ??= Stopwatch.StartNew();
		}

		protected DBORMCommandBase(DBORMCommandBase<TRecord> template)
		{
			DB = template.DB;
			operationLogger = template.operationLogger;
			recordLogger = template.recordLogger;
			metaBuilder = template.metaBuilder;
			if (template.parameters != null) parameters = new List<object?>(template.parameters);
		}

		public IDBExpression Parameter(object? value)
		{
			parameters ??= new List<object?>(8);
			var pnum = parameters.Count;
			var pname = pnum < parametersNameCache.Length ? parametersNameCache[pnum] : "P" + pnum;
			parameters.Add(value);
			return DB.CommandBuilder.Parameter(pname);
		}

		public IDBExpression Array(IEnumerable values)
		{
			var pvalues = new List<IDBExpression>();
			foreach (var value in values)
				pvalues.Add(Parameter(value));
			return DB.CommandBuilder.Array(pvalues.ToArray());
		}

		public IDBIdentifier Field(string name)
		{
			var metafield = meta.GetField(name);
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

		private string CommandForLog(TraceToken token, bool includeCommand)
		{
			var ormcommand = GetType().Name;
			ormcommand = ormcommand.Substring(5, ormcommand.IndexOf('`') - 5);
			ormcommand = ormcommand + "\t" + typeof(TRecord).Name;
			if (token.command == null || !includeCommand) return ormcommand;
			return ormcommand +  "\t" + token.command.ToStringForLog();
		}

		protected TraceToken LogStart(IDBSelect? dbcommand = null)
		{
			if (operationLogger == null) return default;
			if (!operationLogger.IsEnabled(LogLevel.Warning)) return default;
			int id = Interlocked.Increment(ref nextTraceId);
			TraceToken token = new TraceToken();
			token.id = new EventId(id);
			token.start = opStopwatch.ElapsedMilliseconds;
			token.command = dbcommand;
			if (operationLogger.IsEnabled(LogLevel.Debug)) operationLogger.LogDebug(token.id, "{command}", CommandForLog(token, true));
			return token;
		}

		protected void LogEnd(TraceToken token, int? result = null)
		{
			if (operationLogger == null) return;
			if (token.id == 0) return;
			var time = opStopwatch.ElapsedMilliseconds - token.start;
			var level = time >= 1000 ? LogLevel.Warning : LogLevel.Information;
			if (!operationLogger.IsEnabled(level)) return;
			if (result == null) operationLogger.Log(level, token.id, "{command}\t{time} ms", CommandForLog(token, true), time);
			else operationLogger.Log(level, token.id, "{command}\t{time} ms\t{result}", CommandForLog(token, true), time, result);
		}

		protected void LogRaw(TraceToken token, string sql, IEnumerable<object?> parameters)
		{
			if (operationLogger == null) return;
			if (token.id == 0) return;
			if (!operationLogger.IsEnabled(LogLevel.Debug)) return;
			var desc = CommandForLog(token, true);
			operationLogger.LogDebug(token.id, "{desc}\t{sql}", desc, sql);
			var i = 0;
			foreach (var parameter in parameters)
			{
				operationLogger.LogDebug(token.id, "{desc}\t@P{i}\t{parameter}", desc, i, parameter);
				i++;
			}
		}

		protected void LogRecord(LogLevel level, TraceToken token, TRecord record)
		{
			if (recordLogger == null) return;
			if (token.id == 0) return;
			if (!recordLogger.IsEnabled(level)) return;
			if (record.Ref().IsNull) recordLogger.Log(level, token.id, "{command}\t{fields}", CommandForLog(token, false), record.ToStringByFields(meta));
			else recordLogger.Log(level, token.id, "{command}\t{ref}\t{fields}", CommandForLog(token, false), record.Ref(), record.ToStringByFields(meta));
		}

		protected void LogID(TraceToken token, TRecord record)
		{
			if (recordLogger == null) return;
			if (token.id == 0) return;
			if (!recordLogger.IsEnabled(LogLevel.Information)) return;
			recordLogger.Log(LogLevel.Information, token.id, "{command}\t{ref}", CommandForLog(token, false), record.Ref());
		}

		protected struct TraceToken
		{
			public EventId id;
			public long start;
			public IDBSelect? command;
		}
	}

	abstract class DBORMCommandBase<TRecord, TCommand> : DBORMCommandBase<TRecord>
		where TRecord : IRecord
		where TCommand : IDBCommand<TCommand>
	{
		protected static TCommand? template;

		private TCommand? command;
		protected TCommand Command
		{
			get
			{
				if (command == null)
				{
					var cb = DB.CommandBuilder;
					var localTemplate = template;
					if (localTemplate == null || localTemplate.Builder != cb)
					{
						localTemplate = BuildCommand(cb);
						template = localTemplate;
					}
					command = localTemplate.Clone();
				}
				return command;
			}
		}

		protected abstract TCommand BuildCommand(IDBCommandBuilder cb);

		public DBORMCommandBase(IDB db, IMetaBuilder metaBuilder, ILogger operationLogger, ILogger recordLogger)
			: base(db, metaBuilder, operationLogger, recordLogger)
		{
		}

		protected DBORMCommandBase(DBORMCommandBase<TRecord, TCommand> template)
			: base(template)
		{
			if (template.command != null) command = template.command.Clone();
		}
	}
}
