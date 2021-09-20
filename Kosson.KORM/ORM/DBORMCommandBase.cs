﻿using System.Collections.Generic;
using System.Linq;

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

		public IDB DB { get; }

		public DBORMCommandBase(IDB db, IMetaBuilder metaBuilder)
		{
			this.DB = db;
			this.metaBuilder = metaBuilder;
			if (meta == null) meta = metaBuilder.Get(typeof(TRecord));
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
	}

	abstract class DBORMCommandBase<TRecord, TCommand> : DBORMCommandBase<TRecord>
		where TRecord : IRecord
		where TCommand : IDBCommand<TCommand>
	{
		private static TCommand template;

		protected TCommand command;

		protected abstract TCommand BuildCommand(IDBCommandBuilder cb);

		public DBORMCommandBase(IDB db, IMetaBuilder metaBuilder)
			: base(db, metaBuilder)
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
