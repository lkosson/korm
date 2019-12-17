using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IFactory.
	/// </summary>
	public static class FactoryExtensions
	{
		/// <summary>
		/// Creates a new instance of a given type.
		/// </summary>
		/// <param name="factory">Factory to use for instance creation.</param>
		/// <param name="type">Type of the instance to create.</param>
		/// <returns>New instance of a given type.</returns>
		public static object Create(this IFactory factory, Type type)
		{
			var constructor = factory.GetConstructor(type);
			return constructor();
		}

		/// <summary>
		/// Creates a new instance of a given type.
		/// </summary>
		/// <typeparam name="T">Type of the instance to create.</typeparam>
		/// <param name="factory">Factory to use for instance creation.</param>
		/// <returns>New instance of a given type.</returns>
		public static T Create<T>(this IFactory factory) where T : new()
		{
			return (T)factory.Create(typeof(T));
		}
	}
}
