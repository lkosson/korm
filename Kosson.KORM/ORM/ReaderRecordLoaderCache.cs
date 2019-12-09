using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kosson.KRUD.ORM
{
	class ReaderRecordLoaderCache
	{
		private ReaderWriterLockSlim rwlock;
		private Dictionary<Type, Delegate> cache;

		public ReaderRecordLoaderCache()
		{
			cache = new Dictionary<Type, Delegate>();
			rwlock = new ReaderWriterLockSlim();
		}

		public LoaderFromReaderByIndexDelegate<T> GetLoader<T>() where T : IRecord
		{
			Type type = typeof(T);
			Delegate loader;

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

				var newloader = new ReaderRecordLoaderBuilder<T>().Build();

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
