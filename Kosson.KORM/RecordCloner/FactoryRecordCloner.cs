namespace Kosson.KORM.RecordCloner
{
	class FactoryRecordCloner : IRecordCloner
	{
		private readonly IFactory factory;
		private readonly IMetaBuilder metaBuilder;

		public FactoryRecordCloner(IFactory factory, IMetaBuilder metaBuilder)
		{
			this.factory = factory;
			this.metaBuilder = metaBuilder;
		}

		T? IRecordCloner.Clone<T>(T? source)
			where T : class
		{
			return (T?)CloneImpl(source);
		}

		private object? CloneImpl(object? source)
		{
			if (source == null) return null;
			var clone = factory.Create(source.GetType());
			var fields = metaBuilder.Get(source.GetType()).Fields;
			foreach (var field in fields)
			{
				if (field.Property.GetMethod == null) continue;
				if (field.Property.SetMethod == null) continue;
				var value = field.Property.GetMethod.Invoke(source, null);
				if (field.IsInline)
				{
					value = ((IRecordCloner)this).Clone(value);
				}
				field.Property.SetMethod.Invoke(clone, [value]);
			}
			return clone;
		}
	}
}
