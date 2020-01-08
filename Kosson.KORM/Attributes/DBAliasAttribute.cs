using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Marks a property as a read-only alias to existing database column.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class DBAliasAttribute : Attribute
	{
		/// <summary>
		/// Base name for aliased property.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Marks a property as a read-only alias to existing database column.
		/// </summary>
		/// <param name="name">Name of aliased property.</param>
		public DBAliasAttribute(string name)
		{
			Name = name;
		}
	}
}
