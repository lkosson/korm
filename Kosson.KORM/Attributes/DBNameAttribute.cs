using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Database identifier assigned to class, property, parameter or method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
	public sealed class DBNameAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets database identifier for an item.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Database identifier assigned to class, property, parameter or method.
		/// </summary>
		/// <param name="name">Database name for the member.</param>
		public DBNameAttribute(string name)
		{
			Name = name;
		}
	}
}
