using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KORM
{
	/// <summary>
	/// Marks property as inlined. Properties declared in type of this property are mapped to columns as thought they were declared directly in table type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class InlineAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets prefix prepended to database identifiers of properties of type of this property.
		/// </summary>
		public string Prefix { get; private set; }

		/// <summary>
		/// Marks property as inlined. Properties declared in type of this property are mapped to columns as thought they were declared directly in table type.
		/// </summary>
		/// <param name="prefix">Prefix prepended to database identifiers of properties of type of this property.</param>
		public InlineAttribute(string prefix = null)
		{
			Prefix = prefix;
		}
	}
}
