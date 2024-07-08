using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace Kosson.KORM.PropertyBinder
{
	/// <summary>
	/// Property binder implementation based on type descriptors.
	/// </summary>
	class DescriptorPropertyBinder : IPropertyBinder
	{
		private ConcurrentDictionary<Type, PropertyDescriptorCollection> cache = new ConcurrentDictionary<Type, PropertyDescriptorCollection>();

		private readonly IConverter converter;

		public DescriptorPropertyBinder(IConverter converter)
		{
			this.converter = converter;
		}

		private void AccessTarget(ref object? target, ref string expression)
		{
			int dot;
			while (target != null && (dot = expression.IndexOf('.')) != -1 && dot != 0)
			{
				var propname = expression.Substring(0, dot);
				var property = GetProperty(target, propname);
				target = property.GetValue(target);
				expression = expression.Substring(dot + 1);
			}
		}

		private PropertyDescriptor GetProperty(object target, string propname)
		{
			var type = target.GetType();
			if (!cache.TryGetValue(type, out var properties))
			{
				properties = TypeDescriptor.GetProperties(target);
				cache[type] = properties;
			}

			return properties.Find(propname, true) ?? throw new ArgumentException("Property \"" + propname + "\" not found in \"" + type + "\".", "expression");
		}

		object? IPropertyBinder.Get(object target, string expression)
		{
			var value = target;
			expression += ".";
			AccessTarget(ref value, ref expression);
			return value;
		}

		void IPropertyBinder.Set(object? target, string expression, object? value)
		{
			AccessTarget(ref target, ref expression);
			var property = GetProperty(target!, expression);
			var convertedvalue = converter.Convert(value, property.PropertyType);
			property.SetValue(target, convertedvalue);
		}

		Func<TInput, TOutput> IPropertyBinder.BuildGetter<TInput, TOutput>(string expression)
		{
			return new BinderAccessor<TInput, TOutput>(this, converter, expression).Get;
		}
	}
}