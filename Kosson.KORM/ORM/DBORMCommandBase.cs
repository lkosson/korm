using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Kosson.KORM.ORM
{
	class DBORMCommandBase<TRecord>
		where TRecord : IRecord
	{
		protected static IMetaRecord meta;
		private static string[] parametersNameCache = new[] { "P0", "P1", "P2", "P3", "P4", "P5", "P6", "P7" };
		protected readonly IMetaBuilder metaBuilder;
		private List<object> parameters;
		protected virtual bool UseFullFieldNames { get { return true; } }
		protected IEnumerable<object> Parameters { get { return parameters ?? Enumerable.Empty<object>(); } }
		private readonly ILogger logger;
		private static int nextTraceId;
		private static Stopwatch opStopwatch;

		public IDB DB { get; }

		public DBORMCommandBase(IDB db, IMetaBuilder metaBuilder, ILogger logger)
		{
			this.DB = db;
			this.metaBuilder = metaBuilder;
			this.logger = logger;
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
			{
				if (UseFullFieldNames)
				{
					var metaRecord = metafield.Record;
					while (metaRecord.InliningField != null) metaRecord = metaRecord.InliningField.Record;
					return DB.CommandBuilder.Identifier(metaRecord.Name, metafield.DBName);
				}
				else
				{
					return DB.CommandBuilder.Identifier(metafield.DBName);
				}
			}
		}

		protected TraceToken LogStart([CallerMemberName] string method = "")
		{
			if (logger == null) return default;
			if (!logger.IsEnabled(LogLevel.Information)) return default;
			var command = GetType().Name;
			command = command.Substring(5, command.IndexOf('`') - 5);
			int id = Interlocked.Increment(ref nextTraceId);
			TraceToken token = new TraceToken();
			token.id = new EventId(id);
			token.start = opStopwatch.ElapsedMilliseconds;
			token.command = command;
			token.method = method;

			logger.LogInformation(token.id, $"{command}\t{typeof(TRecord).Name}\t{method}");
			return token;
		}

		protected void LogEnd(TraceToken token, int? result = null)
		{
			if (logger == null) return;
			if (token.id == 0) return;
			logger.LogInformation(token.id, $"{token.command}\t{typeof(TRecord).Name}\t{token.method}\t{opStopwatch.ElapsedMilliseconds - token.start} ms\t{result}");
		}

		protected void LogRaw(TraceToken token, string sql, IEnumerable<object> parameters)
		{
			if (logger == null) return;
			if (token.id == 0) return;
			if (!logger.IsEnabled(LogLevel.Debug)) return;
			logger.LogDebug(token.id, $"{token.command}\t{typeof(TRecord).Name}\t{sql}");
			var i = 0;
			foreach (var parameter in parameters)
			{
				logger.LogDebug(token.id, $"{token.command}\t{typeof(TRecord).Name}\tP{i}\t{parameter}");
				i++;
			}
		}

		protected void LogRecord(LogLevel level, TraceToken token, TRecord record)
		{
			if (logger == null) return;
			if (token.id == 0) return;
			if (!logger.IsEnabled(level)) return;
			var sb = new StringBuilder();
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly && !field.IsPrimaryKey) continue;
				if (sb.Length > 0) sb.Append(", ");
				sb.Append(field.Name);
				sb.Append("=");
				var value = field.Property.GetValue(record);
				if (value == null)
				{
					sb.Append("null");
				}
				else
				{
					if (field.Type == typeof(string)) sb.Append("\"");
					sb.Append(value);
					if (field.Type == typeof(string)) sb.Append("\"");
				}
			}
			logger.Log(level, token.id, $"{token.command}\t{typeof(TRecord).Name}\t{token.method}\t{sb}");
		}

		protected struct TraceToken
		{
			public EventId id;
			public long start;
			public string command;
			public string method;
		}
	}

	abstract class DBORMCommandBase<TRecord, TCommand> : DBORMCommandBase<TRecord>
		where TRecord : IRecord
		where TCommand : IDBCommand<TCommand>
	{
		private static TCommand template;

		protected TCommand command;

		protected abstract TCommand BuildCommand(IDBCommandBuilder cb);

		public DBORMCommandBase(IDB db, IMetaBuilder metaBuilder, ILogger logger)
			: base(db, metaBuilder, logger)
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
