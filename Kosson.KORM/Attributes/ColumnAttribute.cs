using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Marks a property as backed by database column. It will be stored and retrieved from database using KRUD ORM operations.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public class ColumnAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets read-only flag of a property. Read-only properties are not stored using INSERT or UPDATE operations, but are retrieved using SELECT.
		/// </summary>
		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Gets or sets database type used for backing column of a property. Default value of DbType.Object leaves database type selection to a KRUD provider.
		/// Value is ignored if custom backing type is provided.
		/// </summary>
		public DbType DBType { get; set; }

		/// <summary>
		/// Determines whether column is not nullable.
		/// </summary>
		public bool IsNotNull { get; set; }

		/// <summary>
		/// Determines whether column has a default value based on value assigned in object constructor.
		/// </summary>
		public bool HasDefaultValue { get; set; }

		/// <summary>
		/// Gets or sets maximum data length of a column.
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		/// Gets or sets precision of a data stored in a column.
		/// </summary>
		public int Precision { get; set; }

		/// <summary>
		/// Determines whether values exceeding declared length should be trimmed to fit the column before being passed to database provider.
		/// </summary>
		public bool Trim { get; set; }

		/// <summary>
		/// Gets or sets database engine-specific data type for backing column of a property.
		/// </summary>
		public string ColumnDefinition { get; set; }

		/// <summary>
		/// Marks a property as backed by database column. It will be stored and retrieved from database using KRUD ORM operations.
		/// </summary>
		/// <param name="type">Database type used for backing column for the property.</param>
		public ColumnAttribute(DbType type = DbType.Object)
		{
			DBType = type;
		}

		/// <summary>
		/// Marks a property as backed by database column. It will be stored and retrieved from database using KRUD ORM operations.
		/// </summary>
		/// <param name="length">Maximum data length of a column.</param>
		/// <param name="precision">Precision of data stored in a column.</param>
		public ColumnAttribute(int length, int precision = 0)
		{
			DBType = DbType.Object;
			Length = length;
			Precision = precision;
		}

		/// <summary>
		/// Marks a property as backed by database column with a custom definition. It will be stored and retrieved from database using KRUD ORM operations.
		/// </summary>
		/// <param name="columnDefinition">Database engine-specific column definition to use for backing column.</param>
		public ColumnAttribute(string columnDefinition)
		{
			ColumnDefinition = columnDefinition;
		}
	}
}
