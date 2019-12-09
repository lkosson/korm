using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Component for high performance object creation of a given type.
	/// </summary>
	public interface IFactory
	{
		/// <summary>
		/// Returns factory method for creating objects of a given type. Each call creates a new instance of an object of a given type.
		/// </summary>
		/// <param name="type">Type of object to construct.</param>
		/// <returns>Delegate to a method for creating an instance of a given type.</returns>
		Func<object> GetConstructor(Type type);
	}
}
