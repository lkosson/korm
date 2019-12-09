using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kosson.KRUD.RecordLoader
{
	/// <summary>
	/// Default implementation of IRecordLoader based on DynamicMethods generated during runtime.
	/// </summary>
	public class DynamicRecordLoader : IRecordLoader, IDisposable
	{
		private LoaderBuilder LoaderBuilder;
		private ReaderWriterLockSlim rwlock;
		private Dictionary<Type, Delegate> byNameCache;
		private Dictionary<Type, Tuple<Delegate, IMetaRecordField[][]>> byIndexCache;

		/// <summary>
		/// Creates a new DynamicRecordLoader instance.
		/// </summary>
		public DynamicRecordLoader(IMetaBuilder metaBuilder)
		{
			byNameCache = new Dictionary<Type, Delegate>();
			byIndexCache = new Dictionary<Type, Tuple<Delegate, IMetaRecordField[][]>>();
			rwlock = new ReaderWriterLockSlim();
			LoaderBuilder = new LoaderBuilder(metaBuilder);
		}

		/// <inheritdoc/>
		void IDisposable.Dispose()
		{
			if (rwlock != null)
			{
				rwlock.Dispose();
				rwlock = null;
			}
		}

		LoaderByNameDelegate<T> IRecordLoader.GetLoader<T>()
		{
			Type type = typeof(T);
			Delegate loader;

			rwlock.EnterReadLock();
			try
			{
				if (byNameCache.TryGetValue(type, out loader)) return (LoaderByNameDelegate<T>)loader;
			}
			finally
			{
				rwlock.ExitReadLock();
			}

			rwlock.EnterUpgradeableReadLock();
			try
			{
				// na wypadek jakby w międzyczasie został zbudowany
				if (byNameCache.TryGetValue(type, out loader)) return (LoaderByNameDelegate<T>)loader;

				var newloader = LoaderBuilder.BuildByName<T>();

				rwlock.EnterWriteLock();
				try
				{
					byNameCache[type] = newloader;
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

		LoaderByIndexDelegate<T> IRecordLoader.GetLoader<T>(out IMetaRecordField[][] fieldMapping)
		{
			Type type = typeof(T);
			//Delegate loader;
			Tuple<Delegate, IMetaRecordField[][]> loader;

			rwlock.EnterReadLock();
			try
			{
				if (byIndexCache.TryGetValue(type, out loader))
				{
					fieldMapping = loader.Item2;
					return (LoaderByIndexDelegate<T>)loader.Item1;
				}
			}
			finally
			{
				rwlock.ExitReadLock();
			}

			rwlock.EnterUpgradeableReadLock();
			try
			{
				// na wypadek jakby w międzyczasie został zbudowany
				if (byIndexCache.TryGetValue(type, out loader))
				{
					fieldMapping = loader.Item2;
					return (LoaderByIndexDelegate<T>)loader.Item1;
				}

				var fieldMappingList = new List<IMetaRecordField[]>();
				var newloader = LoaderBuilder.BuildByIndex<T>(fieldMappingList);
				fieldMapping = fieldMappingList.ToArray();

				rwlock.EnterWriteLock();
				try
				{
					byIndexCache[type] = new Tuple<Delegate, IMetaRecordField[][]>(newloader, fieldMapping);
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
