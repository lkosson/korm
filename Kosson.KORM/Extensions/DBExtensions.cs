using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IDB.
	/// </summary>
	public static class DBExtensions
	{
		private static string[]? parameterNameCache;

		private static string GetParameterName(IDB db, int num)
		{
			var cache = parameterNameCache;
			if (cache == null)
			{
				cache = new string[8];
				for (int i = 0; i < cache.Length; i++) cache[i] = db.CommandBuilder.ParameterPrefix + "P" + i.ToString();
				parameterNameCache = cache;
			}
			if (num >= cache.Length) return db.CommandBuilder.ParameterPrefix + "P" + num.ToString();
			return cache[num];
		}

		/// <summary>
		/// Adds @P0, @P1, ... @Pn parameters to a given DB command with provided values.
		/// Each subsequent call starts numbering parameters from where it stopped last time.
		/// </summary>
		/// <param name="db">IDB provider for DB command manipulation.</param>
		/// <param name="command">DB command to add parameters to.</param>
		/// <param name="values">Parameter values to add to DB command.</param>
		public static void AddParameters(this IDB db, DbCommand command, IEnumerable<object?> values)
		{
			if (values == null) return;
			int i = command.Parameters.Count;
			foreach (var value in values)
			{
				db.AddParameter(command, GetParameterName(db, i), value);
				i++;
			}
		}

		private static T CreateAndExecuteCommand<T>(IDB db, string command, IEnumerable<object?> parameters, Func<DbCommand, T> executor)
		{
			using var cmd = db.CreateCommand(command);
			db.AddParameters(cmd, parameters);
			return executor(cmd);
		}

		private static async Task<T> CreateAndExecuteCommandAsync<T>(IDB db, string command, IEnumerable<object?> parameters, Func<DbCommand, Task<T>> executor)
		{
			using var cmd = db.CreateCommand(command);
			db.AddParameters(cmd, parameters);
			return await executor(cmd);
		}

		private static T FormatQueryAndExecute<T>(IDB db, FormattableString command, Func<IDB, string, object?[], T> executor)
		{
			var args = new string[command.ArgumentCount];
			var vals = new object?[command.ArgumentCount];
			for (int i = 0; i < args.Length; i++) args[i] = GetParameterName(db, i);
			for (int i = 0; i < vals.Length; i++) vals[i] = command.GetArgument(i);
			var sql = String.Format(command.Format, args);
			return executor(db, sql, vals);
		}

		/// <summary>
		/// Creates and executes non-query DB command with parameters @P0, @P1, ..., @Pn with given values.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>Number of affected database rows or result code from DB command.</returns>
		public static int ExecuteNonQueryRaw(this IDB db, string command, params object?[] parameters)
			=> CreateAndExecuteCommand(db, command, parameters, db.ExecuteNonQuery);

		/// <summary>
		/// Creates and executes non-query DB command with parameters @P0, @P1, ..., @Pn with given values.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>Number of affected database rows or result code from DB command.</returns>
		public static int ExecuteNonQueryRaw(this IDB db, string command, IEnumerable<object?> parameters)
			=> CreateAndExecuteCommand(db, command, parameters, db.ExecuteNonQuery);

		/// <summary>
		/// Creates and executes non-query DB command based on interpolated string.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <returns>Number of affected database rows or result code from DB command.</returns>
		public static int ExecuteNonQuery(this IDB db, FormattableString command)
			=> FormatQueryAndExecute(db, command, ExecuteNonQueryRaw);

		/// <summary>
		/// Asynchronous version of ExecuteQuery.
		/// Creates and executes non-query DB command with parameters @P0, @P1, ..., @Pn with given values.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning number of affected database rows or result code from DB command.</returns>
		public static Task<int> ExecuteNonQueryRawAsync(this IDB db, string command, params object?[] parameters)
			=> CreateAndExecuteCommandAsync(db, command, parameters, db.ExecuteNonQueryAsync);

		/// <summary>
		/// Asynchronous version of ExecuteNonQuery.
		/// Creates and executes non-query DB command with parameters @P0, @P1, ..., @Pn with given values.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning number of affected database rows or result code from DB command.</returns>
		public static Task<int> ExecuteNonQueryRawAsync(this IDB db, string command, IEnumerable<object?> parameters)
			=> CreateAndExecuteCommandAsync(db, command, parameters, db.ExecuteNonQueryAsync);

		/// <summary>
		/// Asynchronous version of ExecuteNonQuery.
		/// Creates and executes non-query DB command based on interpolated string.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a command to execute.</param>
		/// <returns>Number of affected database rows or result code from DB command.</returns>
		public static Task<int> ExecuteNonQueryAsync(this IDB db, FormattableString command)
			=> FormatQueryAndExecute(db, command, ExecuteNonQueryRawAsync);

		/// <summary>
		/// Executes DB query and returns array of rows representing the query result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">DB command to execute.</param>
		/// <returns>Array (possibly empty) of a query result.</returns>
		private static IReadOnlyList<IRow> ExecuteQueryAll(this IDB db, DbCommand command)
			=> db.ExecuteQuery(command);

		private static Task<IReadOnlyList<IRow>> ExecuteQueryAllAsync(this IDB db, DbCommand command)
			=> db.ExecuteQueryAsync(command);

		/// <summary>
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>Array (possibly empty) of a query result.</returns>
		public static IReadOnlyList<IRow> ExecuteQueryRaw(this IDB db, string command, params object?[] parameters)
			=> CreateAndExecuteCommand(db, command, parameters, db.ExecuteQueryAll);

		/// <summary>
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>Array (possibly empty) of a query result.</returns>
		public static IReadOnlyList<IRow> ExecuteQueryRaw(this IDB db, string command, IEnumerable<object?> parameters)
			=> CreateAndExecuteCommand(db, command, parameters, db.ExecuteQueryAll);

		/// <summary>
		/// Creates and executes DB query based on interpolated string and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <returns>Array (possibly empty) of a query result.</returns>
		public static IReadOnlyList<IRow> ExecuteQuery(this IDB db, FormattableString command)
			=> FormatQueryAndExecute(db, command, ExecuteQueryRaw);

		/// <summary>
		/// Asynchronous version of ExecuteQuery.
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning an array (possibly empty) of a query result.</returns>
		public static Task<IReadOnlyList<IRow>> ExecuteQueryRawAsync(this IDB db, string command, params object?[] parameters)
			=> CreateAndExecuteCommandAsync(db, command, parameters, db.ExecuteQueryAllAsync);

		/// <summary>
		/// Asynchronous version of ExecuteQuery.
		/// Creates and executes DB query with parameters @P0, @P1, ..., @Pn with given values and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <param name="parameters">Parameters for the command.</param>
		/// <returns>A task representing asynchronous operation returning an array (possibly empty) of a query result.</returns>
		public static Task<IReadOnlyList<IRow>> ExecuteQueryRawAsync(this IDB db, string command, IEnumerable<object?> parameters)
			=> CreateAndExecuteCommandAsync(db, command, parameters, db.ExecuteQueryAllAsync);

		/// <summary>
		/// Asynchronous version of ExecuteQuery.
		/// Creates and executes DB query based on interpolated string and returns array of rows of a result.
		/// </summary>
		/// <param name="db">IDB provider for DB command creation, manipulation and execution.</param>
		/// <param name="command">Text of a DB query to execute.</param>
		/// <returns>A task representing asynchronous operation returning an array (possibly empty) of a query result.</returns>
		public static Task<IReadOnlyList<IRow>> ExecuteQueryAsync(this IDB db, FormattableString command)
			=> FormatQueryAndExecute(db, command, ExecuteQueryRawAsync);

		// Removed ExecuteReader helpers due to DbCommand early dispose in CreateAndExecuteCommand
	}
}
