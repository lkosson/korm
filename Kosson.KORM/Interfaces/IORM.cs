﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Object-relational mapper for IRecords.
	/// </summary>
	public interface IORM
	{
		/// <summary>
		/// Updates structure of database by creating tables, columns, constraints and indices for specified record types.
		/// </summary>
		/// <param name="types">Types for which database structure should be updated.</param>
		void CreateTables(IEnumerable<Type> types);

		/// <summary>
		/// Creates command for retrieval of records of a given type from backing database table.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to retrieve.</typeparam>
		/// <returns>Command to retrieve records.</returns>
		IORMSelect<TRecord> Select<TRecord>() where TRecord : class, IRecord, new();

		/// <summary>
		/// Inserts specified records to its database backing table and assigns primary key (ID) property value.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to insert.</typeparam>
		/// <returns>Number of records inserted.</returns>
		IORMInsert<TRecord> Insert<TRecord>() where TRecord : IRecord;

		/// <summary>
		/// Creates a command for updating records of a given type in its backing database table.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to update.</typeparam>
		/// <returns>Command to update records in backing database table.</returns>
		IORMUpdate<TRecord> Update<TRecord>() where TRecord : IRecord;

		/// <summary>
		/// Creates a command for deleting records of a given type in its backing database table.
		/// </summary>
		/// <typeparam name="TRecord">Type of records to delete.</typeparam>
		/// <returns>Command to delete records in backing database table.</returns>
		IORMDelete<TRecord> Delete<TRecord>() where TRecord : IRecord;
	}

	/// <summary>
	/// Abstract ORM command.
	/// </summary>
	public interface IORMCommand
	{
		/// <summary>
		/// IDB provider to use for command construction and execution.
		/// </summary>
		IDB DB { get; }

		/// <summary>
		/// Adds new database parameter with autogenerated name and specified value to the command.
		/// </summary>
		/// <param name="value">Value of the parameter to add.</param>
		/// <returns>Expression referencing the parameter.</returns>
		IDBExpression Parameter(object value);

		/// <summary>
		/// Creates new array expression from provided values.
		/// </summary>
		/// <param name="values">Elements of array to create.</param>
		/// <returns>Expression represeting array of provided values.</returns>
		IDBExpression Array(object[] values);

		/// <summary>
		/// Creates a new expression referencing specified database column name or property.
		/// </summary>
		/// <param name="name">Name of the column or property to reference.</param>
		/// <returns>Expression referencing given column or property.</returns>
		IDBIdentifier Field(string name);
	}

	/// <summary>
	/// Abstract ORM command.
	/// </summary>
	/// <typeparam name="TCommand">Derived concrete interface of the command.</typeparam>
	public interface IORMCommand<TCommand> : IORMCommand
		where TCommand : IORMCommand<TCommand>
	{
		/// <summary>
		/// Tags the command with a provided comment.
		/// </summary>
		/// <param name="comment">Comment to include in database command.</param>
		/// <returns>Original command with tag comment added to it.</returns>
		TCommand Tag(IDBComment comment);
	}

	/// <summary>
	/// Abstract ORM command with WHERE clause.
	/// </summary>
	/// <typeparam name="TCommand">Derived concrete interface of the command.</typeparam>
	public interface IORMNarrowableCommand<TCommand> : IORMCommand<TCommand>
		where TCommand : IORMNarrowableCommand<TCommand>
	{
		/// <summary>
		/// Appends WHERE clause to the command; multiple clauses are joined by AND operator.
		/// </summary>
		/// <param name="expression">WHERE expression to add.</param>
		/// <returns>Original command with WHERE clause added to it.</returns>
		TCommand Where(IDBExpression expression);

		/// <summary>
		/// Starts a new group of clauses joined by AND operator separated from previous clauses by OR operator.
		/// </summary>
		TCommand Or();
	}

	/// <summary>
	/// Abstract ORM command retrieving records from database.
	/// </summary>
	/// <typeparam name="TRecord">Type of records returned.</typeparam>
	public interface IORMQueryCommand<TRecord> where TRecord : IRecord
	{
		/// <summary>
		/// Executes the command and returns records built from its result.
		/// </summary>
		/// <returns></returns>
		IReadOnlyCollection<TRecord> Execute();

		/// <summary>
		/// Asynchronous version of Execute. Executes the command and returns records built from its result.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task<IReadOnlyCollection<TRecord>> ExecuteAsync();

		/// <summary>
		/// Executes the command and returns records build from its results as one-time, on-the-fly enumerable set.
		/// </summary>
		/// <returns></returns>
		IORMReader<TRecord> ExecuteReader();

		/// <summary>
		/// Asynchronous version of ExecuteReader.
		/// Executes the command and returns records build from its results as one-time, on-the-fly enumerable set.
		/// </summary>
		/// <returns></returns>
		Task<IORMReader<TRecord>> ExecuteReaderAsync();
	}

	/// <summary>
	/// Abstract ORM command modifying records in database.
	/// </summary>
	/// <typeparam name="TRecord"></typeparam>
	public interface IORMNonQueryCommand<TRecord> where TRecord : IRecord
	{
		/// <summary>
		/// Executes the command and returns number of affected database records.
		/// </summary>
		/// <returns>Number of affected records.</returns>
		int Execute();

		/// <summary>
		/// Asynchronous version of Execute.
		/// Executes the command and returns number of affected database records.
		/// </summary>
		/// <returns>Task representing asynchronous operation returning number of affected records.</returns>
		Task<int> ExecuteAsync();

		/// <summary>
		/// Executes the command on provided records.
		/// </summary>
		/// <param name="records">Records to affect.</param>
		/// <returns>Number of affected records.</returns>
		int Records(IEnumerable<TRecord> records);

		/// <summary>
		/// Asynchronous version of Records.
		/// Executes the command on provided records.
		/// </summary>
		/// <param name="records">Records to affect.</param>
		/// <returns>Task representing asynchronous operation returning number of affected records.</returns>
		Task<int> RecordsAsync(IEnumerable<TRecord> records);
	}

	/// <summary>
	/// ORM command for retrieving records of a specified type from its backing database table.
	/// </summary>
	/// <typeparam name="TRecord">Type of records to retrieve.</typeparam>
	public interface IORMSelect<TRecord> : IORMCommand<IORMSelect<TRecord>>, IORMNarrowableCommand<IORMSelect<TRecord>>, IORMQueryCommand<TRecord> where TRecord : IRecord
	{
		/// <summary>
		/// Changes command behavior to SELECT FOR UPDATE mode.
		/// </summary>
		/// <returns>Original command with SELECT mode changed.</returns>
		IORMSelect<TRecord> ForUpdate();

		/// <summary>
		/// Sets new LIMIT clause of the command to narrow records returned to first given number of records.
		/// </summary>
		/// <param name="limit">Number of records to return.</param>
		/// <returns>Original command with LIMIT clause applied.</returns>
		IORMSelect<TRecord> Limit(int limit);

		/// <summary>
		/// Adds ORDER BY clause to the command to sort resulting rows by a given field.
		/// </summary>
		/// <param name="expression">Database column to sort results by.</param>
		/// <param name="descending">Determines whether order should be descending or ascending.</param>
		/// <returns>Original command with ORDER BY clause added to it.</returns>
		IORMSelect<TRecord> OrderBy(IDBExpression expression, bool descending = false);
	}

	/// <summary>
	/// ORM command for inserting records into database.
	/// </summary>
	/// <typeparam name="TRecord">Type of records to delete</typeparam>
	public interface IORMInsert<TRecord> : IORMCommand<IORMInsert<TRecord>> where TRecord : IRecord
	{
		/// <summary>
		/// Executes the command on provided records.
		/// </summary>
		/// <param name="records">Records to affect.</param>
		/// <returns>Number of affected records.</returns>
		int Records(IEnumerable<TRecord> records);

		/// <summary>
		/// Asynchronous version of Records.
		/// Executes the command on provided records.
		/// </summary>
		/// <param name="records">Records to affect.</param>
		/// <returns>Task representing asynchronous operation returning number of affected records.</returns>
		Task<int> RecordsAsync(IEnumerable<TRecord> records);

		/// <summary>
		/// Forces use of primary key value (ID) provided in record instead of database-assigned ones.
		/// </summary>
		/// <returns>Original command modified to include the primary key value.</returns>
		IORMInsert<TRecord> WithProvidedID();
	}

	/// <summary>
	/// ORM command for deleting records of a specified type from its backing database table.
	/// </summary>
	/// <typeparam name="TRecord">Type of records to delete</typeparam>
	public interface IORMDelete<TRecord> : IORMCommand<IORMDelete<TRecord>>, IORMNarrowableCommand<IORMDelete<TRecord>>, IORMNonQueryCommand<TRecord> where TRecord : IRecord
	{
	}

	/// <summary>
	/// ORM command for modifying records of a specified type from its backing database table.
	/// </summary>
	/// <typeparam name="TRecord">Type of records to modify.</typeparam>
	public interface IORMUpdate<TRecord> : IORMCommand<IORMUpdate<TRecord>>, IORMNarrowableCommand<IORMUpdate<TRecord>>, IORMNonQueryCommand<TRecord> where TRecord : IRecord
	{
		/// <summary>
		/// Adds SET clause to the command, changing given column to new value.
		/// </summary>
		/// <param name="field">Database column to change.</param>
		/// <param name="value">Expression determining new value of a column.</param>
		/// <returns>Original command with SET clause added to it.</returns>
		IORMUpdate<TRecord> Set(IDBIdentifier field, IDBExpression value);
	}
}
