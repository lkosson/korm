﻿using System;
using System.Text;

namespace Kosson.KORM.DB
{
	class DBExpression
	{
		protected readonly IDBCommandBuilder builder;

		protected DBExpression(IDBCommandBuilder builder)
		{
			this.builder = builder;
		}
	}

	class DBRawExpression : DBExpression, IDBExpression
	{
		private readonly string expression;

		public string RawValue { get { if (expression == null) throw new NotSupportedException(); return expression; } }

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
			base.Append(sb);
		}
	}

	class DBIdentifier : DBRawExpression, IDBIdentifier
	{
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
		public string[] Fragments { get; }

		public DBDottedIdentifier(IDBCommandBuilder builder, string[] fragments)
			: base(builder)
		{
			foreach (var fragment in fragments)
			{
				if (fragment.Contains(builder.IdentifierQuoteRight)) throw new ArgumentException("Identifier fragment contains invalid character: " + builder.IdentifierQuoteRight, "identifier");
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
			sb.Append("((");
			lexpr.Append(sb);
			sb.Append(") ");

			if (rexpr == null)
			{
				if (comparison == DBExpressionComparison.Equal) sb.Append("IS NULL");
				if (comparison == DBExpressionComparison.NotEqual) sb.Append("IS NOT NULL");

			}
			else
			{
				if (comparison == DBExpressionComparison.Equal) sb.Append("=");
				else if (comparison == DBExpressionComparison.NotEqual) sb.Append("<>");
				else if (comparison == DBExpressionComparison.Greater) sb.Append(">");
				else if (comparison == DBExpressionComparison.GreaterOrEqual) sb.Append(">=");
				else if (comparison == DBExpressionComparison.Less) sb.Append("<");
				else if (comparison == DBExpressionComparison.LessOrEqual) sb.Append("<=");
				else if (comparison == DBExpressionComparison.In) sb.Append("IN");

				// Parentheses required for IN operator.
				sb.Append(" (");
				rexpr.Append(sb);
				sb.Append(")");
			}
			sb.Append(")");
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Append(sb);
			return sb.ToString();
		}
	}
}