using System;

namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for IDBCommandBuilder.
	/// </summary>
	public static class CommandBuilderExtensions
	{
		/// <summary>
		/// Constructs an equality comparison expression.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction.</param>
		/// <param name="lexpr">First value to compare.</param>
		/// <param name="rexpr">Second value to compare.</param>
		/// <returns>Comparison expression testing for equality between lexpr and rexpr.</returns>
		public static IDBExpression Equal(this IDBCommandBuilder builder, IDBIdentifier lexpr, IDBExpression? rexpr)
			=> builder.Comparison(lexpr, DBExpressionComparison.Equal, rexpr);

		/// <summary>
		/// Constructs an inequality comparison expression.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction.</param>
		/// <param name="lexpr">First value to compare.</param>
		/// <param name="rexpr">Second value to compare.</param>
		/// <returns>Comparison expression testing for inequality between lexpr and rexpr.</returns>
		public static IDBExpression NotEqual(this IDBCommandBuilder builder, IDBIdentifier lexpr, IDBExpression? rexpr)
			=> builder.Comparison(lexpr, DBExpressionComparison.NotEqual, rexpr);

		/// <summary>
		/// Constructs an expression testing given value for NULL.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction.</param>
		/// <param name="expr">Value to compare to NULL.</param>
		/// <returns>Comparison expression testing expr for NULL.</returns>
		public static IDBExpression IsNull(this IDBCommandBuilder builder, IDBIdentifier expr)
			=> Equal(builder, expr, null);

		/// <summary>
		/// Constructs an expression testing given value for NOT NULL.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction.</param>
		/// <param name="expr">Value to compare to NULL.</param>
		/// <returns>Comparison expression testing expr for NOT NULL.</returns>
		public static IDBExpression IsNotNull(this IDBCommandBuilder builder, IDBIdentifier expr)
			=> NotEqual(builder, expr, null);

		/// <summary>
		/// Constructs an expression representing a given constant value.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction.</param>
		/// <param name="value">Constant value to build expression for.</param>
		/// <returns>Constant expression representing a given value.</returns>
		public static IDBExpression Const(this IDBCommandBuilder builder, object? value)
		{
			if (value == null) return builder.Null();
			if (value is byte byteValue) return builder.Const(byteValue);
			if (value is short shortValue) return builder.Const(shortValue);
			if (value is int intValue) return builder.Const(intValue);
			if (value is long longValue) return builder.Const(longValue);
			if (value is float floatValue) return builder.Const(floatValue);
			if (value is double doubleValue) return builder.Const(doubleValue);
			if (value is decimal decimalValue) return builder.Const(decimalValue);
			if (value is string stringValue) return builder.Const(stringValue);
			if (value is DateTime dateValue) return builder.Const(dateValue);
			if (value is Enum) return builder.Const((int)value);
			if (value is bool boolValue) return builder.Const(boolValue);
			if (value is byte[] blobValue) return builder.Const(blobValue);
			if (value is IHasID idValue) return idValue.ID == 0 ? builder.Null() : builder.Const(idValue.ID);
			throw new InvalidCastException("Type " + value.GetType().Name + " is not supported.");
		}
	}
}
