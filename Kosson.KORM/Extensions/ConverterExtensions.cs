namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IConverter.
	/// </summary>
	public static class ConverterExtensions
	{
		/// <summary>
		/// Converts value to a given type using provided IConverter.
		/// </summary>
		/// <typeparam name="T">Type to convert value to.</typeparam>
		/// <param name="converter">Converter to use for value conversion.</param>
		/// <param name="value">Value to convert to given type.</param>
		/// <returns>Value converted to given type.</returns>
		public static T Convert<T>(this IConverter converter, object value)
			=> (T)converter.Convert(value, typeof(T));
	}
}
