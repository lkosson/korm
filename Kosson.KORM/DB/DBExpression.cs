﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM.DB
{
	abstract class DBExpression
	{
		protected readonly IDBCommandBuilder builder;
		public abstract bool IsSimple { get; }

		protected DBExpression(IDBCommandBuilder builder)
		{
			this.builder = builder;
		}
	}

	class DBRawExpression : DBExpression, IDBExpression
	{
		private readonly string expression;

		public string RawValue { get { if (expression == null) throw new NotSupportedException(); return expression; } }
		public override bool IsSimple => false;

		public DBRawExpression(IDBCommandBuilder builder, string expression)
			: base(builder)
		{
			this.expression = expression;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Append(sb);
			return sb.ToString();
		}

		public virtual void Append(StringBuilder sb)
		{
			sb.Append(expression);
		}
	}

	class DBComment : DBExpression, IDBComment
	{
		public string RawValue { get; }
		public override bool IsSimple => true;

		public DBComment(IDBCommandBuilder builder, string value)
			: base(builder)
		{
			if (value != null) RawValue = value.Replace(builder.CommentDelimiterRight, "");
		}

		public virtual void Append(StringBuilder sb)
		{
			if (RawValue == null) return;
			sb.Append(builder.CommentDelimiterLeft);
			sb.Append(RawValue);
			sb.Append(builder.CommentDelimiterRight);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Append(sb);
			return sb.ToString();
		}
	}

	class DBStringConst : DBExpression, IDBExpression
	{
		public string RawValue { get; }
		public override bool IsSimple => true;

		public DBStringConst(IDBCommandBuilder builder, string value)
			: base(builder)
		{
			if (value != null) this.RawValue = value.Replace(builder.StringQuoteLeft, builder.StringQuoteLeft + builder.StringQuoteLeft);
		}

		public virtual void Append(StringBuilder sb)
		{
			if (RawValue == null)
			{
				sb.Append("NULL");
			}
			else
			{
				sb.Append(builder.StringQuoteLeft);
				sb.Append(RawValue);
				sb.Append(builder.StringQuoteRight);
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Append(sb);
			return sb.ToString();
		}
	}

	class DBBlobConst : IDBExpression
	{
		private readonly byte[] value;

		public string RawValue { get { throw new NotImplementedException(); } }
		public bool IsSimple => true;

		public DBBlobConst(byte[] value)
		{
			if (value == null) throw new ArgumentNullException("value");
			this.value = value;
		}

		public void Append(StringBuilder sb)
		{
			sb.Append("0x");
			for (int i = 0; i < value.Length; i++)
			{
				var v = value[i];
				sb.Append(HexDigit(v / 16));
				sb.Append(HexDigit(v % 16));
			}
		}

		private static char HexDigit(int b)
		{
			if (b < 10) return (char)(b + '0');
			return (char)('A' + (b - 10));
		}
	}

	class DBParameter : DBRawExpression
	{
		public override bool IsSimple => true;

		public DBParameter(IDBCommandBuilder builder, string name)
			: base(builder, name)
		{
		}

		public override void Append(StringBuilder sb)
		{
			sb.Append(builder.ParameterPrefix);
			base.Append(sb);
		}
	}

	class DBArray : DBRawExpression
	{
		private readonly IDBExpression[] values;
		public override bool IsSimple => true;

		public DBArray(IDBCommandBuilder builder, IDBExpression[] values)
			: base(builder, null)
		{
			this.values = values;
		}

		public override void Append(StringBuilder sb)
		{
			for (int i = 0; i < values.Length; i++)
			{
				if (i > 0) sb.Append(builder.ArrayElementSeparator);
				values[i].Append(sb);
			}
		}
	}

	class DBCompoundCondition : DBRawExpression
	{
		private readonly string conditionOperator;
		private readonly IDBExpression[] conditions;
		public override bool IsSimple => false;

		public DBCompoundCondition(IDBCommandBuilder builder, string conditionOperator, IDBExpression[] conditions)
			: base(builder, null)
		{
			this.conditionOperator = conditionOperator;
			this.conditions = conditions;
		}

		public override void Append(StringBuilder sb)
		{
			for (int i = 0; i < conditions.Length; i++)
			{
				if (i > 0) sb.Append(conditionOperator);
				var condition = conditions[i];
				if (condition is DBCompoundCondition) sb.Append(builder.ConditionParenthesisLeft);
				condition.Append(sb);
				if (condition is DBCompoundCondition) sb.Append(builder.ConditionParenthesisRight);
			}
		}
	}

	class DBIdentifier : DBRawExpression, IDBIdentifier
	{
		public override bool IsSimple => true;

		public DBIdentifier(IDBCommandBuilder builder, string identifier)
			: base(builder, identifier)
		{
			if (identifier.Contains(builder.IdentifierQuoteRight)) throw new ArgumentException("Identifier contains invalid character: " + builder.IdentifierQuoteRight, "identifier");
		}

		public override void Append(StringBuilder sb)
		{
			sb.Append(builder.IdentifierQuoteLeft);
			base.Append(sb);
			sb.Append(builder.IdentifierQuoteRight);
		}
	}

	class DBDottedIdentifier : DBExpression, IDBDottedIdentifier
	{
		public string RawValue { get { throw new NotSupportedException(); } }
		public override bool IsSimple => true;
		public string[] Fragments { get; }

		public DBDottedIdentifier(IDBCommandBuilder builder, string[] fragments)
			: base(builder)
		{
			var hasNull = false;
			foreach (var fragment in fragments)
			{
				if (fragment == null) hasNull = true;
				else if (fragment.Contains(builder.IdentifierQuoteRight)) throw new ArgumentException("Identifier fragment contains invalid character: " + builder.IdentifierQuoteRight, "identifier");
			}
			if (hasNull)
			{
				var nonNullFragments = new List<string>(fragments.Length - 1);
				foreach (var fragment in fragments)
				{
					if (fragment == null) continue;
					nonNullFragments.Add(fragment);
				}
				fragments = nonNullFragments.ToArray();
			}
			this.Fragments = fragments;
		}

		public virtual void Append(StringBuilder sb)
		{
			for (int i = 0; i < Fragments.Length; i++)
			{
				if (i > 0) sb.Append(builder.IdentifierSeparator);
				sb.Append(builder.IdentifierQuoteLeft);
				sb.Append(Fragments[i]);
				sb.Append(builder.IdentifierQuoteRight);
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Append(sb);
			return sb.ToString();
		}
	}

	class DBComparison : DBExpression, IDBExpression
	{
		private readonly IDBExpression lexpr;
		private readonly IDBExpression rexpr;
		private readonly DBExpressionComparison comparison;

		public string RawValue { get { throw new NotSupportedException(); } }
		public override bool IsSimple => false;

		public DBComparison(IDBCommandBuilder builder, IDBExpression lexpr, DBExpressionComparison comparison, IDBExpression rexpr)
			: base(builder)
		{
			if (lexpr == null) throw new ArgumentNullException("lexpr");
			if (rexpr == null && comparison != DBExpressionComparison.Equal && comparison != DBExpressionComparison.NotEqual) throw new ArgumentNullException("rexpr");
			this.lexpr = lexpr;
			this.rexpr = rexpr;
			this.comparison = comparison;
		}

		public virtual void Append(StringBuilder sb)
		{
			if (!lexpr.IsSimple) sb.Append(builder.ConditionParenthesisLeft);
			lexpr.Append(sb);
			if (!lexpr.IsSimple) sb.Append(builder.ConditionParenthesisRight);

			if (rexpr == null)
			{
				if (comparison == DBExpressionComparison.Equal) sb.Append(" IS NULL");
				if (comparison == DBExpressionComparison.NotEqual) sb.Append(" IS NOT NULL");
			}
			else
			{
				if (comparison == DBExpressionComparison.Equal) sb.Append(" = ");
				else if (comparison == DBExpressionComparison.NotEqual) sb.Append(" <> ");
				else if (comparison == DBExpressionComparison.Greater) sb.Append(" > ");
				else if (comparison == DBExpressionComparison.GreaterOrEqual) sb.Append(" >= ");
				else if (comparison == DBExpressionComparison.Less) sb.Append(" < ");
				else if (comparison == DBExpressionComparison.LessOrEqual) sb.Append(" <= ");
				else if (comparison == DBExpressionComparison.In) sb.Append(" IN (");
				else if (comparison == DBExpressionComparison.Like) sb.Append(" LIKE ");
				else throw new ArgumentOutOfRangeException("comparison", comparison, "Unsupported comparison type.");

				if (!rexpr.IsSimple) sb.Append(builder.ConditionParenthesisLeft);
				rexpr.Append(sb);
				if (!rexpr.IsSimple) sb.Append(builder.ConditionParenthesisRight);

				if (comparison == DBExpressionComparison.In) sb.Append(")");
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Append(sb);
			return sb.ToString();
		}
	}

	class DBUnaryExpression : DBExpression, IDBExpression
	{
		private readonly IDBExpression expr;
		private readonly DBUnaryOperator op;

		public string RawValue { get { throw new NotSupportedException(); } }
		public override bool IsSimple => false;

		public DBUnaryExpression(IDBCommandBuilder builder, IDBExpression expr, DBUnaryOperator op)
			: base(builder)
		{
			if (expr == null) throw new ArgumentNullException("expr");
			this.expr = expr;
			this.op = op;
		}

		public virtual void Append(StringBuilder sb)
		{
			if (op == DBUnaryOperator.Not) sb.Append("NOT");
			else if (op == DBUnaryOperator.Negate) sb.Append("-");
			else throw new ArgumentOutOfRangeException("op", op, "Unsupported operator type.");

			if (!expr.IsSimple) sb.Append(builder.ConditionParenthesisLeft);
			expr.Append(sb);
			if (!expr.IsSimple) sb.Append(builder.ConditionParenthesisRight);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Append(sb);
			return sb.ToString();
		}
	}

	class DBBinaryExpression : DBExpression, IDBExpression
	{
		private readonly IDBExpression lexpr;
		private readonly IDBExpression rexpr;
		private readonly DBBinaryOperator op;

		public string RawValue { get { throw new NotSupportedException(); } }
		public override bool IsSimple => false;

		public DBBinaryExpression(IDBCommandBuilder builder, IDBExpression lexpr, DBBinaryOperator op, IDBExpression rexpr)
			: base(builder)
		{
			if (lexpr == null) throw new ArgumentNullException("lexpr");
			if (rexpr == null) throw new ArgumentNullException("rexpr");
			this.lexpr = lexpr;
			this.rexpr = rexpr;
			this.op = op;
		}

		public virtual void Append(StringBuilder sb)
		{
			if (!lexpr.IsSimple) sb.Append(builder.ConditionParenthesisLeft);
			lexpr.Append(sb);
			if (!lexpr.IsSimple) sb.Append(builder.ConditionParenthesisRight);

			if (op == DBBinaryOperator.Add) sb.Append(" + ");
			else if (op == DBBinaryOperator.Subtract) sb.Append(" - ");
			else if (op == DBBinaryOperator.Multiply) sb.Append(" * ");
			else if (op == DBBinaryOperator.Divide) sb.Append(" / ");
			else throw new ArgumentOutOfRangeException("op", op, "Unsupported operator type.");

			if (!rexpr.IsSimple) sb.Append(builder.ConditionParenthesisLeft);
			rexpr.Append(sb);
			if (!rexpr.IsSimple) sb.Append(builder.ConditionParenthesisRight);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Append(sb);
			return sb.ToString();
		}
	}
}
