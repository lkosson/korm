using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KRUD.CommandBuilder
{
	/// <summary>
	/// Base class for KRUD database commands.
	/// </summary>
	public abstract class DBCommand : IDBCommand
	{
		private IDBCommandBuilder builder;

		/// <summary>
		/// Table name.
		/// </summary>
		protected IDBIdentifier table;

		/// <summary>
		/// Command tag.
		/// </summary>
		protected IDBComment tag;

		/// <inheritdoc/>
		public IDBCommandBuilder Builder { get { return builder; } }

		/// <summary>
		/// Creates new DBCommand using provided builder.
		/// </summary>
		/// <param name="builder">Builder to construct this command.</param>
		protected DBCommand(IDBCommandBuilder builder)
		{
			this.builder = builder;
		}

		/// <summary>
		/// Creates a new DBCommand using provided command as a template.
		/// </summary>
		/// <param name="template">Existing command to clone.</param>
		protected DBCommand(DBCommand template) : this(template.builder)
		{
			table = template.table;
		}

		void IDBCommand.Tag(IDBComment tag)
		{
			this.tag = tag;
		}

		/// <summary>
		/// Constructs command text.
		/// </summary>
		/// <returns>Command text.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			AppendHeader(sb);
			AppendTag(sb);
			AppendCommandText(sb);
			AppendFooter(sb);
			return sb.ToString();
		}

		/// <summary>
		/// Appends command's tag to a StringBuilder.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendTag(StringBuilder sb)
		{
			if (tag == null) return;
			tag.Append(sb);
			AppendCRLF(sb);
		}

		/// <summary>
		/// Appends command text to a StringBuilder.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected abstract void AppendCommandText(StringBuilder sb);

		/// <summary>
		/// Appends new line to a command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendCRLF(StringBuilder sb)
		{
			sb.Append("\r\n");
		}

		/// <summary>
		/// Appends table name to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendTable(StringBuilder sb)
		{
			if (table == null) throw new ArgumentNullException("table");
			table.Append(sb);
		}

		/// <summary>
		/// Appends command header to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendHeader(StringBuilder sb)
		{
		}

		/// <summary>
		/// Appends command footer to command text.
		/// </summary>
		/// <param name="sb">StringBuilder constructing a command text.</param>
		protected virtual void AppendFooter(StringBuilder sb)
		{
		}
	}
}
