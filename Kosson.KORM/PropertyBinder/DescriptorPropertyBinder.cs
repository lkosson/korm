﻿using Kosson.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.Kore.PropertyBinder
{
	/// <summary>
	/// Property binder implementation based on type descriptors.
	/// </summary>
	public class DescriptorPropertyBinder : IPropertyBinder
	{
		private ConcurrentDictionary<Type, PropertyDescriptorCollection> cache = new ConcurrentDictionary<Type, PropertyDescriptorCollection>();

		private void AccessTarget(ref object target, ref string expression)
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
			PropertyDescriptorCollection properties;
			if (!cache.TryGetValue(type, out properties))
			{
				properties = TypeDescriptor.GetProperties(target);
				cache[type] = properties;
			}

			var property = properties.Find(propname, true);
			if (property == null) throw new ArgumentException("Property \"" + propname + "\" not found in \"" + type + "\".", "expression");
			return property;
		}

		object IPropertyBinder.Get(object target, string expression)
		{
			expression += ".";
			AccessTarget(ref target, ref expression);
			return target;
		}

		void IPropertyBinder.Set(object target, string expression, object value)
		{
			AccessTarget(ref target, ref expression);
			var property = GetProperty(target, expression);
			var convertedvalue = KORMContext.Current.Converter.Convert(value, property.PropertyType);
			property.SetValue(target, convertedvalue);
		}

		Func<TInput, TOutput> IPropertyBinder.BuildGetter<TInput, TOutput>(string expression)
		{
			var acc = new BinderAccessor<TInput, TOutput>(expression);
			return acc.Get;
		}
	}
}