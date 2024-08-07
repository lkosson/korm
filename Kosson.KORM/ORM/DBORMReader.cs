﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Kosson.KORM.ORM
{
	class DBORMReader<TRecord> : IORMReader<TRecord>
		 where TRecord : class, IRecord, new()
	{
		private readonly IDB db;
		private readonly string sql;
		private readonly IEnumerable<object?> parameters;
		private readonly LoaderFromReaderByIndexDelegate<TRecord> loader;
		private readonly IFactory factory;
		private readonly Func<TRecord> constructor;
		private readonly Func<TRecord?> builder;
		private readonly IConverter converter;
		private DbCommand? command;
		private DbDataReader? reader;
		private bool aborted;

		public DBORMReader(IDB db, IFactory factory, IConverter converter, LoaderFromReaderByIndexDelegate<TRecord> loader, string sql, IEnumerable<object?> parameters)
		{
			this.db = db;
			this.sql = sql;
			this.parameters = parameters;
			this.factory = factory;
			this.constructor = (Func<TRecord>)factory.GetConstructor(typeof(TRecord));
			this.converter = converter;
			this.loader = loader;
			this.builder = typeof(IRecordNotifySelect).IsAssignableFrom(typeof(TRecord)) ? BuildRecordWithNotify : BuildRecord;
		}

		[MemberNotNull(nameof(command))]
		private void PrepareCommand()
		{
			command = db.CreateCommand(sql);
			db.AddParameters(command, parameters);
		}

		public void PrepareReader()
		{
			PrepareCommand();
			reader = db.ExecuteReader(command);
		}

		public async Task PrepareReaderAsync()
		{
			PrepareCommand();
			reader = await db.ExecuteReaderAsync(command);
		}

		private TRecord? BuildRecord()
		{
			if (reader == null) ThrowDisposed();
			var record = constructor();
			loader(record, reader, converter, factory);
			return record;
		}

		private TRecord? BuildRecordWithNotify()
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

		[DoesNotReturn]
		private static void ThrowDisposed()
		{
			throw new ObjectDisposedException("Reader already disposed.");
		}

		private IEnumerator<TRecord> BuildEnumerator()
		{
			if (reader == null) ThrowDisposed();
			while (reader.Read())
			{
				var record = builder();
				if (record != null) yield return record;
			}
			reader.Dispose();
			reader = null;
		}

		public void Dispose()
		{
			if (reader == null || command == null) return;
			reader.Dispose();
			reader = null;
			command.Dispose();
			command = null;
		}

		bool IORMReader<TRecord>.MoveNext()
		{
			if (reader == null || command == null) ThrowDisposed();
			try
			{
				var result = !aborted && reader.Read();
				if (!result) Dispose();
				return result;
			}
			catch (Exception exc)
			{
				if (db is Kosson.KORM.DB.ADONETDB adonetdb) adonetdb.HandleException(exc, command, default);
				throw;
			}
		}

		async Task<bool> IORMReader<TRecord>.MoveNextAsync()
		{
			if (reader == null || command == null) ThrowDisposed();
			try
			{
				var result = !aborted && await reader.ReadAsync();
				if (!result) Dispose();
				return result;
			}
			catch (Exception exc)
			{
				if (db is Kosson.KORM.DB.ADONETDB adonetdb) adonetdb.HandleException(exc, command, default);
				throw;
			}
		}

		TRecord IORMReader<TRecord>.Read()
		{
			next:
			var record = builder();
			if (record == null) goto next;
			return record;
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
