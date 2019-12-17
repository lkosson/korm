using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Adds database index on a table.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class IndexAttribute : Attribute
	{
		/// <summary>
		/// Determines whether index is unique.
		/// </summary>
		public bool IsUnique { get; set; }

		/// <summary>
		/// Gets or sets index name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets columns of the index key. Names can be provided as a column identifiers or property identifiers.
		/// </summary>
		public string[] Fields { get; set; }

		/// <summary>
		/// Gets or sets columns included in index leaf nodes, but not in index key. Names can be provided as a column identifiers or property identifiers.
		/// </summary>
		public string[] IncludedFields { get; set; }

		/// <summary>
		/// Adds database index on a table.
		/// </summary>
		/// <param name="indexName">Database name of the index.</param>
		/// <param name="indexFields">Columns or properties names of the index key.</param>
		public IndexAttribute(string indexName, params string[] indexFields)
		{
			Name = indexName;
			Fields = indexFields;
		}
	}
}
