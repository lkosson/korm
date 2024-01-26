using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Kosson.KORM
{
	class RecordRefTypeConverter : TypeConverter
	{
		private Func<long, object> constructor;
		private string recordTypeName;

		public RecordRefTypeConverter(Type recordRefType)
		{
			var entityType = recordRefType.GetGenericArguments()[0];
			constructor = (Func<long, object>)GetType().GetMethod(nameof(CreateEntityRef), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(entityType).CreateDelegate(typeof(Func<long, object>));
			recordTypeName = entityType.Name;
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || sourceType == typeof(long) || base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string str)
			{
				var at = str.IndexOf('@');
				if (at >= 0 && str.Substring(0, at) == recordTypeName) str = str.Substring(at + 1);
				if (Int64.TryParse(str, out long id)) return constructor(id);
			}
			else if (value is long id)
			{
				return constructor(id);
			}
			return base.ConvertFrom(context, culture, value);
		}

		private static object CreateEntityRef<TEntity>(long id) where TEntity : IRecord
		{
			return new RecordRef<TEntity>(id);
		}

	}
}
