using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KRUD
{
	class DBExpression : IDBExpression
	{
		private string expression;

		public string RawValue { get { if (expression == null) throw new NotSupportedException(); return expression; } }

		public DBExpression(string expression)
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

	class DBComment : IDBComment
	{
		private string value;

		private IDBCommandBuilder builder;

		public string RawValue { get { return value; } }

		public DBComment(IDBCommandBuilder builder, string value)
		{
			this.builder = builder;
			if (value != null) this.value = value.Replace(builder.CommentDelimiterRight, "");
		}

		public virtual void Append(StringBuilder sb)
		{
			if (value == null) return;
			sb.Append(builder.CommentDelimiterLeft);
			sb.Append(value);
			sb.Append(builder.CommentDelimiterRight);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Append(sb);
			return sb.ToString();
		}
	}

	class DBStringConst : IDBExpression
	{
		private string value;
		private IDBCommandBuilder builder;

		public string RawValue { get { return value; } }

		public DBStringConst(IDBCommandBuilder builder, string value)
		{
			this.builder = builder;
			if (value != null) this.value = value.Replace(builder.StringQuoteLeft, builder.StringQuoteLeft + builder.StringQuoteLeft);
		}

		public virtual void Append(StringBuilder sb)
		{
			if (value == null)
			{
				sb.Append("NULL");
			}
			else
			{
				sb.Append(builder.StringQuoteLeft);
				sb.Append(value);
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
		private byte[] value;

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

	class DBParameter : DBExpression
	{
		private IDBCommandBuilder builder;

		public DBParameter(IDBCommandBuilder builder, string name)
			: base(name)
		{
			this.builder = builder;
		}

		public override void Append(StringBuilder sb)
		{
			sb.Append(builder.ParameterPrefix);
			base.Append(sb);
		}
	}

	class DBArray : DBExpression
	{
		private IDBCommandBuilder builder;
		private IDBExpression[] values;

		public DBArray(IDBCommandBuilder builder, IDBExpression[] values)
			: base(null)
		{
			this.builder = builder;
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

	class DBIdentifier : DBExpression, IDBIdentifier
	{
		private IDBCommandBuilder builder;

		public DBIdentifier(IDBCommandBuilder builder, string identifier)
			: base(identifier)
		{
			if (identifier.Contains(builder.IdentifierQuoteRight)) throw new ArgumentException("Identifier contains invalid character: " + builder.IdentifierQuoteRight, "identifier");
			this.builder = builder;
		}

		public override void Append(StringBuilder sb)
		{
			sb.Append(builder.IdentifierQuoteLeft);
			base.Append(sb);
			sb.Append(builder.IdentifierQuoteRight);
		}
	}

	class DBDottedIdentifier : IDBDottedIdentifier
	{
		private IDBCommandBuilder builder;
		private string[] fragments;

		public string RawValue { get { throw new NotSupportedException(); } }
		public string[] Fragments { get { return fragments; } }

		public DBDottedIdentifier(IDBCommandBuilder builder, string[] fragments)
		{
			foreach (var fragment in fragments)
			{
				if (fragment.Contains(builder.IdentifierQuoteRight)) throw new ArgumentException("Identifier fragment contains invalid character: " + builder.IdentifierQuoteRight, "identifier");
			}
			this.builder = builder;
			this.fragments = fragments;
		}

		public virtual void Append(StringBuilder sb)
		{
			for (int i = 0; i < fragments.Length; i++)
			{
				if (i > 0) sb.Append(builder.IdentifierSeparator);
				sb.Append(builder.IdentifierQuoteLeft);
				sb.Append(fragments[i]);
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

	class DBComparison : IDBExpression
	{
		private IDBCommandBuilder builder;
		private IDBExpression lexpr;
		private IDBExpression rexpr;
		private DBExpressionComparison comparison;

		public string RawValue { get { throw new NotSupportedException(); } }

		public DBComparison(IDBCommandBuilder builder, IDBExpression lexpr, DBExpressionComparison comparison, IDBExpression rexpr)
		{
			if (lexpr == null) throw new ArgumentNullException("lexpr");
			if (rexpr == null && comparison != DBExpressionComparison.Equal && comparison != DBExpressionComparison.NotEqual) throw new ArgumentNullException("rexpr");
			this.builder = builder;
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
