using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Adds a foreign key to database column used to store property value. Foreign key is referencing table generated for type of the property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public class ForeignKeyAttribute : Attribute
	{
		/// <summary>
		/// Determines whether foreign key constraint has ON DELETE CASCADE defined.
		/// </summary>
		public bool IsCascade { get; private set; }

		/// <summary>
		/// Determines whether foreign key constraint has ON DELETE SET NULL defined.
		/// </summary>
		public bool IsSetNull { get; private set; }

		/// <summary>
		/// Adds a foreign key to database column used to store property value. Foreign key is referencing table generated for type of the property.
		/// </summary>
		/// <param name="isCascade">Determines whether foreign key constraint has ON DELETE CASCADE defined.</param>
		/// <param name="isSetNull">Determines whether foreign key constraint has ON DELETE SET NULL defined.</param>
		protected ForeignKeyAttribute(bool isCascade, bool isSetNull)
		{
			IsCascade = isCascade;
			IsSetNull = isSetNull;
		}
	}

	/// <summary>
	/// Adds a foreign key to database column used to store property value. Foreign key is referencing table generated for type of the property.
	/// </summary>
	public static class ForeignKey
	{
		/// <summary>
		/// Adds a foreign key to database column used to store property value with ON DELETE CASCADE action. Foreign key is referencing table generated for type of the property.
		/// </summary>
		public sealed class CascadeAttribute : ForeignKeyAttribute
		{
			/// <summary>
			/// Adds a foreign key to database column used to store property value with ON DELETE CASCADE action. Foreign key is referencing table generated for type of the property.
			/// </summary>
			public CascadeAttribute()
				: base(true, false)
			{
			}
		}

		/// <summary>
		/// Adds a foreign key to database column used to store property value with ON DELETE SET NULL action. Foreign key is referencing table generated for type of the property.
		/// </summary>
		public sealed class SetNullAttribute : ForeignKeyAttribute
		{
			/// <summary>
			/// Adds a foreign key to database column used to store property value with ON DELETE SET NULL action. Foreign key is referencing table generated for type of the property.
			/// </summary>
			public SetNullAttribute()
				: base(false, true)
			{
			}
		}

		/// <summary>
		/// Adds a foreign key to database column used to store property value without ON DELETE action. Foreign key is referencing table generated for type of the property.
		/// </summary>
		public sealed class NoneAttribute : ForeignKeyAttribute
		{
			/// <summary>
			/// Adds a foreign key to database column used to store property value without ON DELETE action. Foreign key is referencing table generated for type of the property.
			/// </summary>
			public NoneAttribute()
				: base(false, false)
			{
			}
		}
	}
}
