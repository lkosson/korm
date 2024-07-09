using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMSelect<TRecord> : DBORMCommandBase<TRecord, IDBSelect>, IORMSelect<TRecord> where TRecord : class, IRecord, new()
	{
		private readonly IConverter converter;
		private readonly IFactory factory;
		private readonly LoaderFromReaderByIndexDelegate<TRecord> loaderFromReader;

		public DBORMSelect(IDB db, IMetaBuilder metaBuilder, IConverter converter, IFactory factory, LoaderFromReaderByIndexDelegate<TRecord> loaderFromReader, ILogger operationLogger, ILogger recordLogger)
			: base(db, metaBuilder, operationLogger, recordLogger)
		{
			this.converter = converter;
			this.factory = factory;
			this.loaderFromReader = loaderFromReader;
		}

		private DBORMSelect(DBORMSelect<TRecord> template)
			: base(template)
		{
			converter = template.converter;
			factory = template.factory;
			loaderFromReader = template.loaderFromReader;
		}

		IORMSelect<TRecord> IORMCommand<IORMSelect<TRecord>>.Clone() => new DBORMSelect<TRecord>(this);

		protected override IDBSelect BuildCommand(IDBCommandBuilder cb)
		{
			var template = cb.Select();
			if (meta.DBQuery == null)
				template.From(cb.Identifier(meta.DBSchema, meta.DBName), cb.Identifier(meta.Name));
			else
				template.FromSubquery(cb.Expression(meta.DBQuery), cb.Identifier(meta.Name));
			PrepareTemplate(cb, template, meta, meta.Name, meta.Name, null, new Stack<long>());
			return template;
		}

		private static void PrepareTemplate(IDBCommandBuilder cb, IDBSelect template, IMetaRecord meta, string tableAliasForColumns, string tableAliasForJoins, string? fieldPrefix, Stack<long> descentPath)
		{
			// keep order in sync with ReaderRecordLoaderBuilder
			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;

				var fieldNameWithPrefix = fieldPrefix == null ? field.Name : fieldPrefix + "." + field.Name;

				if (field.IsInline)
				{
					if (descentPath.Contains(field.ID)) throw new KORMInvalidOperationException("Cyclic inlines detected on " + meta.Name + "." + field.Name + ".");
					var inlineAlias = tableAliasForJoins + "." + field.Name;
					descentPath.Push(field.ID);
					// Inlined columns are referencing inlining table alias - inlined column name contains inlining descent path and is unique even if same inline in included multiple times.
					// Joins from inlined columns should use table alias based on inlining descent path; using just inlining table alias leads to ambiguity when same inline is included more than once.
					PrepareTemplate(cb, template, field.InlineRecord, tableAliasForColumns, inlineAlias, fieldNameWithPrefix, descentPath);
					descentPath.Pop();
				}
				else
				{
					var fieldAlias = cb.Identifier(fieldNameWithPrefix);
					if (field.SubqueryBuilder == null)
					{
						string fieldColumn = field.DBName;
						var fieldName = cb.Identifier(tableAliasForColumns, fieldColumn);
						template.Column(fieldName, fieldAlias);
					}
					else
					{
						var subqueryExpr = field.SubqueryBuilder(tableAliasForColumns, field, cb);
						template.Subquery(subqueryExpr, fieldAlias);
					}
				}
			}

			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;
				if (!field.IsEagerLookup) continue;

				var eagerMeta = field.ForeignMeta;
				var localName = cb.Identifier(tableAliasForColumns, field.DBName);
				var remoteAlias = tableAliasForJoins + "." + field.Name;
				var remoteKey = eagerMeta.PrimaryKey;
				var remoteName = cb.Identifier(remoteAlias, remoteKey.DBName);
				var remotePrefix = fieldPrefix == null ? field.Name : fieldPrefix + "." + field.Name;

				if (descentPath.Contains(field.ID)) throw new KORMInvalidOperationException("Cyclic eager lookups detected on " + meta.Name + "." + field.Name + ".");
				descentPath.Push(field.ID);
				template.Join(cb.Identifier(eagerMeta.DBSchema, eagerMeta.DBName), cb.Equal(localName, remoteName), cb.Identifier(remoteAlias));
				PrepareTemplate(cb, template, eagerMeta, remoteAlias, remoteAlias, remotePrefix, descentPath);
				descentPath.Pop();
			}
		}

		public IReadOnlyCollection<TRecord> Execute()
		{
			var token = LogStart(dbcommand: Command);
			using var reader = ExecuteReaderNoLog();
			var result = new List<TRecord>();
			while (reader.MoveNext())
			{
				var record = reader.Read();
				result.Add(record);
				LogRecord(LogLevel.Debug, token, record);
			}
			LogEnd(token, result.Count);
			return result;
		}

		public async Task<IReadOnlyCollection<TRecord>> ExecuteAsync()
		{
			var token = LogStart(dbcommand: Command);
			using var reader = await ExecuteReaderAsyncNoLog();
			var result = new List<TRecord>();
			while (await reader.MoveNextAsync())
			{
				var record = reader.Read();
				result.Add(record);
				LogRecord(LogLevel.Debug, token, record);
			}
			LogEnd(token, result.Count);
			return result;
		}

		public IORMReader<TRecord> ExecuteReader()
		{
			var token = LogStart(dbcommand: Command);
			var reader = ExecuteReaderNoLog();
			LogEnd(token);
			return reader;
		}

		private IORMReader<TRecord> ExecuteReaderNoLog()
		{
			var sql = Command.ToString();
			var reader = new DBORMReader<TRecord>(DB, factory, converter, loaderFromReader, sql, Parameters);
			reader.PrepareReader();
			return reader;
		}

		public async Task<IORMReader<TRecord>> ExecuteReaderAsync()
		{
			var token = LogStart(dbcommand: Command);
			var reader = await ExecuteReaderAsyncNoLog();
			LogEnd(token);
			return reader;
		}

		private async Task<IORMReader<TRecord>> ExecuteReaderAsyncNoLog()
		{
			var sql = Command.ToString();
			var reader = new DBORMReader<TRecord>(DB, factory, converter, loaderFromReader, sql, Parameters);
			await reader.PrepareReaderAsync();
			return reader;
		}

		public int ExecuteCount()
		{
			var countCommand = Command.Clone();
			countCommand.ForCount();
			var sql = countCommand.ToString();
			var token = LogStart(countCommand);
			using var dbcommand = DB.CreateCommand(sql);
			DB.AddParameters(dbcommand, Parameters);
			using var dbreader = DB.ExecuteReader(dbcommand);
			try
			{
				if (!dbreader.Read()) return 0;
				var count = dbreader.GetInt32(0);
				return count;
			}
			catch (Exception exc)
			{
				if (DB is Kosson.KORM.DB.ADONETDB adonetdb) adonetdb.HandleException(exc, dbcommand, default);
			}
			finally
			{
				LogEnd(token);
			}
			return 0;
		}

		public async Task<int> ExecuteCountAsync()
		{
			var countCommand = Command.Clone();
			countCommand.ForCount();
			var sql = countCommand.ToString();
			var token = LogStart(countCommand);
			using var dbcommand = DB.CreateCommand(sql);
			DB.AddParameters(dbcommand, Parameters);
			using var dbreader = await DB.ExecuteReaderAsync(dbcommand);
			try
			{
				if (!await dbreader.ReadAsync()) return 0;
				var count = dbreader.GetInt32(0);
				return count;
			}
			catch (Exception exc)
			{
				if (DB is Kosson.KORM.DB.ADONETDB adonetdb) adonetdb.HandleException(exc, dbcommand, default);
			}
			finally
			{
				LogEnd(token);
			}
			return 0;
		}

		public IORMSelect<TRecord> ForUpdate()
		{
			Command.ForUpdate();
			return this;
		}

		public IORMSelect<TRecord> Limit(int limit)
		{
			Command.Limit(limit);
			return this;
		}

		public IORMSelect<TRecord> Where(IDBExpression expression)
		{
			Command.Where(expression);
			return this;
		}

		public IORMSelect<TRecord> Or()
		{
			Command.StartWhereGroup();
			return this;
		}

		public IORMSelect<TRecord> OrderBy(IDBExpression field, bool descending = false)
		{
			Command.OrderBy(field, descending);
			return this;
		}

		public IORMSelect<TRecord> Tag(IDBComment comment)
		{
			Command.Tag(comment);
			return this;
		}

		public IORMSelectAnonymous<TRecord, TResult> Select<TResult>(System.Linq.Expressions.Expression<Func<TRecord, TResult>> selectorExpression)
		{
			var columnAliases = new HashSet<string>();
			var columnPrefixes = new HashSet<string>();

			void ProcessExpression(Expression? expression)
			{
				var path = new List<string>();
				while (expression != null)
				{
					if (expression is ParameterExpression) break;
					if (expression is not MemberExpression memberExpression) throw new InvalidOperationException("Expected simple property expression.");
					if (memberExpression.Member is not PropertyInfo propertyInfo) throw new InvalidOperationException("Expected simple property expression.");
					path.Add(propertyInfo.Name);
					expression = memberExpression.Expression;
				}
				path.Reverse();
				for (int i = 0; i < path.Count; i++)
					columnAliases.Add(String.Join(".", path.Take(i + 1)));
				columnPrefixes.Add(String.Join(".", path) + ".");
			}

			if (selectorExpression.Body is MemberExpression memberExpression) ProcessExpression(memberExpression);
			else if (selectorExpression.Body is ParameterExpression) columnPrefixes.Add("");
			else if (selectorExpression.Body is NewExpression newExpression)
			{
				foreach (var argument in newExpression.Arguments)
				{
					ProcessExpression(argument);
				}
			}
			else throw new InvalidOperationException("Expected \"record => new { record.Property, ... }\" or \"record => record.Property\" expression.");
			Command.RemoveColumns(alias => !columnAliases.Contains(alias) && !columnPrefixes.Any(prefix => alias.StartsWith(prefix)));
			return new DBORMSelectAnonymous<TRecord, TResult>(this, selectorExpression);
		}

		public override string ToString()
		{
			return Command.ToString();
		}
	}
}
