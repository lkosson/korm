﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMReader<TRecord> : IORMReader<TRecord>
		 where TRecord : class, IRecord, new()
	{
		private readonly IDB db;
		private readonly string sql;
		private readonly IEnumerable<object> parameters;
		private readonly LoaderFromReaderByIndexDelegate<TRecord> loader;
		private readonly IFactory factory;
		private readonly Func<TRecord> constructor;
		private readonly Func<TRecord> builder;
		private readonly IConverter converter;
		private DbDataReader reader;
		private bool aborted;

		public DBORMReader(IDB db, IFactory factory, IConverter converter, LoaderFromReaderByIndexDelegate<TRecord> loader, string sql, IEnumerable<object> parameters)
		{
			this.db = db;
			this.sql = sql;
			this.parameters = parameters;
			this.factory = factory;
			this.constructor = (Func<TRecord>)factory.GetConstructor(typeof(TRecord));
			this.converter = converter;
			this.loader = loader;
			this.builder = typeof(IRecordNotifySelect).IsAssignableFrom(typeof(TRecord)) ? (Func<TRecord>)BuildRecordWithNotify : BuildRecord;
		}

		public void PrepareReader()
		{
			reader = db.ExecuteReader(sql, parameters);
		}

		public async Task PrepareReaderAsync()
		{
			reader = await db.ExecuteReaderAsync(sql, parameters);
		}

		private TRecord BuildRecord()
		{
			if (reader == null) ThrowDisposed();
			var record = constructor();
			loader(record, reader, converter, factory);
			return record;
		}

		private TRecord BuildRecordWithNotify()
		{
			if (reader == null) ThrowDisposed();
			var record = constructor();

			var notify = (IRecordNotifySelect)record;
			var result = notify.OnSelect(null);
			if (result == RecordNotifyResult.Skip) return null;
			if (result == RecordNotifyResult.Break)
			{
				aborted = true;
				return null;
			}

			loader(record, reader, converter, factory);

			result = notify.OnSelected(null);
			if (result == RecordNotifyResult.Skip) return null;
			if (result == RecordNotifyResult.Break)
			{
				aborted = true;
				return null;
			}
			return record;
		}

		private void ThrowDisposed()
		{
			throw new ObjectDisposedException("Reader already disposed.");
		}

		private IEnumerator<TRecord> BuildEnumerator()
		{
			if (reader == null) ThrowDisposed();
			while (reader.Read())
			{
				yield return builder();
			}
			reader.Dispose();
			reader = null;
		}

		public void Dispose()
		{
			if (reader == null) return;
			reader.Dispose();
			reader = null;
		}

		bool IORMReader<TRecord>.MoveNext()
		{
			if (reader == null) ThrowDisposed();
			var result = aborted ? false : reader.Read();
			if (!result) Dispose();
			return result;
		}

		async Task<bool> IORMReader<TRecord>.MoveNextAsync()
		{
			if (reader == null) ThrowDisposed();
			var result = aborted ? false : await reader.ReadAsync();
			if (!result) Dispose();
			return result;
		}

		TRecord IORMReader<TRecord>.Read()
		{
			return builder();
		}

		IEnumerator<TRecord> IEnumerable<TRecord>.GetEnumerator()
		{
			return BuildEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return BuildEnumerator();
		}
	}
}
