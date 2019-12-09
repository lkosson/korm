using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Kosson.Interfaces
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
		public static IDBExpression Equal(this IDBCommandBuilder builder, IDBExpression lexpr, IDBExpression rexpr)
		{
			return builder.Comparison(lexpr, DBExpressionComparison.Equal, rexpr);
		}

		/// <summary>
		/// Constructs an inequality comparison expression.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction.</param>
		/// <param name="lexpr">First value to compare.</param>
		/// <param name="rexpr">Second value to compare.</param>
		/// <returns>Comparison expression testing for inequality between lexpr and rexpr.</returns>
		public static IDBExpression NotEqual(this IDBCommandBuilder builder, IDBExpression lexpr, IDBExpression rexpr)
		{
			return builder.Comparison(lexpr, DBExpressionComparison.NotEqual, rexpr);
		}

		/// <summary>
		/// Constructs an expression testing given value for NULL.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction.</param>
		/// <param name="expr">Value to compare to NULL.</param>
		/// <returns>Comparison expression testing expr for NULL.</returns>
		public static IDBExpression IsNull(this IDBCommandBuilder builder, IDBExpression expr)
		{
			return Equal(builder, expr, null);
		}

		/// <summary>
		/// Constructs an expression testing given value for NOT NULL.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction.</param>
		/// <param name="expr">Value to compare to NULL.</param>
		/// <returns>Comparison expression testing expr for NOT NULL.</returns>
		public static IDBExpression IsNotNull(this IDBCommandBuilder builder, IDBExpression expr)
		{
			return NotEqual(builder, expr, null);
		}

		/// <summary>
		/// Constructs an expression representing a given constant value.
		/// </summary>
		/// <param name="builder">Builder to use for expression construction.</param>
		/// <param name="value">Constant value to build expression for.</param>
		/// <returns>Constant expression representing a given value.</returns>
		public static IDBExpression Const(this IDBCommandBuilder builder, object value)
		{
			if (value == null) return builder.Null();
			if (value is byte) return builder.Const((byte)value);
			if (value is short) return builder.Const((short)value);
			if (value is int) return builder.Const((int)value);
			if (value is long) return builder.Const((long)value);
			if (value is float) return builder.Const((float)value);
			if (value is double) return builder.Const((double)value);
			if (value is decimal) return builder.Const((decimal)value);
			if (value is string) return builder.Const((string)value);
			if (value is DateTime) return builder.Const((DateTime)value);
			if (value is Enum) return builder.Const((int)value);
			if (value is bool) return builder.Const((bool)value ? 1 : 0);
			if (value is byte[]) return builder.Const((byte[])value);
			if (value is IHasID)
			{
				var id = ((IHasID)value).ID;
				if (id == 0)
					return builder.Null();
				else
					return builder.Const(id);
			}
			throw new InvalidCastException("Type " + value.GetType().Name + " is not supported.");
		}
	}
}
