using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
	/// <summary>
	/// Extension methods for System.Type.
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// Retrieves DB metadata for a given type.
		/// </summary>
		/// <param name="type">Type to retrieve metadata for.</param>
		/// <returns>Database metadata.</returns>
		public static IMetaRecord Meta(this Type type)
		{
			return KORMContext.Current.MetaBuilder.Get(type);
		}
	}
}
