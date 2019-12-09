using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
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
		object Convert(object value, Type type);
	}
}
