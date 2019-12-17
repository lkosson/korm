using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Component for getting and setting object's property value based on text expression.
	/// </summary>
	public interface IPropertyBinder
	{
		/// <summary>
		/// Gets value of an object property determined by given text expression.
		/// </summary>
		/// <param name="target">Object to retrieve property value from.</param>
		/// <param name="expression">Expression determining property to retrieve.</param>
		/// <returns>Value of the property of the object.</returns>
		object Get(object target, string expression);

		/// <summary>
		/// Sets value of an object property determined by given text expression.
		/// </summary>
		/// <param name="target">Object to set property value on.</param>
		/// <param name="expression">Expression determining property to change.</param>
		/// <param name="value">New value of a property.</param>
		void Set(object target, string expression, object value);

		/// <summary>
		/// Creates a delegate for accessing property determined by given text expression.
		/// </summary>
		/// <typeparam name="TInput">Type of an object passed as an input to a delegate.</typeparam>
		/// <typeparam name="TOutput">Type of the value referenced by expression.</typeparam>
		/// <param name="expression">Expression determining a property to access.</param>
		/// <returns>Delegate accessing a property of an object.</returns>
		Func<TInput, TOutput> BuildGetter<TInput, TOutput>(string expression);
	}
}
