using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMSelectAnonymous<TRecord, TResult> : IORMSelectAnonymous<TRecord, TResult>
		where TRecord : class, IRecord, new()
	{
		private readonly DBORMSelect<TRecord> fullSelect;
		private readonly Expression<Func<TRecord, TResult>> selectorExpression;

		IORMSelect<TRecord> IORMSelectAnonymous<TRecord, TResult>.OriginalSelect => fullSelect;

		public DBORMSelectAnonymous(DBORMSelect<TRecord> fullSelect, Expression<Func<TRecord, TResult>> selectorExpression)
		{
			this.fullSelect = fullSelect;
			this.selectorExpression = selectorExpression;
		}

		public IReadOnlyCollection<TResult> Execute()
		{
			var selector = selectorExpression.Compile();
			var records = fullSelect.Execute();
			return records.Select(selector).ToList();
		}

		public async Task<IReadOnlyCollection<TResult>> ExecuteAsync()
		{
			var selector = selectorExpression.Compile();
			var records = await fullSelect.ExecuteAsync();
			return records.Select(selector).ToList();
		}

		public IORMReader<TResult> ExecuteReader()
		{
			var selector = selectorExpression.Compile();
			var reader = fullSelect.ExecuteReader();
			return new DBORMWrappedReader<TRecord, TResult>(reader, selector);
		}

		public async Task<IORMReader<TResult>> ExecuteReaderAsync()
		{
			var selector = selectorExpression.Compile();
			var reader = await fullSelect.ExecuteReaderAsync();
			return new DBORMWrappedReader<TRecord, TResult>(reader, selector);
		}
	}
}
