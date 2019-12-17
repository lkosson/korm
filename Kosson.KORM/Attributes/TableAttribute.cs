using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KORM
{
	/// <summary>
	/// Declares type as backed by database table.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class TableAttribute : Attribute
	{
		/// <summary>
		/// Custom query used to retrieve data for the type. Values from columns are mapped to properties with matching names.
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// Prefix to prepend to all automatically generated backing column names of the table.
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// Determines whether primary key value for the backing table has to be provided manually when performing INSERT.
		/// </summary>
		public bool IsManualID { get; set; }

		/// <summary>
		/// Declares type as backed by database table.
		/// </summary>
		/// <param name="tablePrefix">Prefix to prepend to all automatically generated backing column names of the table.</param>
		/// <param name="isManualId">Determines whether primary key value for the backing table has to be provided manually when performing INSERT.</param>
		public TableAttribute(string tablePrefix = null, bool isManualId = false)
		{
			Prefix = tablePrefix;
			IsManualID = isManualId;
		}
	}
}
