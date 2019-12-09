using Kosson.Interfaces;
using Kosson.Kore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD.ORM
{
	class DBQuerySelect<TRecord> : DBORMCommandBase<TRecord, IDBSelect>, IORMSelect<TRecord> where TRecord : class, IRecord, new()
	{
		public DBQuerySelect(IDB db) : base(db)
		{
		}

		protected override IDBSelect BuildCommand(IDBCommandBuilder cb)
		{
			var template = cb.Select();
			if (meta.DBQuery == null)
				template.From(cb.Identifier(meta.DBName), cb.Identifier(meta.Name));
			else
				template.FromSubquery(cb.Expression(meta.DBQuery), cb.Identifier(meta.Name));
			PrepareTemplate(cb, template, meta, meta.Name, meta.Name, null, new Stack<long>());
			return template;
		}

		private void PrepareTemplate(IDBCommandBuilder cb, IDBSelect template, IMetaRecord meta, string tableAliasForColumns, string tableAliasForJoins, string fieldPrefix, Stack<long> descentPath)
		{
			// keep order in sync with ReaderRecordLoaderBuilder
			var metaBuilder = KORMContext.Current.MetaBuilder;

			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;

				var fieldNameWithPrefix = fieldPrefix == null ? field.Name : fieldPrefix + "." + field.Name;

				if (field.IsInline)
				{
					if (descentPath.Contains(field.ID)) throw new KRUDInvalidOperationException("Cyclic inlines detected on " + meta.Name + "." + field.Name + ".");
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

				var eagerMeta = metaBuilder.Get(field.Type);
				var localName = cb.Identifier(tableAliasForColumns, field.DBName);
				var remoteAlias = tableAliasForJoins + "." + field.Name;
				var remoteKey = eagerMeta.PrimaryKey;
				var remoteName = cb.Identifier(remoteAlias, remoteKey.DBName);
				var remotePrefix = fieldPrefix == null ? field.Name : fieldPrefix + "." + field.Name;

				if (descentPath.Contains(field.ID)) throw new KRUDInvalidOperationException("Cyclic eager lookups detected on " + meta.Name + "." + field.Name + ".");
				descentPath.Push(field.ID);
				template.Join(cb.Identifier(eagerMeta.DBName), cb.Equal(localName, remoteName), cb.Identifier(remoteAlias));
				PrepareTemplate(cb, template, eagerMeta, remoteAlias, remoteAlias, remotePrefix, descentPath);
				descentPath.Pop();
			}
		}

		public IReadOnlyCollection<TRecord> Execute()
		{
			var sql = command.ToString();
			var rows = DB.ExecuteQuery(sql, Parameters);
			return rows.Load<TRecord>();
		}

		public async Task<IReadOnlyCollection<TRecord>> ExecuteAsync()
		{
			var sql = command.ToString();
			var rows = await DB.ExecuteQueryAsync(sql, Parameters);
			return rows.Load<TRecord>();
		}

		public IORMReader<TRecord> ExecuteReader()
		{
			var sql = command.ToString();
			var reader = new DBORMReader<TRecord>(DB, sql, Parameters);
			reader.PrepareReader();
			return reader;
		}

		public async Task<IORMReader<TRecord>> ExecuteReaderAsync()
		{
			var sql = command.ToString();
			var reader = new DBORMReader<TRecord>(DB, sql, Parameters);
			await reader.PrepareReaderAsync();
			return reader;
		}

		public IORMSelect<TRecord> ForUpdate()
		{
			command.ForUpdate();
			return this;
		}

		public IORMSelect<TRecord> Limit(int limit)
		{
			command.Limit(limit);
			return this;
		}

		public IORMSelect<TRecord> Where(IDBExpression expression)
		{
			command.Where(expression);
			return this;
		}

		public IORMSelect<TRecord> Or()
		{
			command.StartWhereGroup();
			return this;
		}

		public IORMSelect<TRecord> OrderBy(IDBExpression field, bool descending = false)
		{
			command.OrderBy(field, descending);
			return this;
		}

		public IORMSelect<TRecord> Tag(IDBComment comment)
		{
			command.Tag(comment);
			return this;
		}

		public override string ToString()
		{
			return command.ToString();
		}
	}
}
