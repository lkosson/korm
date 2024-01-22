using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMWrappedReader<TRecord, TResult> : IORMReader<TResult>
		 where TRecord : class, IRecord, new()
	{
		private readonly IORMReader<TRecord> innerReader;
		private readonly Func<TRecord, TResult> converter;

		public DBORMWrappedReader(IORMReader<TRecord> innerReader, Func<TRecord, TResult> converter)
		{
			this.innerReader = innerReader;
			this.converter = converter;
		}

		private IEnumerator<TResult> BuildEnumerator()
		{
			var enumerator = innerReader.GetEnumerator();
			while (enumerator.MoveNext())
			{
				yield return converter(enumerator.Current);
			}
		}

		TResult IORMReader<TResult>.Read() => converter(innerReader.Read());
		IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator() => BuildEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => BuildEnumerator();
		bool IORMReader<TResult>.MoveNext() => innerReader.MoveNext();
		Task<bool> IORMReader<TResult>.MoveNextAsync() => innerReader.MoveNextAsync();
		void IDisposable.Dispose() => innerReader.Dispose();
	}
}
