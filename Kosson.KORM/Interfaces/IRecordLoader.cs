namespace Kosson.Interfaces
{
	/// <summary>
	/// Provides support for creating record of a given type from IRow containing values of record properties.
	/// </summary>
	public interface IRecordLoader
	{
		/// <summary>
		/// Creates a delegate to fill existing record from IRow using dot-expression for properties as a keys to lookup values in provided row.
		/// </summary>
		/// <typeparam name="T">Type of record to fill.</typeparam>
		/// <returns>Delegate for filling record values based on a given row.</returns>
		LoaderByNameDelegate<T> GetLoader<T>() where T : class;

		/// <summary>
		/// Creates a delegate to fill existing record from IIndexBasedRow using int-based indices as keys to lookup values in provided row and providing
		/// mapping between record properties and row indices.
		/// </summary>
		/// <typeparam name="T">Type of record to fill.</typeparam>
		/// <param name="fieldMapping">Array of dot-expressions represented by array of meta-fields providing mapping between row indices (array index) and record properties (array values).</param>
		/// <returns>Delegate for filling record values based on a given row.</returns>
		LoaderByIndexDelegate<T> GetLoader<T>(out IMetaRecordField[][] fieldMapping) where T : class;
	}

	/// <summary>
	/// Method for filling given record from values provided in IRow.
	/// </summary>
	/// <typeparam name="T">Type of record to full.</typeparam>
	/// <param name="target">Record to fill.</param>
	/// <param name="source">Row with values to copy to record.</param>
	/// <param name="converter">Converter to use for conversion between given values and actual record properties' types.</param>
	/// <param name="factory">Factory to use for constructing foreign key records and inline objects.</param>
	public delegate void LoaderByNameDelegate<T>(T target, IRow source, IConverter converter, IFactory factory);

	/// <summary>
	/// Method for filling given record from values provided in IIndexBasedRow.
	/// </summary>
	/// <typeparam name="T">Type of record to full.</typeparam>
	/// <param name="target">Record to fill.</param>
	/// <param name="source">Row with values to copy to record.</param>
	/// <param name="converter">Converter to use for conversion between given values and actual record properties' types.</param>
	/// <param name="factory">Factory to use for constructing foreign key records and inline objects.</param>
	public delegate void LoaderByIndexDelegate<T>(T target, IIndexBasedRow source, IConverter converter, IFactory factory);
}
