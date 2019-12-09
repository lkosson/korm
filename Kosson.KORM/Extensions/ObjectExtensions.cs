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
