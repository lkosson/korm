using System;
using System.Collections.Generic;
using System.Threading;

namespace Kosson.KORM.ORM
{
	class ReaderRecordLoaderCache
	{
		private readonly ReaderRecordLoaderBuilder builder;
		private readonly ReaderWriterLockSlim rwlock;
		private readonly Dictionary<Type, Delegate> cache;

		public ReaderRecordLoaderCache(IMetaBuilder metaBuilder)
		{
			builder = new ReaderRecordLoaderBuilder(metaBuilder);
			cache = [];
			rwlock = new ReaderWriterLockSlim();
		}

		public LoaderFromReaderByIndexDelegate<T> GetLoader<T>() where T : IRecord
		{
			Type type = typeof(T);
			Delegate? loader;

			rwlock.EnterReadLock();
			try
			{
				if (cache.TryGetValue(type, out loader)) return (LoaderFromReaderByIndexDelegate<T>)loader;
			}
			finally
			{
				rwlock.ExitReadLock();
			}

			rwlock.EnterUpgradeableReadLock();
			try
			{
				// In case it got constructed by other thread
				if (cache.TryGetValue(type, out loader)) return (LoaderFromReaderByIndexDelegate<T>)loader;

				var newloader = builder.Build<T>();

				rwlock.EnterWriteLock();
				try
				{
					cache[type] = newloader;
				}
				finally
				{
					rwlock.ExitWriteLock();
				}
				return newloader;
			}
			finally
			{
				rwlock.ExitUpgradeableReadLock();
			}
		}
	}
}
