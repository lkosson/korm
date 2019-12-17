using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Kosson.KORM.PropertyBinder
{
	/// <summary>
	/// Property binder implementation based on reflection.
	/// </summary>
	class ReflectionPropertyBinder : IPropertyBinder
	{
		private readonly IConverter converter;
		private readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> cache;

		public ReflectionPropertyBinder(IConverter converter)
		{
			this.converter = converter;
			cache = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();
		}

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

		private Dictionary<string, PropertyInfo> GetProperties(object target)
		{
			Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase);
			var type = target.GetType();
			while (type != null)
			{
				var propertyinfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
				foreach (var propertyinfo in propertyinfos)
				{
					var name = propertyinfo.Name;
					if (properties.ContainsKey(name)) continue;
					properties[name] = propertyinfo;
				}
				type = type.BaseType;
			}
			return properties;
		}

		private PropertyInfo GetProperty(object target, string propname)
		{
			var type = target.GetType();
			Dictionary<string, PropertyInfo> properties;
			if (!cache.TryGetValue(type, out properties))
			{
				properties = GetProperties(target);
				cache[type] = properties;
			}

			PropertyInfo property;
			if (!properties.TryGetValue(propname, out property)) throw new ArgumentException("Property \"" + propname + "\" not found in \"" + type + "\".", "expression");
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
			string orgExpr = expression;
			object orgTarget = target;
			AccessTarget(ref target, ref expression);
			if (target == null)
			{
				if (orgExpr == expression)
					throw new NullReferenceException("Null dereference when accesing \"" + value + "\"");
				else
					throw new NullReferenceException("Null dereference when accesing \"" + orgExpr.Substring(0, orgExpr.Length - expression.Length - 1) + "\" on \"" + orgTarget + "\"");
			}
			var property = GetProperty(target, expression);
			var convertedvalue = converter.Convert(value, property.PropertyType);
			property.SetValue(target, convertedvalue);
		}

		Func<TInput, TOutput> IPropertyBinder.BuildGetter<TInput, TOutput>(string expression)
		{
			return DynamicAccessor.BuildAccesor<TInput, TOutput>(expression);
		}
	}
}