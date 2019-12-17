using Kosson.KORM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.RecordCloner
{
	class FactoryRecordCloner : IRecordCloner
	{
		private IFactory factory;
		private IMetaBuilder metaBuilder;

		public FactoryRecordCloner(IFactory factory, IMetaBuilder metaBuilder)
		{
			this.factory = factory;
			this.metaBuilder = metaBuilder;
		}

		T IRecordCloner.Clone<T>(T source)
		{
			return (T)CloneImpl(source);
		}

		private object CloneImpl(object source)
		{
			if (source == null) return null;
			var clone = factory.Create(source.GetType());
			var fields = metaBuilder.Get(source.GetType()).Fields;
			foreach (var field in fields)
			{
				var value = field.Property.GetMethod.Invoke(source, null);
				if (field.IsInline)
				{
					value = ((IRecordCloner)this).Clone(value);
				}
				field.Property.SetMethod.Invoke(clone, new[] { value });
			}
			return clone;
		}
	}
}
