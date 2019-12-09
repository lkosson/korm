using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IDB.
	/// </summary>
	public static class DBExtensions
	{
		/// <summary>
		/// Adds @P0, @P1, ... @Pn parameters to a given DB command with provided values.
		/// Each subsequent call starts numbering parameters from where it stopped last time.
		/// </summary>
		/// <param name="db">IDB provider for DB command manipulation.</param>
		/// <param name="command">DB command to add parameters to.</param>
		/// <param name="values">Parameter values to add to DB command.</param>
		public static void AddParameters(this IDB db, DbCommand command, IEnumerable<object> values)
		{
			if (values == null) return;
			int i = command.Parameters.Count;
			foreach (var value in values)
			{
				db.AddParameter(command, "@P" + i.ToString(), value);
				i++;
			}
		}

		private static T CreateAndExecuteCommand<T>(IDB db, string command, IEnumerable<object> parameters, Func<DbCommand, T> executor)
		{
			using (var cmd = db.CreateCommand(command))
			{
				db.AddParameters(cmd, parameters);
				return executor(cmd);
			}
		}

		private static async Task<T> CreateAndExecuteCommandAsync<T>(IDB db, string command, IEnumerable<object> parameters, Func<DbCommand, Task<T>> executor)
		{
			using (var cmd = db.CreateCommand(command))
			{
				db.AddParameters(cmd, parameters);
				return await executor(cmd);
			}
		}

		/// <summary>
		/// Creates and executes non-query DB command with parameters @P0, @P1, ..., @Pn with given values.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>Number of affected database rows or result code from DB command.</returns>
		public static int ExecuteNonQuery(this IDB db, string command, params object[] parameters)
		{
			return CreateAndExecuteCommand(db, command, parameters, db.ExecuteNonQuery);
		}

		/// <summary>
		/// Creates and executes non-query DB command with parameters @P0, @P1, ..., @Pn with given values.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>Number of affected database rows or result code from DB command.</returns>
		public static int ExecuteNonQuery(this IDB db, string command, IEnumerable<object> parameters)
		{
			return CreateAndExecuteCommand(db, command, parameters, db.ExecuteNonQuery);
		}

		/// <summary>
		/// Asynchronous version of ExecuteQuery.
		/// Creates and executes non-query DB command with parameters @P0, @P1, ..., @Pn with given values.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning number of affected database rows or result code from DB command.</returns>
		public static Task<int> ExecuteNonQueryAsync(this IDB db, string command, params object[] parameters)
		{
			return CreateAndExecuteCommandAsync(db, command, parameters, db.ExecuteNonQueryAsync);
		}

		/// <summary>
		/// Asynchronous version of ExecuteQuery.
		/// Creates and executes non-query DB command with parameters @P0, @P1, ..., @Pn with given values.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning number of affected database rows or result code from DB command.</returns>
		public static Task<int> ExecuteNonQueryAsync(this IDB db, string command, IEnumerable<object> parameters)
		{
			return CreateAndExecuteCommandAsync(db, command, parameters, db.ExecuteNonQueryAsync);
		}

		/// <summary>
		/// Asynchronous version of ExecuteQueryFirst.
		/// Executes DB query and returns first row of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">DB command to execute.</param>
		/// <returns>Task representing asynchronous operation returning first row of a result or null when query produces no result.</returns>
		public static async Task<IRow> ExecuteQueryFirstAsync(this IDB db, DbCommand command)
		{
			var result = await db.ExecuteQueryAsync(command, 1);
			return result.FirstOrDefault();
		}

		/// <summary>
		/// Executes DB query and returns array of rows representing the query result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">DB command to execute.</param>
		/// <returns>Array (possibly empty) of a query result.</returns>
		private static IReadOnlyList<IRow> ExecuteQueryAll(this IDB db, DbCommand command)
		{
			return db.ExecuteQuery(command);
		}

		private static Task<IReadOnlyList<IRow>> ExecuteQueryAllAsync(this IDB db, DbCommand command)
		{
			return db.ExecuteQueryAsync(command);
		}

		/// <summary>
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>Array (possibly empty) of a query result.</returns>
		public static IReadOnlyList<IRow> ExecuteQuery(this IDB db, string command, params object[] parameters)
		{
			return CreateAndExecuteCommand(db, command, parameters, db.ExecuteQueryAll);
		}

		/// <summary>
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>Array (possibly empty) of a query result.</returns>
		public static IReadOnlyList<IRow> ExecuteQuery(this IDB db, string command, IEnumerable<object> parameters)
		{
			return CreateAndExecuteCommand(db, command, parameters, db.ExecuteQueryAll);
		}

		/// <summary>
		/// Asynchronous version of ExecuteQuery.
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning an array (possibly empty) of a query result.</returns>
		public static Task<IReadOnlyList<IRow>> ExecuteQueryAsync(this IDB db, string command, params object[] parameters)
		{
			return CreateAndExecuteCommandAsync(db, command, parameters, db.ExecuteQueryAllAsync);
		}

		/// <summary>
		/// Asynchronous version of ExecuteQuery.
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning an array (possibly empty) of a query result.</returns>
		public static Task<IReadOnlyList<IRow>> ExecuteQueryAsync(this IDB db, string command, IEnumerable<object> parameters)
		{
			return CreateAndExecuteCommandAsync(db, command, parameters, db.ExecuteQueryAllAsync);
		}

		/// <summary>
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns one-time reader of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>One-time reader of rows of a query result.</returns>
		public static DbDataReader ExecuteReader(this IDB db, string command, params object[] parameters)
		{
			return CreateAndExecuteCommand(db, command, parameters, db.ExecuteReader);
		}

		/// <summary>
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns one-time reader of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>One-time reader of rows of a query result.</returns>
		public static DbDataReader ExecuteReader(this IDB db, string command, IEnumerable<object> parameters)
		{
			return CreateAndExecuteCommand(db, command, parameters, db.ExecuteReader);
		}

		/// <summary>
		/// Asynchronous version of ExecuteReader.
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns one-time reader of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning an one-time reader of rows of a query result.</returns>
		public static Task<DbDataReader> ExecuteReaderAsync(this IDB db, string command, params object[] parameters)
		{
			return CreateAndExecuteCommand(db, command, parameters, db.ExecuteReaderAsync);
		}

		/// <summary>
		/// Asynchronous version of ExecuteReader.
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns one-time reader of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning an one-time reader of rows of a query result.</returns>
		public static Task<DbDataReader> ExecuteReaderAsync(this IDB db, string command, IEnumerable<object> parameters)
		{
			return CreateAndExecuteCommand(db, command, parameters, db.ExecuteReaderAsync);
		}
	}
}
