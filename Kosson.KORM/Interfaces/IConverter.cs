using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Component providing best-effort type conversion.
	/// </summary>
	public interface IConverter
	{
		/// <summary>
		/// Converts given value to new type. Throws InvalidCastException if conversion is not possible.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="type">Type to convert the value into.</param>
		/// <returns>Object of given type.</returns>
		object? Convert(object? value, Type type);
	}
}
