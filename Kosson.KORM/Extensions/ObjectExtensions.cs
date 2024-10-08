﻿using System.Collections.Generic;
using System.Linq;

namespace System
{
	/// <summary>
	/// Extension methods for System.Object.
	/// </summary>
	static class ObjectExtensions
	{
		/// <summary>
		/// Changes a generic type argument of a given delegate to a specific type.
		/// </summary>
		/// <typeparam name="TDelegate">Type of delegate to change.</typeparam>
		/// <param name="target">Delegate to change.</param>
		/// <param name="argumentType">New generic argument of a delegate.</param>
		/// <returns>Delegate with a generic type argument changed.</returns>
		public static TDelegate ChangeDelegateGenericArgument<TDelegate>(this TDelegate target, Type argumentType)
		{
			if (target == null) return target;
			var originalDelegate = (Delegate)(object)target;
			var method = originalDelegate.Method;
			var genericMethod = method.GetGenericMethodDefinition();
			var changedMethod = genericMethod.MakeGenericMethod(argumentType);
			var changedDelegate = changedMethod.CreateDelegate(target.GetType(), originalDelegate.Target);
			return (TDelegate)(object)changedDelegate;
		}
	}
}

namespace Kosson.KORM
{
	public static class ObjectExtensions
	{
		public static bool In<T>(this T value, params T[] values) => values.Contains(value);
		public static bool In<T>(this T value, IEnumerable<T> values) => values.Contains(value);
	}
}