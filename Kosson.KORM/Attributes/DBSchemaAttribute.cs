using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Database schema name of a table assigned to the class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class DBSchemaAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets database schema name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Database schema name of a table assigned to the class.
		/// </summary>
		/// <param name="name">Database name for the member.</param>
		public DBSchemaAttribute(string name)
		{
			Name = name;
		}
	}
}
