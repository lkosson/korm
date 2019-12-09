using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
	/// <summary>
	/// Extension methods for System.Object.
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>
		/// Converts an object to a given type.
		/// </summary>
		/// <typeparam name="T">Type to convert to.</typeparam>
		/// <param name="value">Object to convert.</param>
		/// <returns>Object of a given type created from given value.</returns>
		public static T ConvertTo<T>(this object value)
		{
			return KORMContext.Current.Converter.Convert<T>(value);
		}

		/// <summary>
		/// Parses string to a given type.
		/// </summary>
		/// <typeparam name="T">Type to parse string to.</typeparam>
		/// <param name="value">String to parse.</param>
		/// <param name="defvalue">Default value to use when string is empty.</param>
		/// <returns>Parsed value of a given type.</returns>
		public static T ParseAs<T>(this string value, T defvalue)
		{
			if (String.IsNullOrEmpty(value)) return defvalue;
			return ConvertTo<T>(value);
		}

		/// <summary>
		/// Gets value of an object property determined by given text expression.
		/// </summary>
		/// <typeparam name="T">Type of value to return.</typeparam>
		/// <param name="target">Object to retrieve property value from.</param>
		/// <param name="expression">Expression determining property to retrieve.</param>
		/// <returns>Value of the property of the object.</returns>
		public static T GetProperty<T>(this object target, string expression)
		{
			return KORMContext.Current.PropertyBinder.Get(target, expression).ConvertTo<T>();
		}

		/// <summary>
		/// Sets value of an object property determined by given text expression.
		/// </summary>
		/// <param name="target">Object to set property value on.</param>
		/// <param name="expression">Expression determining property to change.</param>
		/// <param name="value">New value of a property.</param>
		public static void SetProperty(this object target, string expression, object value)
		{
			KORMContext.Current.PropertyBinder.Set(target, expression, value);
		}

		/// <summary>
		/// Changes a generic type argument of a given delegate to a specific type.
		/// </summary>
		/// <typeparam name="TDelegate">Type of delegate to change.</typeparam>
		/// <param name="target">Delegate to change.</param>
		/// <param name="argumentType">New generic argument of a delegate.</param>
		/// <returns>Delegate with a generic type argument changed.</returns>
		public static TDelegate ChangeDelegateGenericArgument<TDelegate>(this TDelegate target, Type argumentType)
		{
			var originalDelegate = (Delegate)(object)target;
			var method = originalDelegate.Method;
			var originalGenericArgument = method.GetGenericArguments().First();
			var genericMethod = method.GetGenericMethodDefinition();
			var changedMethod = genericMethod.MakeGenericMethod(argumentType);
			var changedDelegate = changedMethod.CreateDelegate(target.GetType(), originalDelegate.Target);
			return (TDelegate)(object)changedDelegate;
		}

		/// <summary>
		/// Calls a generic static method with provided type as a generic argument.
		/// </summary>
		/// <typeparam name="TArgument">Type of first formal argument of a static method to call.</typeparam>
		/// <param name="target">First argument for static method or object to call method on.</param>
		/// <param name="argumentType">Type to use as a generic argument.</param>
		/// <param name="method">Static method to call.</param>
		public static void CallWithGenericArgument<TArgument>(this TArgument target, Type argumentType, Action<TArgument> method)
		{
			var genericMethod = method.Method.GetGenericMethodDefinition();
			var methodToCall = genericMethod.MakeGenericMethod(argumentType);

			var delegateType = typeof(Action<>).MakeGenericType(target.GetType());
			var delegateToCall = methodToCall.CreateDelegate(delegateType, method.Target);

			var helperDelegate = ChangeDelegateGenericArgument(new Action<Delegate, object>(DelegateCaller<object>), argumentType);
			helperDelegate(delegateToCall, target);
		}

		private static void DelegateCaller<T>(Delegate delegateToCall, object argument)
		{
			var action = (Action<T>)delegateToCall;
			var argumentTyped = (T)argument;
			action(argumentTyped);
		}
	}
}
