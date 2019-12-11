using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.Support
{
	class RecordCloner : IRecordCloner
	{
		private IFactory factory;
		private IMetaBuilder metaBuilder;

		public RecordCloner(IFactory factory, IMetaBuilder metaBuilder)
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
