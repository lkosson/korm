﻿using Kosson.KORM.Meta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IORMCommand providing support for LINQ.
	/// </summary>
	public static class ORMCommandLinqExtensions
	{
		public static TCommand Where<TCommand, TRecord>(this IORMNarrowableCommand<TCommand, TRecord> query, Expression<Func<TRecord, bool>> expression)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord
			=> ProcessExpression(query, expression.Body) switch
			{
				IDBExpression dbExpression => query.Where(dbExpression),
				bool boolExpression => boolExpression ? (TCommand)query : query.Where(query.DB.CommandBuilder.Const(false)),
				var other => throw new NotSupportedException("Unsupported expression result \"" + other + "\".")
			};

		private static object ProcessExpression<TCommand, TRecord>(IORMNarrowableCommand<TCommand, TRecord> query, Expression expression)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord
			=> expression switch
			{
				BinaryExpression binaryExpression => ProcessBinaryExpression(query, binaryExpression),
				MethodCallExpression methodCallExpression => ProcessMethodCallExpression(query, methodCallExpression),
				ConstantExpression constantExpression => ProcessConstantExpression(query, constantExpression),
				ConditionalExpression conditionalExpression => ProcessConditionalExpression(query, conditionalExpression),
				UnaryExpression unaryExpression => ProcessUnaryExpression(query, unaryExpression),
				MemberExpression memberExpression => ProcessMemberExpression(query, memberExpression),
				ParameterExpression parameterExpression => ProcessParameterExpression(query, parameterExpression),
				_ => throw new ArgumentOutOfRangeException(nameof(expression), expression, "Unsupported expression: " + expression)
			};

		private static object ProcessBinaryExpression<TCommand, TRecord>(IORMNarrowableCommand<TCommand, TRecord> query, BinaryExpression binaryExpression)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord
		{
			var left = ProcessExpression(query, binaryExpression.Left);
			var right = ProcessExpression(query, binaryExpression.Right);

			var leftFinal = left is not IDBExpression && left is not PartialIdentifier;
			var rightFinal = right is not IDBExpression && right is not PartialIdentifier;

			if (leftFinal && rightFinal)
			{
				if (binaryExpression.NodeType == ExpressionType.Equal) return left == null ? right == null : left.Equals(right);
				else if (binaryExpression.NodeType == ExpressionType.NotEqual) return left == null ? right != null : !left.Equals(right);
				else if (binaryExpression.NodeType == ExpressionType.GreaterThan && left is IComparable leftComparableGT) return leftComparableGT.CompareTo(right) > 0;
				else if (binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual && left is IComparable leftComparableGTE) return leftComparableGTE.CompareTo(right) >= 0;
				else if (binaryExpression.NodeType == ExpressionType.LessThan && left is IComparable leftComparableLT) return leftComparableLT.CompareTo(right) < 0;
				else if (binaryExpression.NodeType == ExpressionType.LessThanOrEqual && left is IComparable leftComparableLTE) return leftComparableLTE.CompareTo(right) <= 0;
				else if (binaryExpression.NodeType == ExpressionType.AndAlso && left is bool leftBoolAnd && right is bool rightBoolAnd) return leftBoolAnd && rightBoolAnd;
				else if (binaryExpression.NodeType == ExpressionType.OrElse && left is bool leftBoolOr && right is bool rightBoolOr) return leftBoolOr && rightBoolOr;
				else if (binaryExpression.NodeType == ExpressionType.Add && left is int leftIntAdd && right is int rightIntAdd) return leftIntAdd + rightIntAdd;
				else if (binaryExpression.NodeType == ExpressionType.Add && left is long leftLongAdd && right is long rightLongAdd) return leftLongAdd + rightLongAdd;
				else if (binaryExpression.NodeType == ExpressionType.Add && left is decimal leftDecAdd && right is decimal rightDecAdd) return leftDecAdd + rightDecAdd;
				else if (binaryExpression.NodeType == ExpressionType.Subtract && left is int leftIntSubtract && right is int rightIntSubtract) return leftIntSubtract - rightIntSubtract;
				else if (binaryExpression.NodeType == ExpressionType.Subtract && left is long leftLongSubtract && right is long rightLongSubtract) return leftLongSubtract - rightLongSubtract;
				else if (binaryExpression.NodeType == ExpressionType.Subtract && left is decimal leftDecSubtract && right is decimal rightDecSubtract) return leftDecSubtract - rightDecSubtract;
				else if (binaryExpression.NodeType == ExpressionType.Multiply && left is int leftIntMultiply && right is int rightIntMultiply) return leftIntMultiply * rightIntMultiply;
				else if (binaryExpression.NodeType == ExpressionType.Multiply && left is long leftLongMultiply && right is long rightLongMultiply) return leftLongMultiply * rightLongMultiply;
				else if (binaryExpression.NodeType == ExpressionType.Multiply && left is decimal leftDecMultiply && right is decimal rightDecMultiply) return leftDecMultiply * rightDecMultiply;
				else if (binaryExpression.NodeType == ExpressionType.Divide && left is int leftIntDivide && right is int rightIntDivide) return leftIntDivide / rightIntDivide;
				else if (binaryExpression.NodeType == ExpressionType.Divide && left is long leftLongDivide && right is long rightLongDivide) return leftLongDivide / rightLongDivide;
				else if (binaryExpression.NodeType == ExpressionType.Divide && left is decimal leftDecDivide && right is decimal rightDecDivide) return leftDecDivide / rightDecDivide;
				else if (binaryExpression.NodeType == ExpressionType.ArrayIndex && left is Array leftArray && right is int rightInt) return leftArray.GetValue(rightInt);
			}

			left = ConvertToParameterOrField(query, left);
			right = ConvertToParameterOrField(query, right);

			if (leftFinal)
			{
				if (binaryExpression.NodeType == ExpressionType.OrElse && left is bool leftBoolOr) return leftBoolOr ? true : right;
				if (binaryExpression.NodeType == ExpressionType.AndAlso && left is bool leftBoolAnd) return leftBoolAnd ? right : false;
			}

			if (rightFinal)
			{
				if (binaryExpression.NodeType == ExpressionType.OrElse && right is bool rightBoolOr) return rightBoolOr ? true : left;
				if (binaryExpression.NodeType == ExpressionType.AndAlso && right is bool rightBoolAnd) return rightBoolAnd ? left : false;
			}

			if (binaryExpression.NodeType == ExpressionType.Equal) return query.DB.CommandBuilder.Comparison((IDBExpression)left, DBExpressionComparison.Equal, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.NotEqual) return query.DB.CommandBuilder.Comparison((IDBExpression)left, DBExpressionComparison.NotEqual, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.GreaterThan) return query.DB.CommandBuilder.Comparison((IDBExpression)left, DBExpressionComparison.Greater, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual) return query.DB.CommandBuilder.Comparison((IDBExpression)left, DBExpressionComparison.GreaterOrEqual, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.LessThan) return query.DB.CommandBuilder.Comparison((IDBExpression)left, DBExpressionComparison.Less, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.LessThanOrEqual) return query.DB.CommandBuilder.Comparison((IDBExpression)left, DBExpressionComparison.LessOrEqual, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.AndAlso) return query.DB.CommandBuilder.And((IDBExpression)left, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.OrElse) return query.DB.CommandBuilder.Or((IDBExpression)left, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.Add) return query.DB.CommandBuilder.BinaryExpression((IDBExpression)left, DBBinaryOperator.Add, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.Subtract) return query.DB.CommandBuilder.BinaryExpression((IDBExpression)left, DBBinaryOperator.Subtract, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.Multiply) return query.DB.CommandBuilder.BinaryExpression((IDBExpression)left, DBBinaryOperator.Multiply, (IDBExpression)right);
			else if (binaryExpression.NodeType == ExpressionType.Divide) return query.DB.CommandBuilder.BinaryExpression((IDBExpression)left, DBBinaryOperator.Divide, (IDBExpression)right);
			else throw new NotSupportedException("Unsupported binary expression: " + binaryExpression.NodeType);
		}

		private static object ProcessMethodCallExpression<TCommand, TRecord>(IORMNarrowableCommand<TCommand, TRecord> query, MethodCallExpression methodCallExpression)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord
		{
			var target = methodCallExpression.Object == null ? null : ProcessExpression(query, methodCallExpression.Object);
			if (target is IDBExpression) throw new NotSupportedException("Unsupported method call on database value: " + methodCallExpression);
			var arguments = new object[methodCallExpression.Arguments.Count];
			var parameters = methodCallExpression.Method.GetParameters();
			for (int i = 0; i < arguments.Length; i++)
			{
				var parameter = parameters[i];
				var argument = ProcessExpression(query, methodCallExpression.Arguments[i]);
				if (argument != null && !parameter.ParameterType.IsAssignableFrom(argument.GetType())) throw new ArgumentOutOfRangeException(parameter.Name, argument, "Invalid argument in call to " + methodCallExpression.Method.Name + ", expected: " + parameter.ParameterType.Name);
				arguments[i] = argument;
			}
			return methodCallExpression.Method.Invoke(target, arguments);
		}

		private static object ProcessConstantExpression<TCommand, TRecord>(IORMNarrowableCommand<TCommand, TRecord> query, ConstantExpression constantExpression)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord => constantExpression.Value;

		private static object ProcessConditionalExpression<TCommand, TRecord>(IORMNarrowableCommand<TCommand, TRecord> query, ConditionalExpression conditionalExpression)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord => throw new NotImplementedException();

		private static object ProcessUnaryExpression<TCommand, TRecord>(IORMNarrowableCommand<TCommand, TRecord> query, UnaryExpression unaryExpression)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord
		{
			var value = ProcessExpression(query, unaryExpression.Operand);
			if (unaryExpression.NodeType == ExpressionType.Convert) return value is IDBExpression ? value : value is IConvertible convertibleValue ? convertibleValue.ToType(unaryExpression.Type, null) : value;

			if (unaryExpression.NodeType == ExpressionType.Negate && value is int intValue) return -intValue;
			if (unaryExpression.NodeType == ExpressionType.Negate && value is long longValue) return -longValue;
			if (unaryExpression.NodeType == ExpressionType.Negate && value is decimal decimalValue) return -decimalValue;
			if (unaryExpression.NodeType == ExpressionType.Not && value is bool boolValue) return !boolValue;

			var dbExpression = ConvertToParameterOrField(query, value);

			if (unaryExpression.NodeType == ExpressionType.Negate) return query.DB.CommandBuilder.UnaryExpression(dbExpression, DBUnaryOperator.Negate);
			else if (unaryExpression.NodeType == ExpressionType.Not) return query.DB.CommandBuilder.UnaryExpression(dbExpression, DBUnaryOperator.Not);
			else throw new NotSupportedException("Unsupported unary expression: " + unaryExpression);
		}

		private static object ProcessMemberExpression<TCommand, TRecord>(IORMNarrowableCommand<TCommand, TRecord> query, MemberExpression memberExpression)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord
		{
			var subject = memberExpression.Expression == null ? null : ProcessExpression(query, memberExpression.Expression);
			if (memberExpression.Member.Name == nameof(IRecordRef.ID) && typeof(IRecordRef).IsAssignableFrom(memberExpression.Member.DeclaringType)) return subject;
			if (subject is PartialIdentifier partialIdentifier)
			{
				partialIdentifier.Append(memberExpression.Member.Name);
				if (partialIdentifier.Meta != query.Meta) throw new NotSupportedException("Not supported foreign value reference \"" + partialIdentifier + "\".");
				var field = partialIdentifier.Meta.GetField(partialIdentifier.CurrentPath);
				if (field.IsInline) return partialIdentifier;
				if (field.IsEagerLookup) return new PartialIdentifier(field.ForeignMeta, partialIdentifier);
				if (!field.IsFromDB) throw new ArgumentOutOfRangeException(partialIdentifier.ToString(), "Record property is not accessible from database.");
				return partialIdentifier;
			}
			if (memberExpression.Member is FieldInfo fieldInfo) return fieldInfo.GetValue(subject);
			else if (memberExpression.Member is PropertyInfo propertyInfo) return propertyInfo.GetValue(subject);
			else throw new NotSupportedException("Unsupported member expression: " + memberExpression);
		}

		private static IDBExpression ConvertToParameterOrField<TCommand, TRecord>(IORMNarrowableCommand<TCommand, TRecord> query, object value)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord => value switch
			{
				PartialIdentifier partialIdentifier => query.Field(partialIdentifier.FullPath),
				IDBExpression expression => expression,
				null => null,
				_ => query.Parameter(value)
			};

		private static object ProcessParameterExpression<TCommand, TRecord>(IORMNarrowableCommand<TCommand, TRecord> query, ParameterExpression parameterExpression)
			where TCommand : IORMNarrowableCommand<TCommand, TRecord>
			where TRecord : IRecord => new PartialIdentifier(query.Meta);

		class PartialIdentifier
		{
			private readonly List<string> path = [];
			private readonly IMetaRecord meta;
			private readonly PartialIdentifier parent;

			public PartialIdentifier(IMetaRecord meta, PartialIdentifier parent = null)
			{
				this.meta = meta;
				this.parent = parent;
			}

			public IMetaRecord Meta => meta;
			public string CurrentPath => String.Join(".", path);
			public string FullPath => parent == null ? CurrentPath : path.Count == 0 ? parent.FullPath /* direct reference to eager field */ : parent.FullPath + "." + CurrentPath;
			public void Append(string part) => path.Add(part);
			public override string ToString() => FullPath;
		}
	}
}
