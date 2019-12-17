using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kosson.KORM.Meta
{
	class ReflectionMetaBuilder : IMetaBuilder
	{
		private IFactory factory;
		private object syncroot;
		private Dictionary<Type, IMetaRecord> cache;

		public ReflectionMetaBuilder(IFactory factory)
		{
			this.factory = factory;
			syncroot = new object();
			cache = new Dictionary<Type, IMetaRecord>();
		}

		IMetaRecord IMetaBuilder.Get(Type type)
		{
			IMetaRecord meta;

			if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(RecordRef<>))
			{
				type = type.GetGenericArguments()[0];
			}

			lock (syncroot)
			{
				if (cache.TryGetValue(type, out meta)) return meta;
			}

			var newmeta = new MetaRecord(factory, type);

			lock (syncroot)
			{
				// Potentially two metas for one type can be built at the same time.
				if (cache.TryGetValue(type, out meta)) return meta;
				cache[type] = newmeta;
			}
			return newmeta;
		}
	}
}
