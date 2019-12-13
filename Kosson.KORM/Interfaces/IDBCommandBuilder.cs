using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Factory for creation of database commands in a dialect understood by a given database engine.
	/// </summary>
	public interface IDBCommandBuilder
	{
		/// <summary>
		/// Prefix character used to denote database command parameters.
		/// </summary>
		string ParameterPrefix { get; }

		/// <summary>
		/// Left delimiter (prefix) of a column, table or other database object identifier.
		/// </summary>
		string IdentifierQuoteLeft { get; }

		/// <summary>
		/// Right delimiter (suffix) of a column, table or other database object identifier.
		/// </summary>
		string IdentifierQuoteRight { get; }

		/// <summary>
		/// Separator used for delimiting multipart identifiers in fully-qualified database object name.
		/// </summary>
		string IdentifierSeparator { get; }

		/// <summary>
		/// Left delimiter (prefix) of a constant string value.
		/// </summary>
		string StringQuoteLeft { get; }

		/// <summary>
		/// Right delimiter (suffix) of a constant string value.
		/// </summary>
		string StringQuoteRight { get; }

		/// <summary>
		/// Left delimiter (prefix) of a comment text.
		/// </summary>
		string CommentDelimiterLeft { get; }

		/// <summary>
		/// Right delimiter (suffix) of a comment text.
		/// </summary>
		string CommentDelimiterRight { get; }

		/// <summary>
		/// Separator character between elements of array of "field IN (value1, ..., valueN)" expression.
		/// </summary>
		string ArrayElementSeparator { get; }

		/// <summary>
		/// Determines whether database supports inserting rows with user-provided primary key (ID) value.
		/// </summary>
		bool SupportsPrimaryKeyInsert { get; }

		/// <summary>
		/// Creates a new SELECT command builder for a given database dialect.
		/// </summary>
		/// <returns>New SELECT command builder.</returns>
		IDBSelect Select();

		/// <summary>
		/// Creates a new UPDATE command builder for a given database dialect.
		/// </summary>
		/// <returns>New UPDATE command builder.</returns>
		IDBUpdate Update();

		/// <summary>
		/// Creates a new DELETE command builder for a given database dialect.
		/// </summary>
		/// <returns>New DELETE command builder.</returns>
		IDBDelete Delete();

		/// <summary>
		/// Creates a new INSERT command builder for a given database dialect.
		/// </summary>
		/// <returns>New INSERT command builder.</returns>
		IDBInsert Insert();

		/// <summary>
		/// Creates a new CREATE TABLE command builder for a given database dialect. The constructed command does nothing if table already exists.
		/// </summary>
		/// <returns>New CREATE TABLE command builder.</returns>
		IDBCreateTable CreateTable();

		/// <summary>
		/// Creates a new CREATE COLUMN command builder for a given database dialect. The constructed command does nothing if column already exists.
		/// </summary>
		/// <returns>New CREATE COLUMN command builder.</returns>
		IDBCreateColumn CreateColumn();

		/// <summary>
		/// Creates a new CREATE FOREIGN KEY command builder for a given database dialect. The constructed command does nothing if foreign key already exists.
		/// </summary>
		/// <returns>New CREATE FOREIGN KEY command builder.</returns>
		IDBCreateForeignKey CreateForeignKey();

		/// <summary>
		/// Creates a new CREATE INDEX command builder for a given database dialect. The constructed command does nothing if index already exists.
		/// </summary>
		/// <returns>New CREATE INDEX command builder.</returns>
		IDBCreateIndex CreateIndex();

		/// <summary>
		/// Creates a new expression representing a data type in a given database dialect.
		/// </summary>
		/// <param name="type">Data type to create expression for.</param>
		/// <param name="length">Optional data length.</param>
		/// <param name="precision">Optional data precision.</param>
		/// <returns>New expression representing a given data type.</returns>
		IDBExpression Type(DbType type, int length = 0, int precision = 0);

		/// <summary>
		/// Creates new NULL expression.
		/// </summary>
		/// <returns>New NULL expression.</returns>
		IDBExpression Null();

		/// <summary>
		/// Creates a new comment (non-functional part of command).
		/// </summary>
		/// <param name="value">Comment text.</param>
		/// <returns>New comment expression</returns>
		IDBComment Comment(string value);

		/// <summary>
		/// Creates new expression for a given constant value.
		/// </summary>
		/// <param name="value">Constant value to create expression for.</param>
		/// <returns>New constant expression.</returns>
		IDBExpression Const(long value);

		/// <summary>
		/// Creates a new expression for a given constant value.
		/// </summary>
		/// <param name="value">Constant value to create expression for.</param>
		/// <returns>New constant expression.</returns>
		IDBExpression Const(string value);

		/// <summary>
		/// Creates a new expression for a given constant value.
		/// </summary>
		/// <param name="value">Constant value to create expression for.</param>
		/// <returns>New constant expression.</returns>
		IDBExpression Const(double value);

		/// <summary>
		/// Creates a new expression for a given constant value.
		/// </summary>
		/// <param name="value">Constant value to create expression for.</param>
		/// <returns>New constant expression.</returns>
		IDBExpression Const(decimal value);

		/// <summary>
		/// Creates a new expression for a given constant value.
		/// </summary>
		/// <param name="value">Constant value to create expression for.</param>
		/// <returns>New constant expression.</returns>
		IDBExpression Const(DateTime value);

		/// <summary>
		/// Creates a new expression for a given constant value.
		/// </summary>
		/// <param name="value">Constant value to create expression for.</param>
		/// <returns>New constant expression.</returns>
		IDBExpression Const(byte[] value);

		/// <summary>
		/// Creates a new expression for a given database identifier.
		/// </summary>
		/// <param name="name">Identifier name to create expression for.</param>
		/// <returns>New identifier expression.</returns>
		IDBIdentifier Identifier(string name);

		/// <summary>
		/// Creates a new expression for a given multipart database identifier.
		/// </summary>
		/// <param name="names">Identifier names to create expression for.</param>
		/// <returns>New multipart identifier expression.</returns>
		IDBIdentifier Identifier(params string[] names);

		/// <summary>
		/// Creates a new expression from a given text.
		/// </summary>
		/// <param name="expression">Text to create expression from.</param>
		/// <returns>New expression for a given text.</returns>
		IDBExpression Expression(string expression);

		/// <summary>
		/// Creates a new comparison expression between given expressions.
		/// </summary>
		/// <param name="lexpr">Left value of a comparison.</param>
		/// <param name="comparison">Type of comparison between values.</param>
		/// <param name="rexpr">Right value of a comparison.</param>
		/// <returns>New expression representing comparison between given expressions.</returns>
		IDBExpression Comparison(IDBExpression lexpr, DBExpressionComparison comparison, IDBExpression rexpr);

		/// <summary>
		/// Creates a new expression representing a database command parameter.
		/// </summary>
		/// <param name="name">Name of the parameter.</param>
		/// <returns>New expression for a given parameter name.</returns>
		IDBExpression Parameter(string name);

		/// <summary>
		/// Creates new array expression from provided expressions.
		/// </summary>
		/// <param name="values">Expressions representing elements of array to create.</param>
		/// <returns>Expression represeting array of provided expression values.</returns>
		IDBExpression Array(IDBExpression[] values);
	}

	/// <summary>
	/// Type of binary comparison to perform.
	/// </summary>
	public enum DBExpressionComparison
	{
		/// <summary>
		/// Equality (=) comparison.
		/// </summary>
		Equal,

		/// <summary>
		/// Inequality (&lt;&gt;) comparison.
		/// </summary>
		NotEqual,

		/// <summary>
		/// Greater than (&gt;) comparison.
		/// </summary>
		Greater,

		/// <summary>
		/// Not less than (&gt;=) comparison.
		/// </summary>
		GreaterOrEqual,

		/// <summary>
		/// Less than (&lt;) comparison.
		/// </summary>
		Less,

		/// <summary>
		/// Not greater than (&lt;=) comparison.
		/// </summary>
		LessOrEqual,

		/// <summary>
		/// Set equality (IN) comparison.
		/// </summary>
		In
	}

	/// <summary>
	/// Fragment of database command for use in database command clauses.
	/// </summary>
	public interface IDBExpression
	{
		/// <summary>
		/// Returns internal value from which the expression is constructed. For internal use only.
		/// </summary>
		string RawValue { get; }

		/// <summary>
		/// Appends expression to a given StringBuilder. For internal use only.
		/// </summary>
		/// <param name="sb">StringBuilder to append the expression to.</param>
		void Append(StringBuilder sb);
	}

	/// <summary>
	/// Comment in database command.
	/// </summary>
	public interface IDBComment : IDBExpression
	{
	}

	/// <summary>
	/// Database object identifier.
	/// </summary>
	public interface IDBIdentifier : IDBExpression
	{
	}

	/// <summary>
	/// Multipart (fully-qualified) database object identifier.
	/// </summary>
	public interface IDBDottedIdentifier : IDBIdentifier
	{
		/// <summary>
		/// Array of identifier parts.
		/// </summary>
		string[] Fragments { get; }
	}

	/// <summary>
	/// Generic database command.
	/// </summary>
	public interface IDBCommand
	{
		/// <summary>
		/// Command builder used to construct this command.
		/// </summary>
		IDBCommandBuilder Builder { get; }

		/// <summary>
		/// Annotates the command with provided comment.
		/// </summary>
		/// <param name="tag">Comment to append to database command.</param>
		void Tag(IDBComment tag);
	}

	/// <summary>
	/// Generic database command.
	/// </summary>
	/// <typeparam name="TCommand">Concrete interface of the command.</typeparam>
	public interface IDBCommand<TCommand> : IDBCommand where TCommand : IDBCommand<TCommand>
	{
		/// <summary>
		/// Creates a new copy of this command.
		/// </summary>
		/// <returns>New exact, independent copy of this command.</returns>
		TCommand Clone();
	}

	/// <summary>
	/// Generic database command with WHERE clause containing groups joined by OR operator of conditions joined by AND operator.
	/// </summary>
	/// <typeparam name="TCommand">Concrete interface of the command.</typeparam>
	public interface IDBCommandWithWhere<TCommand> : IDBCommand<TCommand> where TCommand : IDBCommandWithWhere<TCommand>
	{
		/// <summary>
		/// Adds WHERE clause to the command; multiple clauses within a group are joined by AND operator.
		/// </summary>
		/// <param name="expression">WHERE expression to add.</param>
		void Where(IDBExpression expression);

		/// <summary>
		/// Starts a new group of clauses joined by AND operator separated from previous clauses by OR operator.
		/// </summary>
		void StartWhereGroup();
	}

	/// <summary>
	/// Database SELECT command.
	/// </summary>
	public interface IDBSelect : IDBCommandWithWhere<IDBSelect>
	{
		/// <summary>
		/// Changes command type to SELECT FOR UPDATE.
		/// </summary>
		void ForUpdate();

		/// <summary>
		/// Appends LIMIT clause to the command.
		/// </summary>
		/// <param name="limit">Limit constant value.</param>
		void Limit(int limit);

		/// <summary>
		/// Adds a column to SELECT clause of the command.
		/// </summary>
		/// <param name="expression">Expression to use as a column value.</param>
		/// <param name="alias">Optional column name.</param>
		void Column(IDBExpression expression, IDBIdentifier alias = null);

		/// <summary>
		/// Adds a subquery to SELECT clause of the command.
		/// </summary>
		/// <param name="expression">Subquery to use as a column value.</param>
		/// <param name="alias">Optional column name.</param>
		void Subquery(IDBExpression expression, IDBIdentifier alias = null);

		/// <summary>
		/// Sets FROM clause of the command to given table.
		/// </summary>
		/// <param name="table">Main table name.</param>
		/// <param name="alias">Optional main table alias.</param>
		void From(IDBIdentifier table, IDBIdentifier alias = null);

		/// <summary>
		/// Sets FROM clause of the command to a given subquery expression.
		/// </summary>
		/// <param name="expression">Subquery to use as a main table.</param>
		/// <param name="alias">Optional main table alias.</param>
		void FromSubquery(IDBExpression expression, IDBIdentifier alias = null);

		/// <summary>
		/// Adds a JOIN clause to the command.
		/// </summary>
		/// <param name="table">Remote table to join to.</param>
		/// <param name="expression">Expression on which the join is made.</param>
		/// <param name="alias">Remote table alias.</param>
		/// <param name="outer">Specifies whether OUTER or INNER JOIN should be produced.</param>
		void Join(IDBIdentifier table, IDBExpression expression, IDBIdentifier alias = null, bool outer = true);

		/// <summary>
		/// Adds GROUP BY clause to the command. Clause is added after existing (if any) GROUP BY clauses.
		/// </summary>
		/// <param name="expression">Expression to group by.</param>
		void GroupBy(IDBExpression expression);

		/// <summary>
		/// Adds ORDER BY clause to the command. Clause is added after existing (if any) ORDER BY clauses.
		/// </summary>
		/// <param name="expression">Expression to sort by.</param>
		/// <param name="descending">Determines whether sort order should be descending or ascending.</param>
		void OrderBy(IDBExpression expression, bool descending = false);
	}

	/// <summary>
	/// Database UPDATE command.
	/// </summary>
	public interface IDBUpdate : IDBCommandWithWhere<IDBUpdate>
	{
		/// <summary>
		/// Sets table to update.
		/// </summary>
		/// <param name="table">Identifier of a table to update.</param>
		void Table(IDBIdentifier table);

		/// <summary>
		/// Adds SET clause to the command.
		/// </summary>
		/// <param name="field">Column name to update.</param>
		/// <param name="expression">Value of the column to set.</param>
		void Set(IDBIdentifier field, IDBExpression expression);
	}

	/// <summary>
	/// Database DELETE command.
	/// </summary>
	public interface IDBDelete : IDBCommandWithWhere<IDBDelete>
	{
		/// <summary>
		/// Sets table to delete from.
		/// </summary>
		/// <param name="table">Identifier of a table to delete from.</param>
		void Table(IDBIdentifier table);
	}

	/// <summary>
	/// Database INSERT command.
	/// </summary>
	public interface IDBInsert : IDBCommand<IDBInsert>
	{
		/// <summary>
		/// Returns database query to retrieve value of primary key (ID) assigned by last INSERT command. Returns null if value can be obtained directly as a result of INSERT command execution.
		/// </summary>
		string GetLastID { get; }

		/// <summary>
		/// Sets table to insert to.
		/// </summary>
		/// <param name="table">Identifier of table to insert to.</param>
		void Table(IDBIdentifier table);

		/// <summary>
		/// Sets table's primary key column name for returning inserted value.
		/// </summary>
		/// <param name="primaryKey">Identifier of table's primary key column.</param>
		void PrimaryKeyReturn(IDBIdentifier primaryKey);

		/// <summary>
		/// Adds column/value pair to insert.
		/// </summary>
		/// <param name="column">Column to insert value to.</param>
		/// <param name="value">Value to insert.</param>
		void Column(IDBIdentifier column, IDBExpression value);

		/// <summary>
		/// Adds value of primary key to insert.
		/// </summary>
		/// <param name="primaryKey">Name of column containing primary key.</param>
		/// <param name="value">Value of primary key to insert.</param>
		void PrimaryKeyInsert(IDBIdentifier primaryKey, IDBExpression value);
	}

	/// <summary>
	/// Database CREATE TABLE command.
	/// </summary>
	public interface IDBCreateTable : IDBCommand
	{
		/// <summary>
		/// Sets table to insert to.
		/// </summary>
		/// <param name="table">Identifier of table to insert to.</param>
		void Table(IDBIdentifier table);

		/// <summary>
		/// Sets primary key (ID) column name.
		/// </summary>
		/// <param name="column">Primary key (ID) column name.</param>
		void PrimaryKey(IDBIdentifier column);

		/// <summary>
		/// Marks primary key (ID) column as auto-incremented by database engine.
		/// </summary>
		void AutoIncrement();
	}

	/// <summary>
	/// Database CREATE TABLE command.
	/// </summary>
	public interface IDBCreateColumn : IDBCommand, IDBForeignKey
	{
		/// <summary>
		/// Sets table name to add column to.
		/// </summary>
		/// <param name="table">Identifier of table to add column to.</param>
		void Table(IDBIdentifier table);

		/// <summary>
		/// Sets name of the column to add.
		/// </summary>
		/// <param name="name">Identifier of the column to add.</param>
		void Name(IDBIdentifier name);

		/// <summary>
		/// Sets type of the column to add.
		/// </summary>
		/// <param name="type">Identifier of column data type</param>
		void Type(IDBExpression type);

		/// <summary>
		/// Sets default value for a column.
		/// </summary>
		/// <param name="defaultValue">Default value expression for a column.</param>
		void DefaultValue(IDBExpression defaultValue);

		/// <summary>
		/// Sets NOT NULL clause for a column.
		/// </summary>
		void NotNull();
	}

	/// <summary>
	/// FOREIGN KEY constraint definition.
	/// </summary>
	public interface IDBForeignKey
	{
		/// <summary>
		/// Sets name of the foreign key constraint to create.
		/// </summary>
		/// <param name="name">Identifier of the foreign key constraint co create.</param>
		void ConstraintName(IDBIdentifier name);

		/// <summary>
		/// Sets name of table referenced by foreign key.
		/// </summary>
		/// <param name="targetTable">Identifier of table referenced by foreign key.</param>
		void TargetTable(IDBIdentifier targetTable);

		/// <summary>
		/// Sets name of column referenced by foreign key.
		/// </summary>
		/// <param name="targetColumn">Identifier of primary key column referenced by foreign key.</param>
		void TargetColumn(IDBIdentifier targetColumn);

		/// <summary>
		/// Marks foreign key as ON DELETE SET NULL.
		/// </summary>
		void SetNull();

		/// <summary>
		/// Marks foreign key as ON DELETE CASCADE.
		/// </summary>
		void Cascade();
	}

	/// <summary>
	/// Database CREATE FOREIGN KEY command.
	/// </summary>
	public interface IDBCreateForeignKey : IDBCommand, IDBForeignKey
	{
		/// <summary>
		/// Sets table name containing the column to add constraint to.
		/// </summary>
		/// <param name="table">Identifier of table containing column to add constraint to.</param>
		void Table(IDBIdentifier table);

		/// <summary>
		/// Sets column name to add constraint to.
		/// </summary>
		/// <param name="column">Identifier of a column to add constraint to.</param>
		void Column(IDBIdentifier column);

		/// <summary>
		/// Appends foreign key constraint definition for use when creating column.
		/// </summary>
		/// <param name="sb">StringBuilder constructing CREATE COLUMN command.</param>
		void AppendColumnDefinition(StringBuilder sb);
	}

	/// <summary>
	/// Database CREATE INDEX command.
	/// </summary>
	public interface IDBCreateIndex : IDBCommand
	{
		/// <summary>
		/// Sets name of the index to create.
		/// </summary>
		/// <param name="name">Index identifier/</param>
		void Name(IDBIdentifier name);

		/// <summary>
		/// Sets table name to create index on.
		/// </summary>
		/// <param name="table">Table identifier to create index on.</param>
		void Table(IDBIdentifier table);

		/// <summary>
		/// Adds column to the index key.
		/// </summary>
		/// <param name="column">Identifier of a index key column.</param>
		void Column(IDBIdentifier column);

		/// <summary>
		/// Adds non-key column to the index for creating covering index.
		/// </summary>
		/// <param name="column">Identifier of a column to include in index leaves.</param>
		void Include(IDBIdentifier column);

		/// <summary>
		/// Marks index as UNIQUE.
		/// </summary>
		void Unique();
	}
}
